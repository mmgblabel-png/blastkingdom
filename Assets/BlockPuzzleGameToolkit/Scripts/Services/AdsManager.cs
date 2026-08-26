// // ©2015 - 2026 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Services.Ads;
using BlockPuzzleGameToolkit.Scripts.Services.Ads.AdUnits;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Services
{
    public class AdsManager : SingletonBehaviour<AdsManager>
    {
        private readonly List<AdSetting> adList = new();
        private readonly List<AdUnit> adUnits = new();
        private EPlatforms platforms;
        private InterstitialSettings interstitialSettings;
        private bool consentInfoUpdateInProgress = false;
        private bool adsInitialized = false;

        public override void Awake()
        {
            base.Awake();
            
            if (!GameManager.instance.GameSettings.enableAds)
            {
                return;
            }

            PrepareAds();
            StartConsentFlow();
        }

        private void PrepareAds()
        {
            platforms = GetPlatform();
            var adElements = Resources.Load<AdsSettings>("Settings/AdsSettings").adProfiles;
            interstitialSettings = Resources.Load<InterstitialSettings>("Settings/AdsInterstitialSettings");
            
            foreach (var t in adElements)
            {
                if (t.platforms == platforms && t.enable)
                {
                    if (Application.isEditor && !t.testInEditor)
                    {
                        continue;
                    }

                    adList.Add(t);
                    foreach (var adElement in t.adElements)
                    {
                        if (!Debug.isDebugBuild && IsGoogleSampleAdUnit(adElement.placementId))
                        {
                            Debug.LogError("A Google sample ad unit is configured for a release build. The unit is disabled until a production ad unit is supplied.");
                            continue;
                        }

                        var adUnit = new AdUnit { PlacementId = adElement.placementId, AdReference = adElement.adReference, AdsHandler = t.adsHandler };
                        adUnit.OnInitialized = placementId => adUnit.Load();
                        adUnits.Add(adUnit);
                    }
                }
            }
        }

        private static bool IsGoogleSampleAdUnit(string placementId)
        {
            return !string.IsNullOrWhiteSpace(placementId) &&
                   placementId.StartsWith("ca-app-pub-3940256099942544/", StringComparison.Ordinal);
        }

        private void InitializeAds()
        {
            if (adsInitialized) return;

            if (!HasResolvedConsentChoice())
            {
                Debug.LogWarning("Ads remain disabled until the player has made a consent choice.");
                return;
            }

            adsInitialized = true;

            Debug.Log("Initializing ads with consent status");

            foreach (var ad in adList)
            {
                ad.adsHandler?.Init(ad.appId, false, new AdsListener(adUnits));
            }

            // Show banners after initialization
            foreach (var adUnit in adUnits)
            {
                if (adUnit.AdReference.adType == EAdType.Banner && !GameManager.instance.IsNoAdsPurchased())
                {
                    adUnit.Show();
                }
            }
        }

        private void StartConsentFlow()
        {
            if (consentInfoUpdateInProgress) return;
            consentInfoUpdateInProgress = true;
            StartCoroutine(WaitForConsentChoice());
        }

        private IEnumerator WaitForConsentChoice()
        {
            while (!HasResolvedConsentChoice())
            {
                yield return null;
            }

            consentInfoUpdateInProgress = false;
            InitializeAds();
        }

        public static bool HasResolvedConsentChoice()
        {
            return PlayerPrefs.GetInt("npa", -1) != -1;
        }

        public static bool HasPersonalizedAdsConsent()
        {
            return PlayerPrefs.GetInt("npa", -1) == 0;
        }

        public void OnConsentPreferenceChanged()
        {
            if (!adsInitialized && HasResolvedConsentChoice())
            {
                InitializeAds();
            }
        }

        private void OnEnable()
        {
            Popup.OnOpenPopup += OnOpenPopup;
            Popup.OnClosePopup += OnClosePopup;
        }

        private void OnDisable()
        {
            Popup.OnOpenPopup -= OnOpenPopup;
            Popup.OnClosePopup -= OnClosePopup;
        }

        private EPlatforms GetPlatform()
        {
            #if UNITY_ANDROID
            return EPlatforms.Android;
            #elif UNITY_IOS
            return EPlatforms.IOS;
            #elif UNITY_WEBGL
            return EPlatforms.WebGL;
            #else
            return EPlatforms.Windows;
            #endif
        }

        private void OnOpenPopup(Popup popup)
        {
            OnPopupTrigger(popup, true);
        }

        private void OnClosePopup(Popup popup)
        {
            OnPopupTrigger(popup, false);
        }

        private void OnPopupTrigger(Popup popup, bool open)
        {
            if (GameManager.instance.IsNoAdsPurchased())
            {
                return;
            }

            // Get current level number
            int currentLevel = GameDataManager.GetLevelNum();

            // Check interstitial ads using InterstitialSettings
            if (interstitialSettings != null && interstitialSettings.interstitials != null)
            {
                foreach (var interstitialElement in interstitialSettings.interstitials)
                {
                    // Check if this interstitial should trigger based on popup
                    if (((open && interstitialElement.showOnOpen) || (!open && interstitialElement.showOnClose))
                        && popup.GetType() == interstitialElement.popup.GetType())
                    {
                        var adUnit = adUnits.Find(i => i.AdReference == interstitialElement.adReference);
                        if (adUnit == null || !adUnit.IsAvailable())
                        {
                            adUnit?.Load();
                            continue;
                        }

                        // Check level conditions
                        if (!IsLevelConditionMet(currentLevel, interstitialElement))
                        {
                            continue;
                        }

                        // Find placement ID for frequency tracking
                        string placementId = GetPlacementIdForAdReference(interstitialElement.adReference);
                        if (placementId == null) continue;

                        if (!IsFrequencyConditionMet(placementId, interstitialElement.frequency))
                        {
                            continue;
                        }

                        // Set ad showing state before showing the ad
                        MenuManager.SetAdShowing(true);
                        
                        // Set up callback to reset ad showing state when ad finishes
                        var originalOnShown = adUnit.OnShown;
                        adUnit.OnShown = (result) =>
                        {
                            MenuManager.SetAdShowing(false);
                            originalOnShown?.Invoke(result);
                        };

                        adUnit.Show();
                        adUnit.Load();
                        IncrementAdFrequency(placementId);
                        return;
                    }
                }
            }

            // Handle non-interstitial ads (banners, rewarded) using the original logic
            foreach (var ad in adList)
            {
                foreach (var adElement in ad.adElements)
                {
                    if (adElement.adReference.adType == EAdType.Interstitial)
                        continue; // Skip interstitials as they're handled above

                    var adUnit = adUnits.Find(i => i.AdReference == adElement.adReference);
                    if (!adUnit.IsAvailable())
                    {
                        adUnit.Load();
                        continue;
                    }

                    if (((open && adElement.popup.showOnOpen) || (!open && adElement.popup.showOnClose)) && popup.GetType() == adElement.popup.popup.GetType())
                    {
                        // Set ad showing state for non-banner ads
                        if (adElement.adReference.adType != EAdType.Banner)
                        {
                            MenuManager.SetAdShowing(true);
                            
                            // Set up callback to reset ad showing state when ad finishes
                            var originalOnShown = adUnit.OnShown;
                            adUnit.OnShown = (result) =>
                            {
                                MenuManager.SetAdShowing(false);
                                originalOnShown?.Invoke(result);
                            };
                        }
                        
                        adUnit.Show();
                        adUnit.Load();
                        return;
                    }
                }
            }
        }

        public void ShowAdByType(AdReference adRef, Action<string> shown)
        {
            if (!GameManager.instance.GameSettings.enableAds) 
            {
                shown?.Invoke(null);
                return;
            }

            if (adRef == null)
            {
                Debug.LogError("Cannot show an ad without an ad reference.");
                return;
            }

            if (GameManager.instance.IsNoAdsPurchased() && adRef.adType != EAdType.Rewarded)
            {
                Debug.Log("Skipping non-rewarded ad because the ad-free entitlement is active.");
                return;
            }

            // Get current level number
            int currentLevel = GameDataManager.GetLevelNum();

            foreach (var adUnit in adUnits)
            {
                if (adUnit.AdReference == adRef && adUnit.IsAvailable())
                {
                    // Check level conditions for interstitial ads using InterstitialSettings
                    if (adRef.adType == EAdType.Interstitial && interstitialSettings != null)
                    {
                        var interstitialElement = interstitialSettings.interstitials?.FirstOrDefault(i => i.adReference == adRef);
                        if (interstitialElement != null)
                        {
                            if (!IsLevelConditionMet(currentLevel, interstitialElement))
                            {
                                shown?.Invoke(null);
                                return;
                            }

                            string placementId = GetPlacementIdForAdReference(adRef);
                            if (placementId != null)
                            {
                                if (!IsFrequencyConditionMet(placementId, interstitialElement.frequency))
                                {
                                    shown?.Invoke(null);
                                    return;
                                }
                                
                                // Increment frequency counter
                                IncrementAdFrequency(placementId);
                            }
                        }
                    }

                    // Set ad showing state before showing the ad
                    MenuManager.SetAdShowing(true);
                    
                    adUnit.OnShown = (result) =>
                    {
                        // Reset ad showing state when ad finishes
                        MenuManager.SetAdShowing(false);
                        shown?.Invoke(result);
                    };
                    adUnit.Show();
                    adUnit.Load();
                    return;
                }
            }
        }

        public bool IsRewardedAvailable(AdReference adRef)
        {
            foreach (var adUnit in adUnits)
            {
                if (adUnit.AdReference == adRef)
                {
                    return adUnit.IsAvailable();
                }
            }

            return false;
        }

        public void RemoveAds()
        {
            foreach (var adUnit in adUnits)
            {
                if (adUnit.AdReference.adType == EAdType.Banner)
                {
                    adUnit.Hide();
                }
            }
        }

        private bool IsLevelConditionMet(int currentLevel, InterstitialAdElement popupSetting)
        {
            return currentLevel >= popupSetting.minLevel && currentLevel <= popupSetting.maxLevel;
        }

        private bool IsFrequencyConditionMet(string placementId, int frequency)
        {
            if (frequency <= 1) return true; // Always show if frequency is 1 or less
            
            int adShowCount = PlayerPrefs.GetInt($"AdCount_{placementId}", 0);
            return adShowCount % frequency == 0;
        }

        private void IncrementAdFrequency(string placementId)
        {
            int currentCount = PlayerPrefs.GetInt($"AdCount_{placementId}", 0);
            PlayerPrefs.SetInt($"AdCount_{placementId}", currentCount + 1);
            PlayerPrefs.Save();
        }

        private string GetPlacementIdForAdReference(AdReference adRef)
        {
            foreach (var ad in adList)
            {
                foreach (var adElement in ad.adElements)
                {
                    if (adElement.adReference == adRef)
                    {
                        return adElement.placementId;
                    }
                }
            }
            return null;
        }

        public bool CanRequestPersonalizedAds()
        {
            return HasPersonalizedAdsConsent();
        }

        public bool IsPrivacyOptionsRequired()
        {
            return true;
        }

        public void ShowPrivacyOptionsForm()
        {
            MenuManager.instance.ShowPopup<Popups.GDPR>();
        }

        public void ReconsiderUMPConsent()
        {
            PlayerPrefs.DeleteKey("npa");
            PlayerPrefs.Save();
            StartConsentFlow();
        }
    }
}

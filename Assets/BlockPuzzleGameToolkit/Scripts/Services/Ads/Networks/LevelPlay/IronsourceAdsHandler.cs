// // ©2015 - 2026 Candy Smith
// // All rights reserved.

#if IRONSOURCE
using Unity.Services.LevelPlay;
#endif
using BlockPuzzleGameToolkit.Scripts.Services.Ads.AdUnits;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Services.Ads.Networks
{
    /// <summary>
    /// Owns the one LevelPlay SDK initialization for the non-banner formats.
    /// It stays inactive until the player has made a stored consent choice.
    /// </summary>
    public class IronsourceAdsHandler : AdsHandlerBase
    {
        private IAdsListener listener;
        private bool initializationRequested;
        private bool initialized;
        private bool rewardedGranted;

#if IRONSOURCE
        private LevelPlayInterstitialAd interstitialAd;
        private LevelPlayRewardedAd rewardedAd;
#endif

        public override void Init(string appKey, bool adSettingTestMode, IAdsListener adsListener)
        {
            listener = adsListener;

#if IRONSOURCE
            if (initializationRequested) return;
            if (string.IsNullOrWhiteSpace(appKey))
            {
                Debug.LogError("LevelPlay initialization is blocked because the Android app key is empty.");
                listener?.OnInitFailed();
                return;
            }

            var consentChoice = PlayerPrefs.GetInt("npa", -1);
            if (consentChoice == -1)
            {
                Debug.LogWarning("LevelPlay initialization is blocked until the player makes a consent choice.");
                listener?.OnInitFailed();
                return;
            }

            initializationRequested = true;
            LevelPlay.OnInitSuccess += OnInitializationSucceeded;
            LevelPlay.OnInitFailed += OnInitializationFailed;
            ApplyStoredConsent(consentChoice);
            LevelPlay.Init(appKey);
#else
            Debug.LogWarning("LevelPlay package is unavailable; ads remain disabled.");
            listener?.OnInitFailed();
#endif
        }

#if IRONSOURCE
        private static void ApplyStoredConsent(int consentChoice)
        {
            var personalizedConsent = consentChoice == 0;
            LevelPlay.SetConsent(personalizedConsent);
            LevelPlay.SetMetaData("do_not_sell", personalizedConsent ? "false" : "true");
        }

        private void OnInitializationSucceeded(LevelPlayConfiguration configuration)
        {
            initialized = true;
            listener?.OnAdsInitialized();
        }

        private void OnInitializationFailed(LevelPlayInitError error)
        {
            initializationRequested = false;
            Debug.LogError($"LevelPlay initialization failed: {error}");
            listener?.OnInitFailed();
        }

        private void EnsureInterstitial(AdUnit adUnit)
        {
            if (interstitialAd != null) return;
            interstitialAd = new LevelPlayInterstitialAd(adUnit.PlacementId);
            interstitialAd.OnAdLoaded += info => listener?.OnAdsLoaded(info.AdUnitId);
            interstitialAd.OnAdLoadFailed += error => listener?.OnAdsLoadFailed();
            interstitialAd.OnAdDisplayed += info => listener?.OnAdsShowStart();
            interstitialAd.OnAdDisplayFailed += (info, error) =>
            {
                listener?.OnAdsShowFailed();
                interstitialAd.LoadAd();
            };
            interstitialAd.OnAdClosed += info =>
            {
                listener?.OnAdsShowComplete();
                interstitialAd.LoadAd();
            };
        }

        private void EnsureRewarded(AdUnit adUnit)
        {
            if (rewardedAd != null) return;
            rewardedAd = new LevelPlayRewardedAd(adUnit.PlacementId);
            rewardedAd.OnAdLoaded += info => listener?.OnAdsLoaded(info.AdUnitId);
            rewardedAd.OnAdLoadFailed += error => listener?.OnAdsLoadFailed();
            rewardedAd.OnAdDisplayed += info => listener?.OnAdsShowStart();
            rewardedAd.OnAdDisplayFailed += (info, error) =>
            {
                listener?.OnAdsShowFailed();
                rewardedAd.LoadAd();
            };
            rewardedAd.OnAdRewarded += (info, reward) =>
            {
                rewardedGranted = true;
                listener?.OnAdsShowComplete();
            };
            rewardedAd.OnAdClosed += info =>
            {
                if (!rewardedGranted) listener?.OnAdsShowFailed();
                rewardedGranted = false;
                rewardedAd.LoadAd();
            };
        }
#endif

        public override void Load(AdUnit adUnit)
        {
#if IRONSOURCE
            if (!initialized) return;
            if (adUnit.AdReference.adType == EAdType.Interstitial)
            {
                EnsureInterstitial(adUnit);
                interstitialAd.LoadAd();
            }
            else if (adUnit.AdReference.adType == EAdType.Rewarded)
            {
                EnsureRewarded(adUnit);
                rewardedAd.LoadAd();
            }
#endif
        }

        public override void Show(AdUnit adUnit)
        {
#if IRONSOURCE
            if (adUnit.AdReference.adType == EAdType.Interstitial && interstitialAd != null && interstitialAd.IsAdReady())
            {
                listener?.Show(adUnit);
                interstitialAd.ShowAd();
                return;
            }

            if (adUnit.AdReference.adType == EAdType.Rewarded && rewardedAd != null && rewardedAd.IsAdReady())
            {
                rewardedGranted = false;
                listener?.Show(adUnit);
                rewardedAd.ShowAd();
                return;
            }
#endif
            listener?.OnAdsShowFailed();
        }

        public override bool IsAvailable(AdUnit adUnit)
        {
#if IRONSOURCE
            if (adUnit.AdReference.adType == EAdType.Interstitial) return interstitialAd != null && interstitialAd.IsAdReady();
            if (adUnit.AdReference.adType == EAdType.Rewarded) return rewardedAd != null && rewardedAd.IsAdReady();
#endif
            return false;
        }

        public override void Hide(AdUnit adUnit) { }
    }
}

// // ©2015 - 2026 Candy Smith
// // All rights reserved.

#if IRONSOURCE
using Unity.Services.LevelPlay;
#endif
using BlockPuzzleGameToolkit.Scripts.Services.Ads.AdUnits;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Services.Ads.Networks
{
    /// <summary>Uses the SDK initialization owned by IronsourceAdsHandler and never initializes LevelPlay a second time.</summary>
    public class IronsourceBannerHandler : AdsHandlerBase
    {
        private IAdsListener listener;
        private bool bannerLoaded;
#if IRONSOURCE
        private LevelPlayBannerAd bannerAd;
#endif

        public override void Init(string appKey, bool adSettingTestMode, IAdsListener adsListener)
        {
            listener = adsListener;
        }

#if IRONSOURCE
        private void EnsureBanner(AdUnit adUnit)
        {
            if (bannerAd != null) return;
            var builder = new LevelPlayBannerAd.Config.Builder();
            builder.SetSize(LevelPlayAdSize.BANNER);
            builder.SetPosition(LevelPlayBannerPosition.BottomCenter);
            builder.SetRespectSafeArea(true);
            builder.SetDisplayOnLoad(false);
            var config = builder.Build();
            bannerAd = new LevelPlayBannerAd(adUnit.PlacementId, config);
            bannerAd.OnAdLoaded += info =>
            {
                bannerLoaded = true;
                listener?.OnAdsLoaded(info.AdUnitId);
            };
            bannerAd.OnAdLoadFailed += error =>
            {
                bannerLoaded = false;
                listener?.OnAdsLoadFailed();
            };
            bannerAd.OnAdDisplayFailed += (info, error) => bannerLoaded = false;
        }
#endif

        public override void Load(AdUnit adUnit)
        {
#if IRONSOURCE
            EnsureBanner(adUnit);
            bannerLoaded = false;
            bannerAd.LoadAd();
#endif
        }

        public override void Show(AdUnit adUnit)
        {
#if IRONSOURCE
            if (bannerAd != null && bannerLoaded)
            {
                listener?.Show(adUnit);
                bannerAd.ShowAd();
                return;
            }
#endif
            listener?.OnAdsShowFailed();
        }

        public override bool IsAvailable(AdUnit adUnit) => bannerLoaded;

        public override void Hide(AdUnit adUnit)
        {
#if IRONSOURCE
            bannerAd?.HideAd();
#endif
        }
    }
}

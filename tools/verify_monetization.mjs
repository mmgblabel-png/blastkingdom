import { readFileSync } from "node:fs";
import { join } from "node:path";

const root = process.cwd();
const read = relativePath => readFileSync(join(root, relativePath), "utf8");
const assert = (condition, message) => {
  if (!condition) throw new Error(message);
};

const projectSettings = read("ProjectSettings/ProjectSettings.asset");
assert(projectSettings.includes("Android: com.mmgb.blastkingdom"), "Android package identifier is not set to the BlastKingdom Play package.");

const expectedAndroidIds = new Map([
  ["CoinsProduct1.asset", "com.mmgb.blastkingdom.coinspack1"],
  ["CoinsProduct2.asset", "com.mmgb.blastkingdom.coinspack2"],
  ["CoinsProduct3.asset", "com.mmgb.blastkingdom.coinspack6"],
  ["CoinsProduct4.asset", "com.mmgb.blastkingdom.coinspack4"],
  ["NoAds.asset", "com.mmgb.blastkingdom.noads"],
]);

for (const [asset, expectedId] of expectedAndroidIds) {
  const content = read(`Assets/BlockPuzzleGameToolkit/Resources/ProductIDs/${asset}`);
  assert(content.includes(`androidId: ${expectedId}`), `${asset} does not map to ${expectedId}.`);
}

const iapManager = read("Assets/BlockPuzzleGameToolkit/Scripts/Services/IAP/IAPManager.cs");
assert(iapManager.includes("InitializationTimeoutSeconds"), "IAP initialization timeout guard is missing.");
assert(iapManager.includes("HasInitializationFailed"), "IAP initialization failure guard is missing.");

const adsManager = read("Assets/BlockPuzzleGameToolkit/Scripts/Services/AdsManager.cs");
assert(adsManager.includes("HasResolvedConsentChoice()"), "Consent gate is missing from the ad initialization flow.");
assert(adsManager.includes("IsNoAdsPurchased() && adRef.adType != EAdType.Rewarded"), "Central ad-free entitlement gate is missing.");
assert(adsManager.includes("StartCoroutine(WaitForConsentChoice())"), "Ads do not wait for an explicit consent choice.");

const adUnit = read("Assets/BlockPuzzleGameToolkit/Scripts/Services/Ads/AdUnits/AdUnit.cs");
assert(adUnit.includes("AdsHandler != null && AdsHandler.IsAvailable(this)"), "Ad availability does not use the handler as the single source of truth.");

const levelPlayHandler = read("Assets/BlockPuzzleGameToolkit/Scripts/Services/Ads/Networks/LevelPlay/IronsourceAdsHandler.cs");
assert(levelPlayHandler.includes("LevelPlay.Init(appKey)"), "LevelPlay initialization is missing.");
assert(levelPlayHandler.includes("PlayerPrefs.GetInt(\"npa\", -1)"), "LevelPlay does not require a stored consent choice.");
assert(levelPlayHandler.includes("new LevelPlayInterstitialAd"), "LevelPlay interstitial integration is missing.");
assert(levelPlayHandler.includes("new LevelPlayRewardedAd"), "LevelPlay rewarded integration is missing.");

const bannerHandler = read("Assets/BlockPuzzleGameToolkit/Scripts/Services/Ads/Networks/LevelPlay/IronsourceBannerHandler.cs");
assert(!bannerHandler.includes("LevelPlay.Init("), "Banner handler must not initialize LevelPlay a second time.");

const adsSettings = read("Assets/BlockPuzzleGameToolkit/Resources/Settings/AdsSettings.asset");
assert(adsSettings.includes("- name: LevelPlay  Android\n    enable: 1"), "Android LevelPlay profile is not enabled for the review build.");
assert(adsSettings.includes("appId: 6144908"), "LevelPlay Android app key does not match the verified Unity project.");
assert(adsSettings.includes("placementId: 0hwfeczdewkpcr3t"), "LevelPlay interstitial ad-unit ID is missing.");
assert(adsSettings.includes("placementId: a73imhekil2b1lun"), "LevelPlay rewarded ad-unit ID is missing.");
assert(adsSettings.includes("placementId: t9e13g4jns1gczza"), "LevelPlay banner ad-unit ID is missing.");
assert(!adsSettings.includes("- name: admob  Android\n    enable: 1"), "AdMob must remain disabled in the review build.");

console.log("BlastKingdom monetization source checks passed.");

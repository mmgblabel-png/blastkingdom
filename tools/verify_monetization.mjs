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
assert(adsManager.includes("ConsentInformation.CanRequestAds()"), "Consent gate is missing from the ad initialization flow.");
assert(adsManager.includes("IsNoAdsPurchased() && adRef.adType != EAdType.Rewarded"), "Central ad-free entitlement gate is missing.");
assert(adsManager.includes("IsGoogleSampleAdUnit"), "Release protection against sample ad units is missing.");

const adUnit = read("Assets/BlockPuzzleGameToolkit/Scripts/Services/Ads/AdUnits/AdUnit.cs");
assert(adUnit.includes("AdsHandler != null && AdsHandler.IsAvailable(this)"), "Ad availability does not use the handler as the single source of truth.");

const admobHandler = read("Assets/BlockPuzzleGameToolkit/Scripts/Services/Ads/Networks/AdmobHandler.cs");
assert(admobHandler.includes("_interstitialAd != null && _interstitialAd.CanShowAd()"), "Interstitial readiness check is missing.");
assert(admobHandler.includes("_rewardedAd != null && _rewardedAd.CanShowAd()"), "Rewarded readiness check is missing.");

console.log("BlastKingdom monetization source checks passed.");

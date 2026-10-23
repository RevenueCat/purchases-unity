# RevenueCat AdMob adapter for Unity

Connects the [Google Mobile Ads Unity plugin](https://developers.google.com/admob/unity/quick-start) to [RevenueCat](https://www.revenuecat.com/)'s ad tracking and reward verification. Load your ads through the adapter and it reports loaded, failed-to-load, displayed, opened, revenue and earned-reward events for you; rewarded ads can additionally have their rewards verified server-side. See the [AdMob integration docs](https://www.revenuecat.com/docs/ad-monetization/admob).

## Installation

Install `com.revenuecat.purchases-unity`, the Google Mobile Ads plugin (`com.google.ads.mobile` 10.x or newer, from OpenUPM or the `.unitypackage`; 11.6.0 or newer when installed through the Package Manager, so that it shares Unity's External Dependency Manager with this package) and this package (`com.revenuecat.purchases-admob-unity`). The adapter references the plugin's `GoogleMobileAds.dll`, so it compiles wherever the plugin lives.

```csharp
using RevenueCat.AdMob;

RevenueCatAdMob.Initialize(purchases); // your configured Purchases component
```

The adapter tracks from whichever thread Google Mobile Ads raises its callbacks on. Enable [`MobileAds.RaiseAdEventsOnUnityMainThread`](https://developers.google.com/admob/unity/global-settings#raise_ad_events_on_the_unity_main_thread) only if your own ad callbacks touch Unity objects.

## Tracking full-screen ads

Replace `InterstitialAd.Load(...)` (and the rewarded, rewarded interstitial and app open equivalents) with the matching `RevenueCatAdMob.LoadAndTrack...` call. The callback receives the same ad object; `placement` is a free-form label for where the ad is shown.

```csharp
RevenueCatAdMob.LoadAndTrackInterstitialAd(adUnitId, new AdRequest(), (ad, error) =>
{
    if (ad == null) return;
    _interstitial = ad;
}, placement: "level_complete");

// Later. The placement given here overrides the load-time one for this impression.
_interstitial.ShowAndTrack("level_complete_retry");
```

Google's events are multicast, so your own `OnAdPaid += ...` handlers keep working next to the adapter's.

## Banner and native overlay ads

```csharp
var banner = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
RevenueCatAdMob.TrackBannerView(banner, adUnitId, placement: "home_bottom");
banner.LoadAd(new AdRequest());

NativeOverlayAd.Load(adUnitId, new AdRequest(), options, (ad, error) =>
{
    if (ad != null) RevenueCatAdMob.TrackNativeOverlayAd(ad, adUnitId, placement: "feed");
});
```

## Reward verification

For `RewardedAd` and `RewardedInterstitialAd`, call `EnableRewardVerification()` once the ad has loaded and show it with `ShowWithRewardVerification(...)`. The adapter attaches a RevenueCat token to AdMob's server-side verification options and polls RevenueCat for the verified reward when the user earns it.

```csharp
RevenueCatAdMob.LoadAndTrackRewardedAd(adUnitId, new AdRequest(), (ad, error) =>
{
    if (ad == null) return;
    ad.EnableRewardVerification(enabled => _rewarded = ad);
}, placement: "extra_life");

_rewarded.ShowWithRewardVerification(
    completed: result => { if (!result.Failed) Grant(result.Reward); },
    started: () => ShowSpinner());
```

`completed` is called at most once per show, after the user earns the reward; it is not called when the ad is dismissed or fails before a reward. The ad unit's server-side verification URL must point at RevenueCat and a reward must be configured for it in the RevenueCat dashboard; see the [rewards docs](https://www.revenuecat.com/docs/ad-monetization/rewards).

## Requirements

* iOS 15.0+ (ad tracking is unavailable below that; calls are ignored with a warning)
* Google Mobile Ads Unity plugin 10.x+ (11.6.0+ when installed through the Package Manager)
* `com.revenuecat.purchases-unity` 9.14.0+

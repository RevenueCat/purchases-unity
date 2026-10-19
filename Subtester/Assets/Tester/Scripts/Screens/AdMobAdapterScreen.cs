using System;
using GoogleMobileAds.Api;
using RevenueCat.AdMob;
using UnityEngine.UIElements;

namespace RevenueCat.Tester.Screens
{
    // Exercises the RevenueCat AdMob adapter: every full-screen format plus a banner is loaded
    // through RevenueCatAdMob, which reports loaded / failed / displayed / opened / revenue events;
    // the rewarded formats additionally verify the earned reward server-side.
    public class AdMobAdapterScreen : ScreenBase
    {
        // Google's official test ad units, always fill and safe to commit. Swap the rewarded ones
        // for your own AdMob units (with their SSV URL pointed at RevenueCat) to grant a real reward.
#if UNITY_ANDROID
        private const string InterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
        private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        private const string RewardedInterstitialAdUnitId = "ca-app-pub-3940256099942544/5354046379";
        private const string AppOpenAdUnitId = "ca-app-pub-3940256099942544/9257395921";
        private const string BannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
        private const string InterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
        private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";
        private const string RewardedInterstitialAdUnitId = "ca-app-pub-3940256099942544/6978759866";
        private const string AppOpenAdUnitId = "ca-app-pub-3940256099942544/5575463023";
        private const string BannerAdUnitId = "ca-app-pub-3940256099942544/2435281174";
#else
        private const string InterstitialAdUnitId = "unused";
        private const string RewardedAdUnitId = "unused";
        private const string RewardedInterstitialAdUnitId = "unused";
        private const string AppOpenAdUnitId = "unused";
        private const string BannerAdUnitId = "unused";
#endif

        private InterstitialAd _interstitial;
        private RewardedAd _rewarded;
        private RewardedInterstitialAd _rewardedInterstitial;
        private AppOpenAd _appOpen;
        private BannerView _banner;

        private Label _statusLabel;
        private Label _resultLabel;
        private Button _showInterstitial;
        private Button _showRewarded;
        private Button _showRewardedInterstitial;
        private Button _showAppOpen;

        public AdMobAdapterScreen(Purchases purchases, LogConsole console)
            : base(purchases, console) { }

        protected override void Build()
        {
            AddSectionHeader("AdMob Adapter");
            _statusLabel = AddInfoLabel("Initializing Mobile Ads...");
            _resultLabel = AddInfoLabel();

            AddButton("Load Interstitial", LoadInterstitial);
            _showInterstitial = AddButton("Show Interstitial", () => _interstitial?.ShowAndTrack("interstitial_shown"));
            AddButton("Load Rewarded", LoadRewarded);
            _showRewarded = AddButton("Show Rewarded", ShowRewarded);
            AddButton("Load Rewarded Interstitial", LoadRewardedInterstitial);
            _showRewardedInterstitial = AddButton("Show Rewarded Interstitial", ShowRewardedInterstitial);
            AddButton("Load App Open", LoadAppOpen);
            _showAppOpen = AddButton("Show App Open", () => _appOpen?.ShowAndTrack("app_open_shown"));
            AddButtonRow(("Load Banner", LoadBanner), ("Remove Banner", RemoveBanner));
            SetShowButtons(false);

            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            RevenueCatAdMob.Initialize(Purchases);
            MobileAds.Initialize(_ => SetStatus("Mobile Ads ready."));
        }

        private void SetShowButtons(bool enabled)
        {
            _showInterstitial.SetEnabled(enabled && _interstitial != null);
            _showRewarded.SetEnabled(enabled && _rewarded != null);
            _showRewardedInterstitial.SetEnabled(enabled && _rewardedInterstitial != null);
            _showAppOpen.SetEnabled(enabled && _appOpen != null);
        }

        private void LoadInterstitial()
        {
            _interstitial?.Destroy();
            _interstitial = null;
            SetShowButtons(true);
            SetStatus("Loading interstitial...");
            RevenueCatAdMob.LoadAndTrackInterstitialAd(InterstitialAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (ad == null)
                {
                    SetStatus($"Interstitial failed: {error?.GetMessage()}");
                    return;
                }
                _interstitial = ad;
                ad.OnAdFullScreenContentClosed += () => { ReleaseInterstitial(); SetStatus("Interstitial dismissed."); };
                ad.OnAdFullScreenContentFailed += error => { ReleaseInterstitial(); SetStatus($"Interstitial show failed: {error?.GetMessage()}"); };
                SetStatus($"Interstitial ready ({ad.GetResponseInfo().GetResponseId()})");
                SetShowButtons(true);
            }, "interstitial_tab");
        }

        private void LoadRewarded()
        {
            _rewarded?.Destroy();
            _rewarded = null;
            SetShowButtons(true);
            _resultLabel.text = "";
            SetStatus("Loading rewarded...");
            RevenueCatAdMob.LoadAndTrackRewardedAd(RewardedAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (ad == null)
                {
                    SetStatus($"Rewarded failed: {error?.GetMessage()}");
                    return;
                }
                _rewarded = ad;
                ad.OnAdFullScreenContentClosed += ReleaseRewarded;
                ad.OnAdFullScreenContentFailed += error => { ReleaseRewarded(); SetStatus($"Rewarded show failed: {error?.GetMessage()}"); };
                ad.EnableRewardVerification(enabled =>
                {
                    if (_rewarded != ad) return;
                    SetStatus($"Rewarded ready, verification {(enabled ? "enabled" : "NOT enabled")} ({ad.GetResponseInfo().GetResponseId()})");
                    SetShowButtons(true);
                });
            }, "rewarded_tab");
        }

        private void ShowRewarded()
        {
            var ad = _rewarded;
            if (ad == null || !ad.CanShowAd()) return;
            ad.ShowWithRewardVerification(OnVerified, () => SetStatus("Verifying reward..."), "rewarded_shown");
        }

        private void LoadRewardedInterstitial()
        {
            _rewardedInterstitial?.Destroy();
            _rewardedInterstitial = null;
            SetShowButtons(true);
            _resultLabel.text = "";
            SetStatus("Loading rewarded interstitial...");
            RevenueCatAdMob.LoadAndTrackRewardedInterstitialAd(RewardedInterstitialAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (ad == null)
                {
                    SetStatus($"Rewarded interstitial failed: {error?.GetMessage()}");
                    return;
                }
                _rewardedInterstitial = ad;
                ad.OnAdFullScreenContentClosed += ReleaseRewardedInterstitial;
                ad.OnAdFullScreenContentFailed += error => { ReleaseRewardedInterstitial(); SetStatus($"Rewarded interstitial show failed: {error?.GetMessage()}"); };
                ad.EnableRewardVerification(enabled =>
                {
                    if (_rewardedInterstitial != ad) return;
                    SetStatus($"Rewarded interstitial ready, verification {(enabled ? "enabled" : "NOT enabled")}");
                    SetShowButtons(true);
                });
            }, "rewarded_interstitial_tab");
        }

        private void ShowRewardedInterstitial()
        {
            var ad = _rewardedInterstitial;
            if (ad == null || !ad.CanShowAd()) return;
            ad.ShowWithRewardVerification(OnVerified, () => SetStatus("Verifying reward..."), "rewarded_interstitial_shown");
        }

        private void LoadAppOpen()
        {
            _appOpen?.Destroy();
            _appOpen = null;
            SetShowButtons(true);
            SetStatus("Loading app open...");
            RevenueCatAdMob.LoadAndTrackAppOpenAd(AppOpenAdUnitId, new AdRequest(), (ad, error) =>
            {
                if (ad == null)
                {
                    SetStatus($"App open failed: {error?.GetMessage()}");
                    return;
                }
                _appOpen = ad;
                ad.OnAdFullScreenContentClosed += ReleaseAppOpen;
                ad.OnAdFullScreenContentFailed += error => { ReleaseAppOpen(); SetStatus($"App open show failed: {error?.GetMessage()}"); };
                SetStatus($"App open ready ({ad.GetResponseInfo().GetResponseId()})");
                SetShowButtons(true);
            }, "app_open_tab");
        }

        private void LoadBanner()
        {
            _banner?.Destroy();
            _banner = new BannerView(BannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
            RevenueCatAdMob.TrackBannerView(_banner, BannerAdUnitId, "banner_tab");
            _banner.OnBannerAdLoaded += () => SetStatus($"Banner loaded ({_banner.GetResponseInfo()?.GetResponseId()})");
            _banner.OnBannerAdLoadFailed += error => SetStatus($"Banner failed: {error?.GetMessage()}");
            _banner.OnAdPaid += value => SetStatus($"Banner paid {value.Value} {value.CurrencyCode}");
            _banner.LoadAd(new AdRequest());
        }

        private void RemoveBanner()
        {
            _banner?.Destroy();
            _banner = null;
            SetStatus("Banner removed.");
        }

        // Full-screen ads are single-use: once shown (or failed to show) the native object is only
        // held so it can be destroyed.
        private void ReleaseInterstitial()
        {
            _interstitial?.Destroy();
            _interstitial = null;
            SetShowButtons(true);
        }

        private void ReleaseRewarded()
        {
            _rewarded?.Destroy();
            _rewarded = null;
            SetShowButtons(true);
        }

        private void ReleaseRewardedInterstitial()
        {
            _rewardedInterstitial?.Destroy();
            _rewardedInterstitial = null;
            SetShowButtons(true);
        }

        private void ReleaseAppOpen()
        {
            _appOpen?.Destroy();
            _appOpen = null;
            SetShowButtons(true);
        }

        private void OnVerified(Purchases.RewardVerificationResult result)
        {
            SetStatus("Done.");
            Log(result.ToString());
            if (result.Failed || result.Reward == null)
            {
                _resultLabel.text = "❌ verification failed";
                return;
            }
            var more = result.MoreRewards.Count == 0 ? "" : $" (+{result.MoreRewards.Count} more)";
            _resultLabel.text = $"✅ {DescribeReward(result.Reward)}{more}";
        }

        private static string DescribeReward(Purchases.VerifiedReward reward)
        {
            switch (reward)
            {
                case Purchases.VerifiedReward.VirtualCurrency vc: return $"+{vc.Amount} {vc.Code}";
                case Purchases.VerifiedReward.Entitlement e: return $"entitlement \"{e.Identifier}\"";
                case Purchases.VerifiedReward.NoReward _: return "no reward";
                default: return "unsupported reward";
            }
        }

        private void SetStatus(string text)
        {
            _statusLabel.text = text;
            Log(text);
        }
    }
}

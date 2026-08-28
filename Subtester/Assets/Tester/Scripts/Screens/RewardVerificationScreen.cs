using GoogleMobileAds.Api;
using UnityEngine.UIElements;

namespace RevenueCat.Tester.Screens
{
    // Demonstrates the rewarded-ad reward-verification primitives end to end against a
    // Google test ad unit:
    //   1. Load a rewarded ad and use its response id as the impression id.
    //   2. GenerateRewardVerificationToken -> forward CustomData + AppUserID to AdMob's
    //      server-side verification options.
    //   3. Show the ad; once the user earns it, PollRewardVerification for the granted reward.
    public class RewardVerificationScreen : ScreenBase
    {
        // Google's official test rewarded-interstitial ad units. Always fill with a test ad and
        // are safe to commit. Swap for your own AdMob unit (with its SSV URL pointed at
        // RevenueCat) to grant a real reward.
#if UNITY_ANDROID
        private const string AdUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
        private const string AdUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
        private const string AdUnitId = "unused";
#endif

        private RewardedInterstitialAd _rewardedAd;
        private Purchases.RewardVerificationToken _token;

        private Label _statusLabel;
        private Label _impressionIdLabel;
        private Label _resultLabel;
        private Button _showButton;

        public RewardVerificationScreen(Purchases purchases, LogConsole console)
            : base(purchases, console) { }

        protected override void Build()
        {
            AddSectionHeader("Rewarded Ad Verification");

            _statusLabel = AddInfoLabel("Initializing Mobile Ads...");
            _impressionIdLabel = AddInfoLabel();
            _resultLabel = AddInfoLabel();

            AddButton("Load Rewarded Ad", LoadAd);
            _showButton = AddButton("Show Ad", ShowAd);
            _showButton.SetEnabled(false);

            MobileAds.Initialize(_ => SetStatus("Mobile Ads ready. Load an ad."));
        }

        private void LoadAd()
        {
            _rewardedAd?.Destroy();
            _rewardedAd = null;
            _showButton.SetEnabled(false);
            _resultLabel.text = "";
            _impressionIdLabel.text = "";
            SetStatus("Loading ad...");

            RewardedInterstitialAd.Load(AdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    SetStatus($"Failed to load ad: {error?.GetMessage()}");
                    return;
                }

                // 1. Use the loaded ad's response id as the impression id, then generate a
                //    verification token for it.
                var impressionId = ad.GetResponseInfo().GetResponseId();
                _impressionIdLabel.text = $"impressionId: {impressionId}";

                Purchases.GenerateRewardVerificationToken(impressionId, (token, tokenError) =>
                {
                    if (tokenError != null)
                    {
                        LogError(tokenError);
                        SetStatus("Failed to generate verification token.");
                        return;
                    }

                    _token = token;
                    _rewardedAd = ad;

                    // 2. Wire RevenueCat verification into AdMob's server-side verification.
                    ad.SetServerSideVerificationOptions(new ServerSideVerificationOptions
                    {
                        CustomData = token.CustomData,
                        UserId = token.AppUserID
                    });

                    ad.OnAdFullScreenContentClosed += LoadAd;

                    _showButton.SetEnabled(true);
                    SetStatus("Ad ready.");
                });
            });
        }

        private void ShowAd()
        {
            var ad = _rewardedAd;
            var token = _token;
            if (ad == null || token == null || !ad.CanShowAd()) return;

            _rewardedAd = null;
            _showButton.SetEnabled(false);

            ad.Show(_ =>
            {
                // 3. The ad was watched. AdMob fires its SSV callback to RevenueCat; poll until
                //    verification reaches a terminal state.
                SetStatus("Verifying reward...");
                Purchases.PollRewardVerification(token.ClientTransactionId, (result, error) =>
                {
                    if (error != null)
                    {
                        LogError(error);
                        SetStatus("Polling failed.");
                        return;
                    }

                    SetStatus("Done.");
                    Log(result.ToString());
                    if (result.Failed || result.Reward == null)
                    {
                        _resultLabel.text = "❌ verification failed";
                        return;
                    }

                    var more = result.MoreRewards.Count == 0 ? "" : $" (+{result.MoreRewards.Count} more)";
                    _resultLabel.text = $"✅ {DescribeReward(result.Reward)}{more}";
                });
            });
        }

        private static string DescribeReward(Purchases.VerifiedReward reward)
        {
            switch (reward)
            {
                case Purchases.VerifiedReward.VirtualCurrency virtualCurrency:
                    return $"+{virtualCurrency.Amount} {virtualCurrency.Code}";
                case Purchases.VerifiedReward.Entitlement entitlement:
                    return $"entitlement \"{entitlement.Identifier}\"";
                case Purchases.VerifiedReward.NoReward _:
                    return "no reward";
                default:
                    return "unsupported reward";
            }
        }

        private void SetStatus(string message)
        {
            _statusLabel.text = message;
            Log(message);
        }
    }
}

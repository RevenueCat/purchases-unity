using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;

namespace RevenueCat.AdMob
{
    /// <summary>
    /// Entry point of the RevenueCat AdMob adapter for Unity.
    ///
    /// Call <see cref="Initialize"/> once with your <see cref="Purchases"/> component, then load
    /// full-screen ads through the <c>LoadAndTrack...</c> methods (or attach tracking to a banner /
    /// native overlay ad with <see cref="TrackBannerView"/> / <see cref="TrackNativeOverlayAd"/>).
    /// The adapter reports loaded, failed-to-load, displayed, opened and revenue events to
    /// RevenueCat; rewarded formats additionally get server-side reward verification through
    /// <see cref="EnableRewardVerification(RewardedAd)"/> and
    /// <see cref="ShowWithRewardVerification(RewardedAd, RewardVerificationCompletedFunc, RewardVerificationStartedFunc, string)"/>.
    ///
    /// Every method takes an optional <c>placement</c>, a free-form label for where in your game the
    /// ad is shown. The <c>ShowAndTrack</c> extensions can override it at show time.
    ///
    /// The adapter tracks from whichever thread Google Mobile Ads raises its callbacks on. Enable
    /// <c>MobileAds.RaiseAdEventsOnUnityMainThread</c> only if your own callbacks touch Unity objects.
    /// </summary>
    public static class RevenueCatAdMob
    {
        /// <summary>Called as soon as the user earned the reward, before verification starts.</summary>
        public delegate void RewardVerificationStartedFunc();

        /// <summary>
        /// Called at most once per show with the verification outcome, after the user earns the
        /// reward. Not called when the ad is dismissed or fails before a reward.
        /// </summary>
        public delegate void RewardVerificationCompletedFunc(Purchases.RewardVerificationResult result);

        /// <summary>The <see cref="Purchases"/> component the adapter reports to.</summary>
        public static Purchases Purchases { get; private set; }

        /// <summary>Points the adapter at your <see cref="Purchases"/> component. Call once after configuring it.</summary>
        public static void Initialize(Purchases purchases)
        {
            Purchases = purchases;
        }

        // Full-screen formats

        public static void LoadAndTrackInterstitialAd(
            string adUnitId, AdRequest request, Action<InterstitialAd, LoadAdError> callback, string placement = null)
        {
            InterstitialAd.Load(adUnitId, request, (ad, error) =>
            {
                if (ad == null)
                {
                    AdMobTracker.TrackFailedToLoad(adUnitId, AdTracker.Format.Interstitial, placement, error);
                }
                else
                {
                    var state = Track(ad, AdTracker.Format.Interstitial, ad.GetAdUnitID(), ad.GetResponseInfo(), placement);
                    ad.OnAdImpressionRecorded += () => AdMobTracker.TrackDisplayed(state);
                    ad.OnAdClicked += () => AdMobTracker.TrackOpened(state);
                    ad.OnAdPaid += value => AdMobTracker.TrackRevenue(state, value);
                }
                callback?.Invoke(ad, error);
            });
        }

        public static void LoadAndTrackAppOpenAd(
            string adUnitId, AdRequest request, Action<AppOpenAd, LoadAdError> callback, string placement = null)
        {
            AppOpenAd.Load(adUnitId, request, (ad, error) =>
            {
                if (ad == null)
                {
                    AdMobTracker.TrackFailedToLoad(adUnitId, AdTracker.Format.AppOpen, placement, error);
                }
                else
                {
                    var state = Track(ad, AdTracker.Format.AppOpen, ad.GetAdUnitID(), ad.GetResponseInfo(), placement);
                    ad.OnAdImpressionRecorded += () => AdMobTracker.TrackDisplayed(state);
                    ad.OnAdClicked += () => AdMobTracker.TrackOpened(state);
                    ad.OnAdPaid += value => AdMobTracker.TrackRevenue(state, value);
                }
                callback?.Invoke(ad, error);
            });
        }

        public static void LoadAndTrackRewardedAd(
            string adUnitId, AdRequest request, Action<RewardedAd, LoadAdError> callback, string placement = null)
        {
            RewardedAd.Load(adUnitId, request, (ad, error) =>
            {
                if (ad == null)
                {
                    AdMobTracker.TrackFailedToLoad(adUnitId, AdTracker.Format.Rewarded, placement, error);
                }
                else
                {
                    var state = Track(ad, AdTracker.Format.Rewarded, ad.GetAdUnitID(), ad.GetResponseInfo(), placement);
                    ad.OnAdImpressionRecorded += () => AdMobTracker.TrackDisplayed(state);
                    ad.OnAdClicked += () => AdMobTracker.TrackOpened(state);
                    ad.OnAdPaid += value => AdMobTracker.TrackRevenue(state, value);
                }
                callback?.Invoke(ad, error);
            });
        }

        public static void LoadAndTrackRewardedInterstitialAd(
            string adUnitId, AdRequest request, Action<RewardedInterstitialAd, LoadAdError> callback, string placement = null)
        {
            RewardedInterstitialAd.Load(adUnitId, request, (ad, error) =>
            {
                if (ad == null)
                {
                    AdMobTracker.TrackFailedToLoad(adUnitId, AdTracker.Format.RewardedInterstitial, placement, error);
                }
                else
                {
                    var state = Track(ad, AdTracker.Format.RewardedInterstitial, ad.GetAdUnitID(), ad.GetResponseInfo(), placement);
                    ad.OnAdImpressionRecorded += () => AdMobTracker.TrackDisplayed(state);
                    ad.OnAdClicked += () => AdMobTracker.TrackOpened(state);
                    ad.OnAdPaid += value => AdMobTracker.TrackRevenue(state, value);
                }
                callback?.Invoke(ad, error);
            });
        }

        // Banner and native overlay

        /// <summary>
        /// Attaches tracking to a <see cref="BannerView"/>. Call right after constructing it and
        /// before <c>LoadAd</c>; refreshes report a new loaded event with the new response id.
        /// </summary>
        public static void TrackBannerView(BannerView banner, string adUnitId, string placement = null)
        {
            var state = TrackedAdState.Create(banner, AdTracker.Format.Banner, adUnitId, placement);
            banner.OnBannerAdLoaded += () =>
            {
                state.UpdateFromResponse(banner.GetResponseInfo());
                AdMobTracker.TrackLoaded(state);
            };
            banner.OnBannerAdLoadFailed += error =>
                AdMobTracker.TrackFailedToLoad(adUnitId, AdTracker.Format.Banner, placement, error);
            banner.OnAdImpressionRecorded += () => AdMobTracker.TrackDisplayed(state);
            banner.OnAdClicked += () => AdMobTracker.TrackOpened(state);
            banner.OnAdPaid += value => AdMobTracker.TrackRevenue(state, value);
        }

        /// <summary>Attaches tracking to a loaded <see cref="NativeOverlayAd"/>.</summary>
        public static void TrackNativeOverlayAd(NativeOverlayAd ad, string adUnitId, string placement = null)
        {
            var state = Track(ad, AdTracker.Format.Native, adUnitId, ad.GetResponseInfo(), placement);
            ad.OnAdImpressionRecorded += () => AdMobTracker.TrackDisplayed(state);
            ad.OnAdClicked += () => AdMobTracker.TrackOpened(state);
            ad.OnAdPaid += value => AdMobTracker.TrackRevenue(state, value);
        }

        // Show-time placement overrides

        /// <summary>Shows the ad; <paramref name="placement"/> overrides the load-time placement for this impression.</summary>
        public static void ShowAndTrack(this InterstitialAd ad, string placement = null)
        {
            PrepareShow(ad, placement);
            ad.Show();
        }

        /// <summary>Shows the ad; <paramref name="placement"/> overrides the load-time placement for this impression.</summary>
        public static void ShowAndTrack(this AppOpenAd ad, string placement = null)
        {
            PrepareShow(ad, placement);
            ad.Show();
        }

        /// <summary>Shows the ad; <paramref name="placement"/> overrides the load-time placement for this impression.</summary>
        public static void ShowAndTrack(this RewardedAd ad, Action<Reward> userRewardEarnedCallback, string placement = null)
        {
            PrepareShow(ad, placement);
            ad.Show(userRewardEarnedCallback);
        }

        /// <summary>Shows the ad; <paramref name="placement"/> overrides the load-time placement for this impression.</summary>
        public static void ShowAndTrack(this RewardedInterstitialAd ad, Action<Reward> userRewardEarnedCallback, string placement = null)
        {
            PrepareShow(ad, placement);
            ad.Show(userRewardEarnedCallback);
        }

        // Reward verification

        /// <summary>
        /// Attaches a RevenueCat reward-verification token to the ad's server-side verification
        /// options. Call after the ad has loaded and before showing it. Completes asynchronously;
        /// showing the ad before <paramref name="callback"/> fires delivers a failed verification.
        /// </summary>
        public static void EnableRewardVerification(this RewardedAd ad, Action<bool> callback = null) =>
            EnableRewardVerification(ad, AdTracker.Format.Rewarded, ad.GetAdUnitID(), ad.GetResponseInfo(),
                ad.SetServerSideVerificationOptions, callback);

        /// <inheritdoc cref="EnableRewardVerification(RewardedAd, Action{bool})"/>
        public static void EnableRewardVerification(this RewardedInterstitialAd ad, Action<bool> callback = null) =>
            EnableRewardVerification(ad, AdTracker.Format.RewardedInterstitial, ad.GetAdUnitID(), ad.GetResponseInfo(),
                ad.SetServerSideVerificationOptions, callback);

        /// <summary>
        /// Shows the ad and verifies the earned reward with RevenueCat. <paramref name="completed"/>
        /// is called at most once, after the user earns the reward; it is not called when the ad
        /// is dismissed or fails before a reward. It receives a failed result when
        /// <see cref="EnableRewardVerification(RewardedAd, Action{bool})"/> was not called for this ad.
        /// </summary>
        public static void ShowWithRewardVerification(
            this RewardedAd ad,
            RewardVerificationCompletedFunc completed,
            RewardVerificationStartedFunc started = null,
            string placement = null,
            Action<Reward> userRewardEarnedCallback = null)
        {
            var state = PrepareShow(ad, placement);
            ad.Show(reward => OnRewardEarned(state, reward, userRewardEarnedCallback, started, completed));
        }

        /// <inheritdoc cref="ShowWithRewardVerification(RewardedAd, RewardVerificationCompletedFunc, RewardVerificationStartedFunc, string, Action{Reward})"/>
        public static void ShowWithRewardVerification(
            this RewardedInterstitialAd ad,
            RewardVerificationCompletedFunc completed,
            RewardVerificationStartedFunc started = null,
            string placement = null,
            Action<Reward> userRewardEarnedCallback = null)
        {
            var state = PrepareShow(ad, placement);
            ad.Show(reward => OnRewardEarned(state, reward, userRewardEarnedCallback, started, completed));
        }

        // Internals

        private static TrackedAdState Track(object ad, AdTracker.Format format, string adUnitId, ResponseInfo info, string placement)
        {
            var state = TrackedAdState.Create(ad, format, adUnitId, placement);
            state.UpdateFromResponse(info);
            AdMobTracker.TrackLoaded(state);
            return state;
        }

        private static TrackedAdState PrepareShow(object ad, string placement)
        {
            var state = TrackedAdState.For(ad);
            if (state == null)
            {
                AdMobTracker.LogWarning("This ad was not loaded through RevenueCatAdMob; it will not be tracked.");
                return null;
            }
            if (placement != null) state.ShowPlacement = placement;
            state.RewardVerificationDelivered = false;
            return state;
        }

        private static void EnableRewardVerification(
            object ad, AdTracker.Format format, string adUnitId, ResponseInfo info,
            Action<ServerSideVerificationOptions> setOptions, Action<bool> callback)
        {
            var purchases = Purchases;
            if (purchases == null || !purchases.IsConfigured())
            {
                AdMobTracker.LogWarning("Purchases is not configured; reward verification was not enabled.");
                callback?.Invoke(false);
                return;
            }
            var state = TrackedAdState.For(ad) ?? TrackedAdState.Create(ad, format, adUnitId, null);
            if (string.IsNullOrEmpty(state.ImpressionId)) state.UpdateFromResponse(info);
            if (string.IsNullOrEmpty(state.ImpressionId))
            {
                AdMobTracker.LogWarning("The loaded ad has no response id; reward verification will use an empty impression id.");
            }

            TokenQueue.Enqueue(() => purchases.GenerateRewardVerificationToken(state.ImpressionId, (token, error) =>
            {
                TokenQueue.Completed();
                if (error != null || token == null)
                {
                    AdMobTracker.LogWarning($"Could not enable reward verification: {error}");
                    callback?.Invoke(false);
                    return;
                }
                setOptions(new ServerSideVerificationOptions
                {
                    UserId = token.AppUserID,
                    CustomData = token.CustomData,
                });
                state.ClientTransactionId = token.ClientTransactionId;
                state.RewardVerificationDelivered = false;
                callback?.Invoke(true);
            }));
        }

        private static void OnRewardEarned(
            TrackedAdState state, Reward reward, Action<Reward> userRewardEarnedCallback,
            RewardVerificationStartedFunc started, RewardVerificationCompletedFunc completed)
        {
            userRewardEarnedCallback?.Invoke(reward);
            started?.Invoke();

            void Deliver(Purchases.RewardVerificationResult result)
            {
                if (state != null)
                {
                    if (state.RewardVerificationDelivered) return;
                    state.RewardVerificationDelivered = true;
                }
                completed?.Invoke(result);
            }

            var clientTransactionId = state?.ClientTransactionId;
            if (state == null || string.IsNullOrEmpty(clientTransactionId))
            {
                AdMobTracker.LogWarning(
                    "Reward earned but reward verification was not enabled for this ad. " +
                    "Call EnableRewardVerification() after the ad loads.");
                Deliver(FailedResult);
                return;
            }

            PollQueue.Enqueue(() => Purchases.PollRewardVerification(clientTransactionId, (result, error) =>
            {
                PollQueue.Completed();
                if (error != null || result == null)
                {
                    AdMobTracker.LogWarning($"Reward verification failed: {error}");
                    Deliver(FailedResult);
                }
                else
                {
                    Deliver(result);
                }
            }, state.TrackingMetadata));
        }

        private static readonly Purchases.RewardVerificationResult FailedResult =
            new Purchases.RewardVerificationResult(SimpleJSON.JSON.Parse("{\"failed\":true,\"moreRewards\":[]}"));

        /// <summary>
        /// <see cref="Purchases.GenerateRewardVerificationToken"/> and
        /// <see cref="Purchases.PollRewardVerification"/> each keep a single pending callback, so two
        /// overlapping calls would drop one result. Calls to each are run one at a time instead.
        /// </summary>
        private sealed class SerialQueue
        {
            private readonly Queue<Action> _pending = new Queue<Action>();
            private bool _inFlight;

            public void Enqueue(Action call)
            {
                lock (_pending)
                {
                    if (_inFlight)
                    {
                        _pending.Enqueue(call);
                        return;
                    }
                    _inFlight = true;
                }
                call();
            }

            public void Completed()
            {
                Action next;
                lock (_pending)
                {
                    if (_pending.Count == 0)
                    {
                        _inFlight = false;
                        return;
                    }
                    next = _pending.Dequeue();
                }
                next();
            }
        }

        private static readonly SerialQueue TokenQueue = new SerialQueue();
        private static readonly SerialQueue PollQueue = new SerialQueue();
    }
}

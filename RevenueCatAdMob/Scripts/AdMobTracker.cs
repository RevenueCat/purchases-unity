using GoogleMobileAds.Api;
using UnityEngine;

namespace RevenueCat.AdMob
{
    /// <summary>
    /// Builds RevenueCat ad events from adapter state and forwards them to
    /// <see cref="Purchases.AdTracker"/>. Google Mobile Ads callbacks can fire before Purchases
    /// is configured (an app open ad on a cold start), so every call is guarded.
    /// </summary>
    internal static class AdMobTracker
    {
        private static readonly AdTracker.CaptureMethod CaptureMethod = AdTracker.CaptureMethod.UnityAdMobAdapter;

        internal static void LogWarning(string message) =>
            Debug.LogWarning($"[RevenueCatAdMob] {message}");

        private static AdTracker Tracker(string what)
        {
            var purchases = RevenueCatAdMob.Purchases;
            if (purchases == null)
            {
                LogWarning($"RevenueCatAdMob.Initialize was not called; not tracking {what}.");
                return null;
            }
            if (!purchases.IsConfigured())
            {
                LogWarning($"Purchases is not configured; not tracking {what}.");
                return null;
            }
            return purchases.AdTracker;
        }

        public static void TrackLoaded(TrackedAdState state) =>
            Tracker("ad loaded")?.TrackAdLoaded(new AdLoadedData(
                AdTracker.MediatorName.AdMob, state.AdFormat, state.AdUnitId, state.ImpressionId,
                state.NetworkName, state.LoadPlacement, CaptureMethod));

        public static void TrackFailedToLoad(string adUnitId, AdTracker.Format adFormat, string placement, LoadAdError error) =>
            Tracker("ad failed to load")?.TrackAdFailedToLoad(new AdFailedToLoadData(
                AdTracker.MediatorName.AdMob, adFormat, adUnitId, placement, error?.GetCode(), CaptureMethod));

        public static void TrackDisplayed(TrackedAdState state) =>
            Tracker("ad displayed")?.TrackAdDisplayed(new AdDisplayedData(
                AdTracker.MediatorName.AdMob, state.AdFormat, state.AdUnitId, state.ImpressionId,
                state.NetworkName, state.Placement, CaptureMethod));

        public static void TrackOpened(TrackedAdState state) =>
            Tracker("ad opened")?.TrackAdOpened(new AdOpenedData(
                AdTracker.MediatorName.AdMob, state.AdFormat, state.AdUnitId, state.ImpressionId,
                state.NetworkName, state.Placement, CaptureMethod));

        public static void TrackRevenue(TrackedAdState state, AdValue value) =>
            Tracker("ad revenue")?.TrackAdRevenue(new AdRevenueData(
                AdTracker.MediatorName.AdMob, state.AdFormat, state.AdUnitId, state.ImpressionId,
                value.Value, value.CurrencyCode, PrecisionFrom(value.Precision),
                state.NetworkName, state.Placement, CaptureMethod));

        internal static AdTracker.Precision PrecisionFrom(AdValue.PrecisionType precision)
        {
            switch (precision)
            {
                case AdValue.PrecisionType.Precise: return AdTracker.Precision.Exact;
                case AdValue.PrecisionType.Estimated: return AdTracker.Precision.Estimated;
                case AdValue.PrecisionType.PublisherProvided: return AdTracker.Precision.PublisherDefined;
                default: return AdTracker.Precision.Unknown;
            }
        }
    }
}

using System.Runtime.CompilerServices;
using GoogleMobileAds.Api;

namespace RevenueCat.AdMob
{
    /// <summary>
    /// Per-ad bookkeeping kept in a weak side table, so the adapter never changes the lifetime of
    /// the Google Mobile Ads objects it tracks.
    /// </summary>
    internal sealed class TrackedAdState
    {
        private static readonly ConditionalWeakTable<object, TrackedAdState> States =
            new ConditionalWeakTable<object, TrackedAdState>();

        public AdTracker.Format AdFormat { get; }
        public string AdUnitId { get; }

        /// <summary>Placement given at load time; load-time events always keep it.</summary>
        public string LoadPlacement { get; set; }

        /// <summary>Placement given at show time; overrides the load-time one for later events.</summary>
        public string ShowPlacement { get; set; }

        public string ImpressionId { get; private set; } = "";
        public string NetworkName { get; private set; }

        public string ClientTransactionId { get; set; }
        public bool RewardVerificationDelivered { get; set; }

        public string Placement => ShowPlacement ?? LoadPlacement;

        private TrackedAdState(AdTracker.Format adFormat, string adUnitId, string placement)
        {
            AdFormat = adFormat;
            AdUnitId = adUnitId;
            LoadPlacement = placement;
        }

        public void UpdateFromResponse(ResponseInfo info)
        {
            ImpressionId = info?.GetResponseId() ?? "";
            NetworkName = info?.GetLoadedAdapterResponseInfo()?.AdapterClassName
                          ?? info?.GetMediationAdapterClassName();
        }

        public RewardedAdTrackingMetadata TrackingMetadata => new RewardedAdTrackingMetadata(
            AdTracker.MediatorName.AdMob,
            AdFormat,
            AdUnitId,
            ImpressionId,
            NetworkName,
            Placement,
            AdTracker.CaptureMethod.UnityAdMobAdapter);

        public static TrackedAdState Create(object ad, AdTracker.Format adFormat, string adUnitId, string placement)
        {
            var state = new TrackedAdState(adFormat, adUnitId, placement);
            States.Remove(ad);
            States.Add(ad, state);
            return state;
        }

        public static TrackedAdState For(object ad) =>
            ad != null && States.TryGetValue(ad, out var state) ? state : null;
    }
}

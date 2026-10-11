using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    public class RewardedAdTrackingMetadata
    {
        public AdTracker.MediatorName MediatorName { get; }
        public AdTracker.Format AdFormat { get; }
        public string AdUnitId { get; }
        public string ImpressionId { get; }
        public string NetworkName { get; }
        public string Placement { get; }

        /// <summary>Reserved for the official RevenueCat ad-network adapters.</summary>
        public AdTracker.CaptureMethod CaptureMethod { get; }

        public RewardedAdTrackingMetadata(
            AdTracker.MediatorName mediatorName,
            AdTracker.Format adFormat,
            string adUnitId,
            string impressionId,
            string networkName = null,
            string placement = null,
            AdTracker.CaptureMethod captureMethod = null)
        {
            MediatorName = mediatorName;
            AdFormat = adFormat;
            AdUnitId = adUnitId;
            ImpressionId = impressionId;
            NetworkName = networkName;
            Placement = placement;
            CaptureMethod = captureMethod;
        }

        public override string ToString() =>
            $"{nameof(MediatorName)}: {MediatorName.Value}, " +
            $"{nameof(AdFormat)}: {AdFormat.Value}, " +
            $"{nameof(AdUnitId)}: {AdUnitId}, " +
            $"{nameof(ImpressionId)}: {ImpressionId}, " +
            $"{nameof(NetworkName)}: {NetworkName}, " +
            $"{nameof(Placement)}: {Placement}";

        public string ToJsonString()
        {
            var obj = new JSONObject();
            obj["mediatorName"] = MediatorName.Value;
            obj["adFormat"] = AdFormat.Value;
            obj["adUnitId"] = AdUnitId;
            obj["impressionId"] = ImpressionId;
            if (NetworkName != null) obj["networkName"] = NetworkName;
            if (Placement != null) obj["placement"] = Placement;
            if (CaptureMethod != null) obj["captureMethod"] = CaptureMethod.Value;
            return obj.ToString();
        }
    }
}

using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    public class AdDisplayedData
    {
        public AdTracker.MediatorName MediatorName { get; }
        public AdTracker.Format AdFormat { get; }
        public string AdUnitId { get; }
        public string ImpressionId { get; }
        public string NetworkName { get; }
        public string Placement { get; }

        internal AdTracker.CaptureMethod CaptureMethod { get; }

        public AdDisplayedData(
            AdTracker.MediatorName mediatorName,
            AdTracker.Format adFormat,
            string adUnitId,
            string impressionId,
            string networkName = null,
            string placement = null)
            : this(
                mediatorName,
                adFormat,
                adUnitId,
                impressionId,
                networkName,
                placement,
                null)
        {
        }

        internal AdDisplayedData(
            AdTracker.MediatorName mediatorName,
            AdTracker.Format adFormat,
            string adUnitId,
            string impressionId,
            string networkName,
            string placement,
            AdTracker.CaptureMethod captureMethod)
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

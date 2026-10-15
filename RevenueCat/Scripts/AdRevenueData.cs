using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    public class AdRevenueData
    {
        public AdTracker.MediatorName MediatorName { get; }
        public AdTracker.Format AdFormat { get; }
        public string AdUnitId { get; }
        public string ImpressionId { get; }
        public long RevenueMicros { get; }
        public string Currency { get; }
        public AdTracker.Precision Precision { get; }
        public string NetworkName { get; }
        public string Placement { get; }

        internal AdTracker.CaptureMethod CaptureMethod { get; }

        public AdRevenueData(
            AdTracker.MediatorName mediatorName,
            AdTracker.Format adFormat,
            string adUnitId,
            string impressionId,
            long revenueMicros,
            string currency,
            AdTracker.Precision precision,
            string networkName = null,
            string placement = null)
            : this(
                mediatorName,
                adFormat,
                adUnitId,
                impressionId,
                revenueMicros,
                currency,
                precision,
                networkName,
                placement,
                null)
        {
        }

        internal AdRevenueData(
            AdTracker.MediatorName mediatorName,
            AdTracker.Format adFormat,
            string adUnitId,
            string impressionId,
            long revenueMicros,
            string currency,
            AdTracker.Precision precision,
            string networkName,
            string placement,
            AdTracker.CaptureMethod captureMethod)
        {
            MediatorName = mediatorName;
            AdFormat = adFormat;
            AdUnitId = adUnitId;
            ImpressionId = impressionId;
            RevenueMicros = revenueMicros;
            Currency = currency;
            Precision = precision;
            NetworkName = networkName;
            Placement = placement;
            CaptureMethod = captureMethod;
        }

        public override string ToString() =>
            $"{nameof(MediatorName)}: {MediatorName.Value}, " +
            $"{nameof(AdFormat)}: {AdFormat.Value}, " +
            $"{nameof(AdUnitId)}: {AdUnitId}, " +
            $"{nameof(ImpressionId)}: {ImpressionId}, " +
            $"{nameof(RevenueMicros)}: {RevenueMicros}, " +
            $"{nameof(Currency)}: {Currency}, " +
            $"{nameof(Precision)}: {Precision.Value}, " +
            $"{nameof(NetworkName)}: {NetworkName}, " +
            $"{nameof(Placement)}: {Placement}";

        public string ToJsonString()
        {
            var obj = new JSONObject();
            obj["mediatorName"] = MediatorName.Value;
            obj["adFormat"] = AdFormat.Value;
            obj["adUnitId"] = AdUnitId;
            obj["impressionId"] = ImpressionId;
            obj["revenueMicros"] = RevenueMicros;
            obj["currency"] = Currency;
            obj["precision"] = Precision.Value;
            if (NetworkName != null) obj["networkName"] = NetworkName;
            if (Placement != null) obj["placement"] = Placement;
            if (CaptureMethod != null) obj["captureMethod"] = CaptureMethod.Value;
            return obj.ToString();
        }
    }
}

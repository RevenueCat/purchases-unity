using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
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
        }

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
            return obj.ToString();
        }
    }
}

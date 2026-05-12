using RevenueCat.SimpleJSON;

namespace RevenueCat.AdTracking
{
    public class AdRevenueData
    {
        public AdMediatorName MediatorName { get; }
        public AdFormat AdFormat { get; }
        public string AdUnitId { get; }
        public string ImpressionId { get; }
        public long RevenueMicros { get; }
        public string Currency { get; }
        public AdRevenuePrecision Precision { get; }
        public string NetworkName { get; }
        public string Placement { get; }

        public AdRevenueData(AdMediatorName mediatorName, AdFormat adFormat,
            string adUnitId, string impressionId,
            long revenueMicros, string currency, AdRevenuePrecision precision,
            string networkName = null, string placement = null)
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
            obj["networkName"] = NetworkName;
            obj["placement"] = Placement;
            return obj.ToString();
        }
    }
}

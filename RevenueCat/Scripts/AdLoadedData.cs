using RevenueCat.SimpleJSON;

namespace RevenueCat.AdTracking
{
    public class AdLoadedData
    {
        public AdMediatorName MediatorName { get; }
        public AdFormat AdFormat { get; }
        public string AdUnitId { get; }
        public string ImpressionId { get; }
        public string NetworkName { get; }
        public string Placement { get; }

        public AdLoadedData(AdMediatorName mediatorName, AdFormat adFormat,
            string adUnitId, string impressionId,
            string networkName = null, string placement = null)
        {
            MediatorName = mediatorName;
            AdFormat = adFormat;
            AdUnitId = adUnitId;
            ImpressionId = impressionId;
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
            obj["networkName"] = NetworkName;
            obj["placement"] = Placement;
            return obj.ToString();
        }
    }
}

using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    public class AdOpenedData
    {
        public AdTracker.MediatorName MediatorName { get; }
        public AdTracker.Format AdFormat { get; }
        public string AdUnitId { get; }
        public string ImpressionId { get; }
        public string NetworkName { get; }
        public string Placement { get; }

        public AdOpenedData(AdTracker.MediatorName mediatorName, AdTracker.Format adFormat,
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
            if (NetworkName != null) obj["networkName"] = NetworkName;
            if (Placement != null) obj["placement"] = Placement;
            return obj.ToString();
        }
    }
}

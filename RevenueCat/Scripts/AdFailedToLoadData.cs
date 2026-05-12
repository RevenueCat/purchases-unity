using RevenueCat.SimpleJSON;

namespace RevenueCat.AdTracking
{
    public class AdFailedToLoadData
    {
        public AdMediatorName MediatorName { get; }
        public AdFormat AdFormat { get; }
        public string AdUnitId { get; }
        public string Placement { get; }
        public int? MediatorErrorCode { get; }

        public AdFailedToLoadData(AdMediatorName mediatorName, AdFormat adFormat,
            string adUnitId,
            string placement = null, int? mediatorErrorCode = null)
        {
            MediatorName = mediatorName;
            AdFormat = adFormat;
            AdUnitId = adUnitId;
            Placement = placement;
            MediatorErrorCode = mediatorErrorCode;
        }

        public string ToJsonString()
        {
            var obj = new JSONObject();
            obj["mediatorName"] = MediatorName.Value;
            obj["adFormat"] = AdFormat.Value;
            obj["adUnitId"] = AdUnitId;
            obj["placement"] = Placement;
            obj["mediatorErrorCode"] = MediatorErrorCode.HasValue ? (JSONNode)new JSONNumber(MediatorErrorCode.Value) : JSONNull.CreateOrGet();
            return obj.ToString();
        }
    }
}

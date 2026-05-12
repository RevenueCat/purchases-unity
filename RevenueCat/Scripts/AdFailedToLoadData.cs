using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    public class AdFailedToLoadData
    {
        public AdTracker.MediatorName MediatorName { get; }
        public AdTracker.Format AdFormat { get; }
        public string AdUnitId { get; }
        public string Placement { get; }
        public int? MediatorErrorCode { get; }

        public AdFailedToLoadData(
            AdTracker.MediatorName mediatorName,
            AdTracker.Format adFormat,
            string adUnitId,
            string placement = null,
            int? mediatorErrorCode = null)
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
            if (Placement != null) obj["placement"] = Placement;
            if (MediatorErrorCode.HasValue) obj["mediatorErrorCode"] = MediatorErrorCode.Value;
            return obj.ToString();
        }
    }
}

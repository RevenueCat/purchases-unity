using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
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

        public override string ToString() =>
            $"{nameof(MediatorName)}: {MediatorName.Value}, " +
            $"{nameof(AdFormat)}: {AdFormat.Value}, " +
            $"{nameof(AdUnitId)}: {AdUnitId}, " +
            $"{nameof(Placement)}: {Placement}, " +
            $"{nameof(MediatorErrorCode)}: {MediatorErrorCode}";

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

using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    public class AdFailedToLoadData
    {
        public AdTracker.MediatorName MediatorName { get; }
        public AdTracker.Format AdFormat { get; }
        public string AdUnitId { get; }
        public string Placement { get; }

        /// <summary>Reserved for the official RevenueCat ad-network adapters.</summary>
        public AdTracker.CaptureMethod CaptureMethod { get; }
        public int? MediatorErrorCode { get; }

        public AdFailedToLoadData(
            AdTracker.MediatorName mediatorName,
            AdTracker.Format adFormat,
            string adUnitId,
            string placement = null,
            int? mediatorErrorCode = null,
            AdTracker.CaptureMethod captureMethod = null)
        {
            MediatorName = mediatorName;
            AdFormat = adFormat;
            AdUnitId = adUnitId;
            Placement = placement;
            CaptureMethod = captureMethod;
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
            if (CaptureMethod != null) obj["captureMethod"] = CaptureMethod.Value;
            if (MediatorErrorCode.HasValue) obj["mediatorErrorCode"] = MediatorErrorCode.Value;
            return obj.ToString();
        }
    }
}

using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    public class AdFailedToLoadData
    {
        public AdTracker.MediatorName MediatorName { get; }
        public AdTracker.Format AdFormat { get; }
        public string AdUnitId { get; }
        public string Placement { get; }

        internal AdTracker.CaptureMethod CaptureMethod { get; }
        public int? MediatorErrorCode { get; }

        public AdFailedToLoadData(
            AdTracker.MediatorName mediatorName,
            AdTracker.Format adFormat,
            string adUnitId,
            string placement = null,
            int? mediatorErrorCode = null)
            : this(
                mediatorName,
                adFormat,
                adUnitId,
                placement,
                mediatorErrorCode,
                null)
        {
        }

        internal AdFailedToLoadData(
            AdTracker.MediatorName mediatorName,
            AdTracker.Format adFormat,
            string adUnitId,
            string placement,
            int? mediatorErrorCode,
            AdTracker.CaptureMethod captureMethod)
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

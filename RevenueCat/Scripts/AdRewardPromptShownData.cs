using RevenueCat.SimpleJSON;

namespace RevenueCat
{
    public class AdRewardPromptShownData
    {
        public AdTracker.MediatorName MediatorName { get; }
        public string AdUnitId { get; }
        public string Placement { get; }

        public AdRewardPromptShownData(
            AdTracker.MediatorName mediatorName,
            string adUnitId,
            string placement = null)
        {
            MediatorName = mediatorName;
            AdUnitId = adUnitId;
            Placement = placement;
        }

        public override string ToString() =>
            $"{nameof(MediatorName)}: {MediatorName.Value}, " +
            $"{nameof(AdUnitId)}: {AdUnitId}, " +
            $"{nameof(Placement)}: {Placement}";

        public string ToJsonString()
        {
            var obj = new JSONObject();
            obj["mediatorName"] = MediatorName.Value;
            obj["adUnitId"] = AdUnitId;
            if (Placement != null) obj["placement"] = Placement;
            return obj.ToString();
        }
    }
}

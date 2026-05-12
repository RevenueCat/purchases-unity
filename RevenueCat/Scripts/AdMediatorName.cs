namespace RevenueCat.AdTracking
{
    public class AdMediatorName
    {
        public string Value { get; }

        public AdMediatorName(string value)
        {
            Value = value;
        }

        public static readonly AdMediatorName AdMob = new AdMediatorName("AdMob");
        public static readonly AdMediatorName AppLovin = new AdMediatorName("AppLovin");
    }
}

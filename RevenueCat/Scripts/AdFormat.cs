namespace RevenueCat.AdTracking
{
    public class AdFormat
    {
        public string Value { get; }

        public AdFormat(string value)
        {
            Value = value;
        }

        public static readonly AdFormat Banner = new AdFormat("banner");
        public static readonly AdFormat Interstitial = new AdFormat("interstitial");
        public static readonly AdFormat Rewarded = new AdFormat("rewarded");
        public static readonly AdFormat RewardedInterstitial = new AdFormat("rewarded_interstitial");
        public static readonly AdFormat Native = new AdFormat("native");
        public static readonly AdFormat AppOpen = new AdFormat("app_open");
        public static readonly AdFormat Other = new AdFormat("other");
    }
}

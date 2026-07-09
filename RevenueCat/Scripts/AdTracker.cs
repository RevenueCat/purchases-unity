namespace RevenueCat
{
    /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
    public class AdTracker
    {
        /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
        public class MediatorName
        {
            public string Value { get; }

            public MediatorName(string value)
            {
                Value = value;
            }

            public static readonly MediatorName AdMob = new MediatorName("AdMob");
            public static readonly MediatorName AppLovin = new MediatorName("AppLovin");

            public override bool Equals(object obj) => obj is MediatorName other && other.Value == Value;
            public override int GetHashCode() => Value?.GetHashCode() ?? 0;
            public static bool operator ==(MediatorName a, MediatorName b) => Equals(a, b);
            public static bool operator !=(MediatorName a, MediatorName b) => !Equals(a, b);
        }

        /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
        public class Format
        {
            public string Value { get; }

            public Format(string value)
            {
                Value = value;
            }

            public static readonly Format Banner = new Format("banner");
            public static readonly Format Interstitial = new Format("interstitial");
            public static readonly Format Rewarded = new Format("rewarded");
            public static readonly Format RewardedInterstitial = new Format("rewarded_interstitial");
            public static readonly Format Native = new Format("native");
            public static readonly Format AppOpen = new Format("app_open");
            public static readonly Format Other = new Format("other");

            public override bool Equals(object obj) => obj is Format other && other.Value == Value;
            public override int GetHashCode() => Value?.GetHashCode() ?? 0;
            public static bool operator ==(Format a, Format b) => Equals(a, b);
            public static bool operator !=(Format a, Format b) => !Equals(a, b);
        }

        /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
        public class Precision
        {
            public string Value { get; }

            public Precision(string value)
            {
                Value = value;
            }

            public static readonly Precision Exact = new Precision("exact");
            public static readonly Precision PublisherDefined = new Precision("publisher_defined");
            public static readonly Precision Estimated = new Precision("estimated");
            public static readonly Precision Unknown = new Precision("unknown");

            public override bool Equals(object obj) => obj is Precision other && other.Value == Value;
            public override int GetHashCode() => Value?.GetHashCode() ?? 0;
            public static bool operator ==(Precision a, Precision b) => Equals(a, b);
            public static bool operator !=(Precision a, Precision b) => !Equals(a, b);
        }

        private readonly IPurchasesWrapper _wrapper;

        internal AdTracker(IPurchasesWrapper wrapper)
        {
            _wrapper = wrapper;
        }

        public void TrackAdDisplayed(AdDisplayedData data) => _wrapper.TrackAdDisplayed(data);
        public void TrackAdOpened(AdOpenedData data) => _wrapper.TrackAdOpened(data);
        public void TrackAdRevenue(AdRevenueData data) => _wrapper.TrackAdRevenue(data);
        public void TrackAdLoaded(AdLoadedData data) => _wrapper.TrackAdLoaded(data);
        public void TrackAdFailedToLoad(AdFailedToLoadData data) => _wrapper.TrackAdFailedToLoad(data);
    }
}

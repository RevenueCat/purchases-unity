namespace RevenueCat.AdTracking
{
    public class AdRevenuePrecision
    {
        public string Value { get; }

        public AdRevenuePrecision(string value)
        {
            Value = value;
        }

        public static readonly AdRevenuePrecision Exact = new AdRevenuePrecision("exact");
        public static readonly AdRevenuePrecision PublisherDefined = new AdRevenuePrecision("publisher_defined");
        public static readonly AdRevenuePrecision Estimated = new AdRevenuePrecision("estimated");
        public static readonly AdRevenuePrecision Unknown = new AdRevenuePrecision("unknown");
    }
}

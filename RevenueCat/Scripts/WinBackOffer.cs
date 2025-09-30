using Newtonsoft.Json;

namespace RevenueCat
{
    /// <summary>
    /// iOS only. Requires StoreKit 2 and iOS 18.0+. Describes a win-back offer that you configured in App Store Connect.
    /// </summary>
    public sealed class WinBackOffer
    {
        /// <summary>
        /// Identifier of the discount.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Price in the local currency.
        /// </summary>
        public float Price { get; }

        /// <summary>
        /// Formatted price, including its currency sign, such as €3.99.
        /// </summary>
        public string PriceString { get; }

        /// <summary>
        /// Number of subscription billing periods for which the user will be given the discount, such as 3.
        /// </summary>
        public int Cycles { get; }

        /// <summary>
        /// Billing period of the discount, specified in ISO 8601 format.
        /// </summary>
        public string Period { get; }

        /// <summary>
        /// Unit for the billing period of the discount, can be DAY, WEEK, MONTH or YEAR.
        /// </summary>
        public string PeriodUnit { get; }

        /// <summary>
        /// Number of units for the billing period of the discount.
        /// </summary>
        public int PeriodNumberOfUnits { get; }

        [JsonConstructor]
        internal WinBackOffer(
            [JsonProperty("identifier")] string identifier,
            [JsonProperty("price")] float price,
            [JsonProperty("priceString")] string priceString,
            [JsonProperty("cycles")] int cycles,
            [JsonProperty("period")] string period,
            [JsonProperty("periodUnit")] string periodUnit,
            [JsonProperty("periodNumberOfUnits")] int periodNumberOfUnits)
        {
            Identifier = identifier;
            Price = price;
            PriceString = priceString;
            Cycles = cycles;
            Period = period;
            PeriodUnit = periodUnit;
            PeriodNumberOfUnits = periodNumberOfUnits;
        }

        public override string ToString()
        {
            return $"{nameof(Identifier)}: {Identifier}\n" +
                   $"{nameof(Price)}: {Price}\n" +
                   $"{nameof(PriceString)}: {PriceString}\n" +
                   $"{nameof(Cycles)}: {Cycles}\n" +
                   $"{nameof(Period)}: {Period}\n" +
                   $"{nameof(PeriodUnit)}: {PeriodUnit}\n" +
                   $"{nameof(PeriodNumberOfUnits)}: {PeriodNumberOfUnits}";
        }
    }
}
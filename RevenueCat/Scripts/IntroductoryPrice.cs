using Newtonsoft.Json;

namespace RevenueCat
{
    public sealed class IntroductoryPrice
    {
        public float Price { get; }
        public string PriceString { get; }
        public string Period { get; }
        public string Unit { get; }
        public int NumberOfUnits { get; }
        public int Cycles { get; }

        [JsonConstructor]
        internal IntroductoryPrice(
            [JsonProperty("price")] float price,
            [JsonProperty("priceString")] string priceString,
            [JsonProperty("period")] string period,
            [JsonProperty("unit")] string unit,
            [JsonProperty("numberOfUnits")] int numberOfUnits,
            [JsonProperty("cycles")] int cycles)
        {
            Price = price;
            PriceString = priceString;
            Period = period;
            Unit = unit;
            NumberOfUnits = numberOfUnits;
            Cycles = cycles;
        }

        public override string ToString()
        {
            return $"{nameof(Price)}: {Price}\n" +
                   $"{nameof(PriceString)}: {PriceString}\n" +
                   $"{nameof(Period)}: {Period}\n" +
                   $"{nameof(Unit)}: {Unit}\n" +
                   $"{nameof(NumberOfUnits)}: {NumberOfUnits}\n" +
                   $"{nameof(Cycles)}: {Cycles}";
        }
    }
}
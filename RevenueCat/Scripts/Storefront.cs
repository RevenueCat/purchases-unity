using Newtonsoft.Json;

namespace RevenueCat
{
    /// <summary>
    /// Contains the information about the current store account.
    /// </summary>
    public sealed class Storefront
    {
        /// <summary>
        /// Country code of the current store account.
        /// </summary>
        [JsonProperty("countryCode")]
        public string CountryCode { get; }

        [JsonConstructor]
        internal Storefront([JsonProperty("countryCode")] string countryCode)
        {
            CountryCode = countryCode;
        }

        public override string ToString()
        {
            return $"{nameof(CountryCode)}: {CountryCode}";
        }
    }
}
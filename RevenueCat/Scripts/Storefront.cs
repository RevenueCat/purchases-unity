using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// Contains the information about the current store account.
    /// </summary>
    public class Storefront
    {
        /// <summary>
        /// Country code of the current store account.
        /// </summary>
        public readonly string CountryCode;

        public Storefront(string countryCode)
        {
            CountryCode = countryCode;
        }

        public override string ToString()
        {
            return $"{nameof(CountryCode)}: {CountryCode}";
        }
    }
}
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class Discount
    {
        /// <summary>
        /// Identifier of the discount.
        /// </summary>
        public string identifier;

        /// <summary>
        /// Price in the local currency.
        /// </summary>
        public float price;

        /// <summary>
        /// Formatted price, including its currency sign, such as €3.99.
        /// </summary>
        public string priceString;

        /// <summary>
        /// Number of subscription billing periods for which the user will be given the discount, such as 3.
        /// </summary>
        public int cycles;

        /// <summary>
        /// Billing period of the discount, specified in ISO 8601 format.
        /// </summary>
        public string period;

        /// <summary>
        /// Unit for the billing period of the discount, can be DAY, WEEK, MONTH or YEAR.
        /// </summary>
        public string periodUnit;

        /// <summary>
        /// Number of units for the billing period of the discount.
        /// </summary>
        public int periodNumberOfUnits;

        public Discount(JSONNode response)
        {
            identifier = response["identifier"];
            price = response["price"];
            priceString = response["priceString"];
            cycles = response["cycles"];
            period = response["period"];
            periodUnit = response["periodUnit"];
            periodNumberOfUnits = response["periodNumberOfUnits"];
        }

        public override string ToString()
        {
            return $"{nameof(identifier)}: {identifier}, " +
                   $"{nameof(price)}: {price}, " +
                   $"{nameof(priceString)}: {priceString}, " +
                   $"{nameof(cycles)}: {cycles}, " +
                   $"{nameof(period)}: {period}, " +
                   $"{nameof(periodUnit)}: {periodUnit}, " +
                   $"{nameof(periodNumberOfUnits)}: {periodNumberOfUnits}";
        }
    }
}
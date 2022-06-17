using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class Discount
    {
        /// <summary>
        /// Identifier of the discount.
        /// </summary>
        public readonly string Identifier;

        /// <summary>
        /// Price in the local currency.
        /// </summary>
        public readonly float Price;

        /// <summary>
        /// Formatted price, including its currency sign, such as €3.99.
        /// </summary>
        public readonly string PriceString;

        /// <summary>
        /// Number of subscription billing periods for which the user will be given the discount, such as 3.
        /// </summary>
        public readonly int Cycles;

        /// <summary>
        /// Billing period of the discount, specified in ISO 8601 format.
        /// </summary>
        public readonly string Period;

        /// <summary>
        /// Unit for the billing period of the discount, can be DAY, WEEK, MONTH or YEAR.
        /// </summary>
        public readonly string PeriodUnit;

        /// <summary>
        /// Number of units for the billing period of the discount.
        /// </summary>
        public readonly int PeriodNumberOfUnits;

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
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
            Identifier = response["identifier"];
            Price = response["price"];
            PriceString = response["priceString"];
            Cycles = response["cycles"];
            Period = response["period"];
            PeriodUnit = response["periodUnit"];
            PeriodNumberOfUnits = response["periodNumberOfUnits"];
        }

        public override string ToString()
        {
            return $"{nameof(Identifier)}: {Identifier}, " +
                   $"{nameof(Price)}: {Price}, " +
                   $"{nameof(PriceString)}: {PriceString}, " +
                   $"{nameof(Cycles)}: {Cycles}, " +
                   $"{nameof(Period)}: {Period}, " +
                   $"{nameof(PeriodUnit)}: {PeriodUnit}, " +
                   $"{nameof(PeriodNumberOfUnits)}: {PeriodNumberOfUnits}";
        }
    }
}
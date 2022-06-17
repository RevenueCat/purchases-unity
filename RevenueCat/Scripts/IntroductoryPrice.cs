using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class IntroductoryPrice
    {
        public float Price;
        public string PriceString;
        public string Period;
        public string Unit;
        public int NumberOfUnits;
        public int Cycles;

        public IntroductoryPrice(JSONNode response)
        {
            Price = response["price"];
            PriceString = response["priceString"];
            Period = response["period"];
            Unit = response["periodUnit"];
            NumberOfUnits = response["periodNumberOfUnits"];
            Cycles = response["cycles"];
        }

        public override string ToString()
        {
            return $"{nameof(Price)}: {Price}, " +
                   $"{nameof(PriceString)}: {PriceString}, " +
                   $"{nameof(Period)}: {Period}, " +
                   $"{nameof(Unit)}: {Unit}, " +
                   $"{nameof(NumberOfUnits)}: {NumberOfUnits}, " +
                   $"{nameof(Cycles)}: {Cycles}";
        }
    }
}
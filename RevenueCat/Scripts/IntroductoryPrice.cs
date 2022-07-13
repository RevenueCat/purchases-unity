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
            return $"{nameof(Price)}: {Price}\n" +
                   $"{nameof(PriceString)}: {PriceString}\n" +
                   $"{nameof(Period)}: {Period}\n" +
                   $"{nameof(Unit)}: {Unit}\n" +
                   $"{nameof(NumberOfUnits)}: {NumberOfUnits}\n" +
                   $"{nameof(Cycles)}: {Cycles}";
        }
    }
}
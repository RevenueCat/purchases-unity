using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class StoreProduct
    {
        public readonly string Title;
        public readonly string Identifier;
        public readonly string Description;
        public readonly float Price;
        public readonly string PriceString;
        [CanBeNull] public readonly string CurrencyCode;
        public readonly float IntroPrice;
        public readonly string IntroPriceString;
        public readonly string IntroPricePeriod;
        public readonly string IntroPricePeriodUnit;
        public readonly int IntroPricePeriodNumberOfUnits;
        public readonly int IntroPriceCycles;
        /// <summary>
        /// Collection of iOS promotional offers for a product. Null for Android.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public readonly Discount[] Discounts;
            
        public StoreProduct(JSONNode response)
        {
            Title = response["title"];
            Identifier = response["identifier"];
            Description = response["description"];
            Price = response["price"];
            PriceString = response["price_string"];
            CurrencyCode = response["currency_code"];
            var introPriceDict = response["intro_price"];
            IntroPrice = introPriceDict["price"];
            IntroPriceString = introPriceDict["introPriceString"];
            IntroPricePeriod = introPriceDict["introPricePeriod"];
            IntroPricePeriodUnit = introPriceDict["introPricePeriodUnit"];
            IntroPricePeriodNumberOfUnits = introPriceDict["introPricePeriodNumberOfUnits"];
            IntroPriceCycles = introPriceDict["introPriceCycles"];
            
            var discountsResponse = response["discounts"];
            if (discountsResponse == null)
            {
                Discounts = null;
                return;
            }
            var temporaryList = new List<Discount>();
            foreach (var discountResponse in discountsResponse)
            {
                temporaryList.Add(new Discount(discountResponse));
            }
            Discounts = temporaryList.ToArray();
        }

        public override string ToString()
        {
            return $"{nameof(Title)}: {Title}, " +
                   $"{nameof(Identifier)}: {Identifier}, " +
                   $"{nameof(Description)}: {Description}, " +
                   $"{nameof(Price)}: {Price}, " +
                   $"{nameof(PriceString)}: {PriceString}, " +
                   $"{nameof(CurrencyCode)}: {CurrencyCode}, " +
                   $"{nameof(IntroPrice)}: {IntroPrice}, " +
                   $"{nameof(IntroPriceString)}: {IntroPriceString}, " +
                   $"{nameof(IntroPricePeriod)}: {IntroPricePeriod}, " +
                   $"{nameof(IntroPricePeriodUnit)}: {IntroPricePeriodUnit}, " +
                   $"{nameof(IntroPricePeriodNumberOfUnits)}: {IntroPricePeriodNumberOfUnits}, " +
                   $"{nameof(IntroPriceCycles)}: {IntroPriceCycles}, " +
                   $"{nameof(Discounts)}: {Discounts}";
        }
    }
}
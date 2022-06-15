using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class StoreProduct
    {
        public string title;
        public string identifier;
        public string description;
        public float price;
        public string priceString;
        [CanBeNull] public string currencyCode;
        public float introPrice;
        public string introPriceString;
        public string introPricePeriod;
        public string introPricePeriodUnit;
        public int introPricePeriodNumberOfUnits;
        public int introPriceCycles;
        /// <summary>
        /// Collection of iOS promotional offers for a product. Null for Android.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public Discount[] discounts;
            
        public StoreProduct(JSONNode response)
        {
            title = response["title"];
            identifier = response["identifier"];
            description = response["description"];
            price = response["price"];
            priceString = response["price_string"];
            currencyCode = response["currency_code"];
            var introPriceDict = response["intro_price"];
            introPrice = introPriceDict["price"];
            introPriceString = introPriceDict["introPriceString"];
            introPricePeriod = introPriceDict["introPricePeriod"];
            introPricePeriodUnit = introPriceDict["introPricePeriodUnit"];
            introPricePeriodNumberOfUnits = introPriceDict["introPricePeriodNumberOfUnits"];
            introPriceCycles = introPriceDict["introPriceCycles"];
            
            var discountsResponse = response["discounts"];
            if (discountsResponse == null)
            {
                discounts = null;
                return;
            }
            var temporaryList = new List<Discount>();
            foreach (var discountResponse in discountsResponse)
            {
                temporaryList.Add(new Discount(discountResponse));
            }
            discounts = temporaryList.ToArray();
        }

        public override string ToString()
        {
            return $"{nameof(title)}: {title}, " +
                   $"{nameof(identifier)}: {identifier}, " +
                   $"{nameof(description)}: {description}, " +
                   $"{nameof(price)}: {price}, " +
                   $"{nameof(priceString)}: {priceString}, " +
                   $"{nameof(currencyCode)}: {currencyCode}, " +
                   $"{nameof(introPrice)}: {introPrice}, " +
                   $"{nameof(introPriceString)}: {introPriceString}, " +
                   $"{nameof(introPricePeriod)}: {introPricePeriod}, " +
                   $"{nameof(introPricePeriodUnit)}: {introPricePeriodUnit}, " +
                   $"{nameof(introPricePeriodNumberOfUnits)}: {introPricePeriodNumberOfUnits}, " +
                   $"{nameof(introPriceCycles)}: {introPriceCycles}, " +
                   $"{nameof(discounts)}: {discounts}";
        }
    }
}
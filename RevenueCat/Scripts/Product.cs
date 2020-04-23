using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class Product
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

        public Product(JSONNode response)
        {
            title = response["title"];
            identifier = response["identifier"];
            description = response["description"];
            price = response["price"];
            priceString = response["price_string"];
            currencyCode = response["currency_code"];
            introPrice = response["intro_price"];
            introPriceString = response["intro_price_string"];
            introPricePeriod = response["intro_price_period"];
            introPricePeriodUnit = response["intro_price_period_unit"];
            introPricePeriodNumberOfUnits = response["intro_price_period_number_of_units"];
            introPriceCycles = response["intro_price_cycles"];
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
                   $"{nameof(introPriceCycles)}: {introPriceCycles}";
        }
    }
}
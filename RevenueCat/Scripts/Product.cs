using System;
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
        
        public IntroductoryPrice IntroductoryPrice;
        
        [Obsolete("Use the IntroductoryPrice.Price property instead.")]
        public float introPrice;
        [Obsolete("Use the IntroductoryPrice.PriceString property instead.")]
        public string introPriceString;
        [Obsolete("Use the IntroductoryPrice.Period property instead.")]
        public string introPricePeriod;
        [Obsolete("Use the IntroductoryPrice.Unit property instead.")]
        public string introPricePeriodUnit;
        [Obsolete("Use the IntroductoryPrice.NumberOfUnits property instead.")]
        public int introPricePeriodNumberOfUnits;
        [Obsolete("Use the IntroductoryPrice.Cycles property instead.")]
        public int introPriceCycles;

        public Product(JSONNode response)
        {
            title = response["title"];
            identifier = response["identifier"];
            description = response["description"];
            price = response["price"];
            priceString = response["price_string"];
            currencyCode = response["currency_code"];
            
            var introPriceJson = response["introPrice"];
            introPrice = introPriceJson["price"];
            introPriceString = introPriceJson["priceString"];
            introPricePeriod = introPriceJson["period"];
            introPricePeriodUnit = introPriceJson["periodUnit"];
            introPricePeriodNumberOfUnits = introPriceJson["periodNumberOfUnits"];
            introPriceCycles = introPriceJson["cycles"];
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
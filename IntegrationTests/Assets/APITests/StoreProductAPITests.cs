using System;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class StoreProductAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.StoreProduct storeProduct = new Purchases.StoreProduct(null);
            string title = storeProduct.Title;
            string identifier = storeProduct.Identifier;
            string description = storeProduct.Description;
            float price = storeProduct.Price;
            string priceString = storeProduct.PriceString;
            string currencyCode = storeProduct.CurrencyCode;
            float introPrice = storeProduct.IntroductoryPrice.Price;
            string introPriceString = storeProduct.IntroductoryPrice.PriceString;
            string introPricePeriod = storeProduct.IntroductoryPrice.Period;
            string introPricePeriodUnit = storeProduct.IntroductoryPrice.Unit;
            int introPricePeriodNumberOfUnits = storeProduct.IntroductoryPrice.NumberOfUnits;
            int introPriceCycles = storeProduct.IntroductoryPrice.Cycles; 
            Purchases.ProductCategory ProductCategory = storeProduct.ProductCategory;
            Purchases.SubscriptionOption DefaultOption = storeProduct.DefaultOption;
            Purchases.SubscriptionOption SubscriptionOption = storeProduct.SubscriptionOptions.First();
            string PresentedOfferingIdentifier = storeProduct.PresentedOfferingIdentifier;
            Purchases.Discount[] discounts = storeProduct.Discounts;
            string subscriptionPeriod = storeProduct.SubscriptionPeriod;
        }
    }
}
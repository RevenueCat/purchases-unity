using System;
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
            float introPrice = storeProduct.IntroPrice;
            string introPriceString = storeProduct.IntroPriceString;
            string introPricePeriod = storeProduct.IntroPricePeriod;
            string introPricePeriodUnit = storeProduct.IntroPricePeriodUnit;
            int introPricePeriodNumberOfUnits = storeProduct.IntroPricePeriodNumberOfUnits;
            int introPriceCycles = storeProduct.IntroPriceCycles;
            Purchases.Discount[] discounts = storeProduct.Discounts;
        }
    }
}
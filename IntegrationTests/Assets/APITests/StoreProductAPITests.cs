using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class StoreProductAPITests : MonoBehaviour
    {
        private void Start()
        {
            // todo: update to Uppercase + readonly
            Purchases.StoreProduct storeProduct = new Purchases.StoreProduct(null);
            string title = storeProduct.title;
            string identifier = storeProduct.identifier;
            string description = storeProduct.description;
            float price = storeProduct.price;
            string priceString = storeProduct.priceString;
            string currencyCode = storeProduct.currencyCode;
            float introPrice = storeProduct.introPrice;
            string introPriceString = storeProduct.introPriceString;
            string introPricePeriod = storeProduct.introPricePeriod;
            string introPricePeriodUnit = storeProduct.introPricePeriodUnit;
            int introPricePeriodNumberOfUnits = storeProduct.introPricePeriodNumberOfUnits;
            int introPriceCycles = storeProduct.introPriceCycles;
            Purchases.Discount[] discounts = storeProduct.discounts;
        }
    }
}
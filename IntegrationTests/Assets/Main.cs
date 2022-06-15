using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;


public class CustomListener : Purchases.UpdatedCustomerInfoListener
{
    public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        throw new System.NotImplementedException();
    }
}

public class Main : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.SetDebugLogsEnabled(true);
        purchases.deprecatedLegacyRevenueCatAPIKey = "abc";
        purchases.revenueCatAPIKeyApple = "def";
        purchases.revenueCatAPIKeyGoogle = "ghi";
        purchases.appUserID = "abc";
        purchases.productIdentifiers = new[] { "a", "b", "c" };
        purchases.listener = new CustomListener();
        purchases.observerMode = true;
        purchases.userDefaultsSuiteName = "suitename";
        purchases.proxyURL = "https://proxy-url.revenuecat.com";

        Purchases.CustomerInfo receivedCustomerInfo;
        Purchases.Error receivedError;

        Purchases.Package receivedPackage;
        Purchases.Offerings receivedOfferings;
        Purchases.Offering receivedOffering;
        String receivedProductIdentifier;
        bool receivedUserCancelled;

        List<Purchases.StoreProduct> receivedProducts = new List<Purchases.StoreProduct>();

        List<Purchases.StoreProduct> storeProducts = new List<Purchases.StoreProduct>();
        purchases.GetProducts(new string[] { "a", "b" }, (products, error) => { receivedProducts = products; });

        purchases.GetProducts(new string[] { "a", "b" }, (products, error) => { receivedProducts = products; }, "type");

        purchases.GetOfferings((offerings, error) =>
        {
            receivedOfferings = offerings;
            receivedError = error;
            receivedOffering = receivedOfferings.All.First().Value;

            receivedPackage = receivedOffering.AvailablePackages.First();
            purchases.PurchasePackage(receivedPackage, (productIdentifier, customerInfo, userCancelled, error2) =>
            {
                receivedProductIdentifier = productIdentifier;
                receivedCustomerInfo = customerInfo;
                receivedUserCancelled = userCancelled;
                receivedError = error2;
            });

            purchases.PurchasePackage(receivedPackage, (productIdentifier, customerInfo, userCancelled, error2) =>
            {
                receivedProductIdentifier = productIdentifier;
                receivedCustomerInfo = customerInfo;
                receivedUserCancelled = userCancelled;
                receivedError = error2;
            }, "oldSku", Purchases.ProrationMode.Deferred);

            Purchases.StoreProduct storeProduct = storeProducts.First();
            Purchases.PromotionalOffer receivedPromoOffer;
            purchases.GetPromotionalOffer(storeProduct, storeProduct.discounts.First(), (offer, error2) =>
            {
                receivedPromoOffer = offer;
                receivedError = error2;

                purchases.PurchaseDiscountedPackage(receivedPackage, receivedPromoOffer,
                    (productIdentifier, purchaserInfo, userCancelled, error3) =>
                    {
                        receivedProductIdentifier = productIdentifier;
                        receivedCustomerInfo = purchaserInfo;
                        receivedUserCancelled = userCancelled;
                        receivedError = error3;
                    });
                
                purchases.PurchaseDiscountedProduct("product_id", receivedPromoOffer,
                    (productIdentifier, purchaserInfo, userCancelled, error3) =>
                    {
                        receivedProductIdentifier = productIdentifier;
                        receivedCustomerInfo = purchaserInfo;
                        receivedUserCancelled = userCancelled;
                        receivedError = error3;
                    });
            });
        });

        purchases.PurchaseProduct("product_id", (productIdentifier, customerInfo, userCancelled, error2) =>
        {
            receivedProductIdentifier = productIdentifier;
            receivedCustomerInfo = customerInfo;
            receivedUserCancelled = userCancelled;
            receivedError = error2;
        });

        purchases.PurchaseProduct("product_id", (productIdentifier, purchaserInfo, userCancelled, error2) =>
        {
            receivedProductIdentifier = productIdentifier;
            receivedCustomerInfo = purchaserInfo;
            receivedUserCancelled = userCancelled;
            receivedError = error2;
        }, "type", "oldSku", Purchases.ProrationMode.Deferred);

        purchases.GetCustomerInfo((info, error) =>
        {
            receivedCustomerInfo = info;
            receivedError = error;
        });
        
        purchases.RestorePurchases((customerInfo, error) =>
        {
            receivedCustomerInfo = customerInfo;
            receivedError = error;
        });
        
    }
}
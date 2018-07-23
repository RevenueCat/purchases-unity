using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PurchasesWrapperAndroid : PurchasesWrapper
{
    public void GetProducts(string[] productIdentifiers, string type = "subs")
    {
		ProductsRequest request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper")) {
            purchases.CallStatic("getProductInfo", JsonUtility.ToJson(request), type);
        }
    }

    private class ProductsRequest
    {
        public string[] productIdentifiers;
    }

    public void MakePurchase(string productIdentifier, string type = "subs")
    {

    }

    public void Setup(string gameObject, string apiKey, string appUserID)
    {
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper")) {
            purchases.CallStatic("setup", apiKey, appUserID, gameObject);
        }
    }
}
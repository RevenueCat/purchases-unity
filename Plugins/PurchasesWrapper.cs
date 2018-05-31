using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PurchasesWrapper
{
    [DllImport("__Internal")]
    private static extern void _RCSetupPurchases(string gameObject, string apiKey, string appUserID);
    public static void Setup(string gameObject, string apiKey, string appUserID)
    {
        _RCSetupPurchases(gameObject, apiKey, appUserID);
    }

    public class ProductsRequest
    {
        public string[] productIdentifiers;
    }

    [DllImport("__Internal")]
    private static extern void _RCGetProducts(string productIdentifiersJSON, string type);
    public static void GetProducts(string[] productIdentifiers, string type = "subs")
    {
        ProductsRequest request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };

        _RCGetProducts(JsonUtility.ToJson(request), type);
    }

    [DllImport("__Internal")]
    private static extern void _RCMakePurchase(string productIdentifier, string type);
    public static void MakePurchase(string productIdentifier, string type = "subs")
    {
        _RCMakePurchase(productIdentifier, type);
    }
}
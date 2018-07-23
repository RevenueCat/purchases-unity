using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PurchasesWrapperAndroid : PurchasesWrapper
{
    public void GetProducts(string[] productIdentifiers, string type = "subs")
    {
        throw new NotImplementedException();
    }

    public void MakePurchase(string productIdentifier, string type = "subs")
    {
        throw new NotImplementedException();
    }

    public void Setup(string gameObject, string apiKey, string appUserID)
    {
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper")) {
            purchases.CallStatic("setup");
        }
    }
}
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PurchasesWrapperAndroid : PurchasesWrapper
{
    private class ProductsRequest
    {
        public string[] productIdentifiers;
    }

    public void GetProducts(string[] productIdentifiers, string type = "subs")
    {
		ProductsRequest request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper")) {
            purchases.CallStatic("getProducts", JsonUtility.ToJson(request), type);
        }
    }

	public void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null)
    {
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
			if (oldSku == null) {
				purchases.CallStatic("makePurchase", productIdentifier, type);
			} else {
				purchases.CallStatic("makePurchase", productIdentifier, type, oldSku);
			}
        }
    }

    public void Setup(string gameObject, string apiKey, string appUserID)
    {
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper")) {
            purchases.CallStatic("setup", apiKey, appUserID, gameObject);
        }
    }

	public void RestoreTransactions()
    {
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("restoreTransactions");
        }
    }

    public void AddAttributionData(int network, string data)
    {
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("addAttributionData", data, network);
        }
    }

    public void CreateAlias(string newAppUserID)
    {
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("createAlias", newAppUserID);
        }
    }

    public void Identify(string appUserID)
    {
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("identify", appUserID);
        }
    }
    
    public void Reset()
    {
        using (AndroidJavaClass purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("reset");
        }
    }

    public void SetFinishTransactions(bool finishTransactions)
    {
        // NOOP for now
    }

}
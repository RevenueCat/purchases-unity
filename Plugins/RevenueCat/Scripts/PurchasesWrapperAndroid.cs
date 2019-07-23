using System.Diagnostics.CodeAnalysis;
using UnityEngine;

#if UNITY_ANDROID
public class PurchasesWrapperAndroid : IPurchasesWrapper
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class ProductsRequest
    {
        public string[] productIdentifiers;
    }

    public void GetProducts(string[] productIdentifiers, string type = "subs")
    {
		var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper")) {
            purchases.CallStatic("getProducts", JsonUtility.ToJson(request), type);
        }
    }

	public void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
			if (oldSku == null) {
				purchases.CallStatic("makePurchase", productIdentifier, type);
			} else {
				purchases.CallStatic("makePurchase", productIdentifier, type, oldSku);
			}
        }
    }

    public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper")) {
            purchases.CallStatic("setup", apiKey, appUserId, gameObject, observerMode);
        }
    }

	public void RestoreTransactions()
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("restoreTransactions");
        }
    }

    public void AddAttributionData(int network, string data, string networkUserId)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("addAttributionData", data, network, networkUserId);
        }
    }

    public void CreateAlias(string newAppUserId)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("createAlias", newAppUserId);
        }
    }

    public void Identify(string appUserId)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("identify", appUserId);
        }
    }
    
    public void Reset()
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("reset");
        }
    }

    public void SetFinishTransactions(bool finishTransactions)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("setFinishTransactions", finishTransactions);
        }
    }

    public void SetAllowSharingStoreAccount(bool allow)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("setAllowSharingStoreAccount", allow);
        }
    }

    public void SetDebugLogsEnabled(bool enabled)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("setDebugLogsEnabled", enabled);
        }
    }

    public string GetAppUserId()
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            return purchases.CallStatic<string>("getAppUserID");
        }
    }

    public void GetPurchaserInfo()
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("getPurchaserInfo");
        }
    }

    public void GetEntitlements()
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("getEntitlements");
        }
    }

    public void SyncPurchases()
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("syncPurchases");
        }
    }

    public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
    {
        // NOOP
    }
}
#endif
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using System.Collections.Generic;
using RevenueCat.SimpleJSON;

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
        CallPurchases("getProducts", JsonUtility.ToJson(request), type);
    }

    public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        if (oldSku == null)
        {
            CallPurchases("purchaseProduct", productIdentifier, type);
        }
        else
        {
            CallPurchases("purchaseProduct", productIdentifier, type, oldSku, (int) prorationMode);
        }
    }

    public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        if (oldSku == null)
        {
            CallPurchases("purchasePackage", packageToPurchase.Identifier, packageToPurchase.OfferingIdentifier);
        }
        else
        {
            CallPurchases("purchasePackage", packageToPurchase.Identifier, packageToPurchase.OfferingIdentifier, oldSku, (int) prorationMode);
        }
    }

    public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode, string userDefaultsSuiteName)
    {
        CallPurchases("setup", apiKey, appUserId, gameObject, observerMode, userDefaultsSuiteName);
    }

    public void RestoreTransactions()
    {
        CallPurchases("restoreTransactions");
    }

    public void AddAttributionData(int network, string data, string networkUserId)
    {
        CallPurchases("addAttributionData", data, network, networkUserId);
    }

    public void CreateAlias(string newAppUserId)
    {
        CallPurchases("createAlias", newAppUserId);
    }

    public void Identify(string appUserId)
    {
        CallPurchases("identify", appUserId);
    }

    public void Reset()
    {
        CallPurchases("reset");
    }

    public void SetFinishTransactions(bool finishTransactions)
    {
        CallPurchases("setFinishTransactions", finishTransactions);
    }

    public void SetAllowSharingStoreAccount(bool allow)
    {
        CallPurchases("setAllowSharingStoreAccount", allow);
    }

    public void SetDebugLogsEnabled(bool enabled)
    {
        CallPurchases("setDebugLogsEnabled", enabled);
    }
    
    public void SetProxyURL(string proxyURL)
    {
        CallPurchases("setProxyURL", proxyURL);
    }

    public string GetAppUserId()
    {
        return CallPurchases<string>("getAppUserID");
    }

    public void GetPurchaserInfo()
    {
        CallPurchases("getPurchaserInfo");
    }

    public void GetOfferings()
    {
        CallPurchases("getOfferings");
    }

    public void SyncPurchases()
    {
        CallPurchases("syncPurchases");
    }

    public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
    {
        // NOOP
    }

    public bool IsAnonymous()
    {
        return CallPurchases<bool>("isAnonymous");
    }

    public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers)
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };
        CallPurchases("checkTrialOrIntroductoryPriceEligibility", JsonUtility.ToJson(request));
    }

    public void InvalidatePurchaserInfoCache()
    {
        CallPurchases("invalidatePurchaserInfoCache");
    }

    public void SetAttributes(string attributesJson)
    {
        CallPurchases("setAttributes", attributesJson);
    }

    public void SetEmail(string email)
    {
        CallPurchases("setEmail", email);
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        CallPurchases("setPhoneNumber", phoneNumber);
    }

    public void SetDisplayName(string displayName)
    {
        CallPurchases("setDisplayName", displayName);
    }

    public void SetPushToken(string token)
    {
        CallPurchases("setPushToken", token);
    }

    private const string PurchasesWrapper = "com.revenuecat.purchasesunity.PurchasesWrapper";

    private static void CallPurchases(string methodName, params object[] args)
    {
        using (var purchases = new AndroidJavaClass(PurchasesWrapper))
        {
            purchases.CallStatic(methodName, args);
        }
    }
    
    private static ReturnType CallPurchases<ReturnType>(string methodName, params object[] args)
    {
        using (var purchases = new AndroidJavaClass(PurchasesWrapper))
        {
            return purchases.CallStatic<ReturnType>(methodName, args);
        }
    }

}
#endif
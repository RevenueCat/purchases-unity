using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using System.Collections.Generic;
using RevenueCat.MiniJSON;

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
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("getProducts", JsonUtility.ToJson(request), type);
        }
    }

    public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            if (oldSku == null)
            {
                purchases.CallStatic("purchaseProduct", productIdentifier, type);
            }
            else
            {
                purchases.CallStatic("purchaseProduct", productIdentifier, type, oldSku, (int) prorationMode);
            }
        }
    }

    public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            if (oldSku == null)
            {
                purchases.CallStatic("purchasePackage", packageToPurchase.Identifier, packageToPurchase.OfferingIdentifier);
            }
            else
            {
                purchases.CallStatic("purchasePackage", packageToPurchase.Identifier, packageToPurchase.OfferingIdentifier, oldSku, (int) prorationMode);
            }
        }
    }

    public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
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

    public void GetOfferings()
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("getOfferings");
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

    public bool IsAnonymous()
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            return purchases.CallStatic<bool>("isAnonymous");
        }
    }

    public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers)
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("checkTrialOrIntroductoryPriceEligibility", JsonUtility.ToJson(request));
        }
    }

    public void InvalidatePurchaserInfoCache()
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("invalidatePurchaserInfoCache");
        }
    }

    public void SetAttributes(Dictionary<string, string> attributes)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("setAttributes", Json.Serialize(attributes));
        }
    }

    public void SetEmail(string email)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("setEmail", email);
        }
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("setPhoneNumber", phoneNumber);
        }
    }

    public void SetDisplayName(string displayName)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("setDisplayName", displayName);
        }
    }

    public void SetPushToken(string token)
    {
        using (var purchases = new AndroidJavaClass("com.revenuecat.purchasesunity.PurchasesWrapper"))
        {
            purchases.CallStatic("setPushToken", token);
        }
    }

}
#endif
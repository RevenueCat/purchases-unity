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
        CallPurchases("getProducts", JsonUtility.ToJson(request), type);
    }

    public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        Purchases.PromotionalOffer discount = null)
    {
        if (oldSku == null)
        {
            CallPurchases("purchaseProduct", productIdentifier, type);
        }
        else
        {
            CallPurchases("purchaseProduct", productIdentifier, type, oldSku, (int)prorationMode);
        }
    }

    public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        Purchases.PromotionalOffer discount = null)
    {
        if (oldSku == null)
        {
            CallPurchases("purchasePackage", packageToPurchase.Identifier, packageToPurchase.OfferingIdentifier);
        }
        else
        {
            CallPurchases("purchasePackage", packageToPurchase.Identifier, packageToPurchase.OfferingIdentifier, oldSku,
                (int)prorationMode);
        }
    }

    public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode, bool usesStoreKit2IfAvailable,
        string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson)
    {
        CallPurchases("setup", apiKey, appUserId, gameObject, observerMode, userDefaultsSuiteName, useAmazon,
            dangerousSettingsJson);
    }

    public void RestorePurchases()
    {
        CallPurchases("restorePurchases");
    }

    public void LogIn(string appUserId)
    {
        CallPurchases("logIn", appUserId);
    }

    public void LogOut()
    {
        CallPurchases("logOut");
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

    public void GetCustomerInfo()
    {
        CallPurchases("getCustomerInfo");
    }

    public void GetOfferings()
    {
        CallPurchases("getOfferings");
    }

    public void SyncPurchases()
    {
        CallPurchases("syncPurchases");
    }

    public void SyncObserverModeAmazonPurchase(string productID, string receiptID, string amazonUserID,
        string isoCurrencyCode, double price)
    {
        CallPurchases("syncObserverModeAmazonPurchase", productID, receiptID, amazonUserID, isoCurrencyCode, price);
    }

    public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
    {
        // NOOP
    }

    public void EnableAdServicesAttributionTokenCollection()
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

    public void InvalidateCustomerInfoCache()
    {
        CallPurchases("invalidateCustomerInfoCache");
    }

    public void PresentCodeRedemptionSheet()
    {
        // NOOP
    }

    public void SetSimulatesAskToBuyInSandbox(bool enabled)
    {
        // NOOP
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

    public void SetAdjustID(string adjustID)
    {
        CallPurchases("setAdjustID", adjustID);
    }

    public void SetAppsflyerID(string appsflyerID)
    {
        CallPurchases("setAppsflyerID", appsflyerID);
    }

    public void SetFBAnonymousID(string fbAnonymousID)
    {
        CallPurchases("setFBAnonymousID", fbAnonymousID);
    }

    public void SetMparticleID(string mparticleID)
    {
        CallPurchases("setMparticleID", mparticleID);
    }

    public void SetOnesignalID(string onesignalID)
    {
        CallPurchases("setOnesignalID", onesignalID);
    }

    public void SetAirshipChannelID(string airshipChannelID)
    {
        CallPurchases("setAirshipChannelID", airshipChannelID);
    }

    public void SetMediaSource(string mediaSource)
    {
        CallPurchases("setMediaSource", mediaSource);
    }

    public void SetCampaign(string campaign)
    {
        CallPurchases("setCampaign", campaign);
    }

    public void SetAdGroup(string adGroup)
    {
        CallPurchases("setAdGroup", adGroup);
    }

    public void SetAd(string ad)
    {
        CallPurchases("setAd", ad);
    }

    public void SetKeyword(string keyword)
    {
        CallPurchases("setKeyword", keyword);
    }

    public void SetCreative(string creative)
    {
        CallPurchases("setCreative", creative);
    }

    public void CollectDeviceIdentifiers()
    {
        CallPurchases("collectDeviceIdentifiers");
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class CanMakePaymentsRequest
    {
        public int[] features;
    }

    public void CanMakePayments(Purchases.BillingFeature[] features)
    {
        int[] featuresAsInts = new int[features.Length];
        for (int i = 0; i < features.Length; i++)
        {
            Purchases.BillingFeature feature = features[i];
            featuresAsInts[i] = (int)feature;
        }

        var request = new CanMakePaymentsRequest
        {
            features = featuresAsInts
        };
        CallPurchases("canMakePayments", JsonUtility.ToJson(request));
    }

    public void GetPromotionalOffer(string productIdentifier, string discountIdentifier)
    {
        CallPurchases("getPromotionalOffer", productIdentifier, discountIdentifier);
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
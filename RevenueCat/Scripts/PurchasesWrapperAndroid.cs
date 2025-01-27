using System.Diagnostics.CodeAnalysis;
using RevenueCat.SimpleJSON;
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
        bool googleIsPersonalizedPrice = false, string presentedOfferingIdentifier = null,
        Purchases.PromotionalOffer discount = null)
    {
        string presentedOfferingContextJSON = null;
        if (presentedOfferingIdentifier != null) {
            var contextDict = new JSONObject();
            contextDict["offeringIdentifier"] = presentedOfferingIdentifier;
            presentedOfferingContextJSON = contextDict.ToString();
        }

        if (oldSku == null)
        {
            CallPurchases("purchaseProduct", productIdentifier, type);
        }
        else
        {
            CallPurchases("purchaseProduct", productIdentifier, type, oldSku, (int)prorationMode, googleIsPersonalizedPrice, presentedOfferingContextJSON);
        }
    }

    public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        bool googleIsPersonalizedPrice = false, Purchases.PromotionalOffer discount = null)
    {
        string presentedOfferingContextJSON = packageToPurchase.PresentedOfferingContext.ToJsonString();

        if (oldSku == null)
        {
            CallPurchases("purchasePackage", packageToPurchase.Identifier, presentedOfferingContextJSON);
        }
        else
        {
            CallPurchases("purchasePackage", packageToPurchase.Identifier, presentedOfferingContextJSON, oldSku,
                (int)prorationMode, googleIsPersonalizedPrice);
        }
    }


    public void PurchaseSubscriptionOption(Purchases.SubscriptionOption subscriptionOption,
        Purchases.GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false)
    {
        string presentedOfferingContextJSON = null;
        if (subscriptionOption.PresentedOfferingContext != null) {
            presentedOfferingContextJSON = subscriptionOption.PresentedOfferingContext.ToJsonString();
        }
        
        if (googleProductChangeInfo == null)
        {
            CallPurchases("purchaseSubscriptionOption", subscriptionOption.ProductId, subscriptionOption.Id,
                null, 0, googleIsPersonalizedPrice, presentedOfferingContextJSON);
        }
        else
        {
            CallPurchases("purchaseSubscriptionOption", subscriptionOption.ProductId, subscriptionOption.Id,
                googleProductChangeInfo.OldProductIdentifier, (int)googleProductChangeInfo.ProrationMode, googleIsPersonalizedPrice,
                presentedOfferingContextJSON);
        }
    }

    public void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy, Purchases.StoreKitVersion storeKitVersion,
        string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson, bool shouldShowInAppMessagesAutomatically,
        bool pendingTransactionsForPrepaidPlansEnabled)
    {
        Setup(gameObject, apiKey, appUserId, purchasesAreCompletedBy, storeKitVersion, userDefaultsSuiteName, useAmazon,
            dangerousSettingsJson, shouldShowInAppMessagesAutomatically, Purchases.EntitlementVerificationMode.Disabled, pendingTransactionsForPrepaidPlansEnabled);
    }

    public void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy, Purchases.StoreKitVersion storeKitVersion,
        string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson, bool shouldShowInAppMessagesAutomatically,
        Purchases.EntitlementVerificationMode entitlementVerificationMode, bool pendingTransactionsForPrepaidPlansEnabled)
    {
        CallPurchases("setup", apiKey, appUserId, gameObject, purchasesAreCompletedBy.Name(), userDefaultsSuiteName, useAmazon, shouldShowInAppMessagesAutomatically,
            dangerousSettingsJson, entitlementVerificationMode.Name(), pendingTransactionsForPrepaidPlansEnabled);
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

    public void SetAllowSharingStoreAccount(bool allow)
    {
        CallPurchases("setAllowSharingStoreAccount", allow);
    }

    public void SetDebugLogsEnabled(bool enabled)
    {
        CallPurchases("setDebugLogsEnabled", enabled);
    }

    public void SetLogLevel(Purchases.LogLevel level)
    {
        CallPurchases("setLogLevel", level.Name());
    }

    public void SetLogHandler()
    {
        CallPurchases("setLogHandler");
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

    public void GetCurrentOfferingForPlacement(string placementIdentifier)
    {
        CallPurchases("getCurrentOfferingForPlacement", placementIdentifier);
    }

    public void SyncAttributesAndOfferingsIfNeeded()
    {
        CallPurchases("syncAttributesAndOfferingsIfNeeded");
    }

    public void SyncPurchases()
    {
        CallPurchases("syncPurchases");
    }

    public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID,
        string isoCurrencyCode, double price)
    {
        CallPurchases("syncAmazonPurchase", productID, receiptID, amazonUserID, isoCurrencyCode, price);
    }

    public void GetAmazonLWAConsentStatus()
    {
        CallPurchases("getAmazonLWAConsentStatus");
    }

    public void EnableAdServicesAttributionTokenCollection()
    {
        // NOOP
    }

    public bool IsAnonymous()
    {
        return CallPurchases<bool>("isAnonymous");
    }

    public bool IsConfigured()
    {
        return CallPurchases<bool>("isConfigured");
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

    public void RecordPurchase(string productID)
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

    public void SetCleverTapID(string cleverTapID)
    {
        CallPurchases("setCleverTapID", cleverTapID);
    }

    public void SetMixpanelDistinctID(string mixpanelDistinctID)
    {
        CallPurchases("setMixpanelDistinctID", mixpanelDistinctID);
    }

    public void SetFirebaseAppInstanceID(string firebaseAppInstanceID)
    {
        CallPurchases("setFirebaseAppInstanceID", firebaseAppInstanceID);
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

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class ShowInAppMessagesRequest
    {
        public int[] messageTypes;
    }

    public void ShowInAppMessages(Purchases.InAppMessageType[] messageTypes)
    {
        int[] messageTypesAsInts = new int[messageTypes.Length];
        for (int i = 0; i < messageTypes.Length; i++) {
            Purchases.InAppMessageType messageType = messageTypes[i];
            messageTypesAsInts[i] = (int)messageType;
        }

        var request = new ShowInAppMessagesRequest
        {
            messageTypes = messageTypesAsInts
        };
        CallPurchases("showInAppMessages", JsonUtility.ToJson(request));
    }

    public void ParseAsWebPurchaseRedemption(string urlString)
    {
        CallPurchases("parseAsWebPurchaseRedemption", urlString);
    }

    public void RedeemWebPurchase(Purchases.WebPurchaseRedemption webPurchaseRedemption)
    {
        CallPurchases("redeemWebPurchase", webPurchaseRedemption.RedemptionLink);
    }

    public void GetEligibleWinBackOffersForProduct(Purchases.StoreProduct storeProduct)
    {
        CallPurchases("getEligibleWinBackOffersForProduct", storeProduct.Identifier);
    }

    public void GetEligibleWinBackOffersForPackage(Purchases.Package package)
    {
        CallPurchases("getEligibleWinBackOffersForPackage", package.StoreProduct.Identifier);
    }

    public void PurchaseProductWithWinBackOffer(Purchases.StoreProduct storeProduct, Purchases.WinBackOffer winBackOffer)
    {
        CallPurchases("purchaseProductWithWinBackOffer", storeProduct.Identifier, winBackOffer.Identifier);
    }

    public void PurchasePackageWithWinBackOffer(Purchases.Package package, Purchases.WinBackOffer winBackOffer)
    {
        CallPurchases("purchasePackageWithWinBackOffer", package.Identifier, package.PresentedOfferingContext.ToJsonString(), winBackOffer.Identifier);
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

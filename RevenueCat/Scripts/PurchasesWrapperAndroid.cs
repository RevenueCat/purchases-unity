using System.Diagnostics.CodeAnalysis;
using RevenueCat;
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

    public void GetStorefront(string requestId = null)
    {
        CallPurchases("getStorefront", requestId);
    }

    public void GetProducts(string[] productIdentifiers, string type = "subs", string requestId = null)
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };
        CallPurchases("getProducts", JsonUtility.ToJson(request), type, requestId);
    }

    public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        bool googleIsPersonalizedPrice = false, string presentedOfferingIdentifier = null,
        Purchases.PromotionalOffer discount = null, string requestId = null)
    {
        string presentedOfferingContextJSON = null;
        if (presentedOfferingIdentifier != null) {
            var contextDict = new JSONObject();
            contextDict["offeringIdentifier"] = presentedOfferingIdentifier;
            presentedOfferingContextJSON = contextDict.ToString();
        }

        if (oldSku == null)
        {
            CallPurchases("purchaseProduct", productIdentifier, type, requestId);
        }
        else
        {
            CallPurchases("purchaseProduct", productIdentifier, type, oldSku, (int)prorationMode,
                googleIsPersonalizedPrice, presentedOfferingContextJSON, requestId);
        }
    }

    public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        bool googleIsPersonalizedPrice = false, Purchases.PromotionalOffer discount = null,
        string requestId = null)
    {
        string presentedOfferingContextJSON = packageToPurchase.PresentedOfferingContext.ToJsonString();

        if (oldSku == null)
        {
            CallPurchases("purchasePackage", packageToPurchase.Identifier, presentedOfferingContextJSON, requestId);
        }
        else
        {
            CallPurchases("purchasePackage", packageToPurchase.Identifier, presentedOfferingContextJSON, oldSku,
                (int)prorationMode, googleIsPersonalizedPrice, requestId);
        }
    }


    public void PurchaseSubscriptionOption(Purchases.SubscriptionOption subscriptionOption,
        Purchases.GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false,
        string requestId = null)
    {
        string presentedOfferingContextJSON = null;
        if (subscriptionOption.PresentedOfferingContext != null) {
            presentedOfferingContextJSON = subscriptionOption.PresentedOfferingContext.ToJsonString();
        }
        
        if (googleProductChangeInfo == null)
        {
            CallPurchases("purchaseSubscriptionOption", subscriptionOption.ProductId, subscriptionOption.Id,
                null, 0, googleIsPersonalizedPrice, presentedOfferingContextJSON, requestId);
        }
        else
        {
            CallPurchases("purchaseSubscriptionOption", subscriptionOption.ProductId, subscriptionOption.Id,
                googleProductChangeInfo.OldProductIdentifier, (int)googleProductChangeInfo.ProrationMode, googleIsPersonalizedPrice,
                presentedOfferingContextJSON, requestId);
        }
    }

    public void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy, Purchases.StoreKitVersion storeKitVersion,
        string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson, bool shouldShowInAppMessagesAutomatically,
        bool pendingTransactionsForPrepaidPlansEnabled, bool diagnosticsEnabled, bool automaticDeviceIdentifierCollectionEnabled,
        string preferredUILocaleOverride)
    {
        Setup(gameObject, apiKey, appUserId, purchasesAreCompletedBy, storeKitVersion, userDefaultsSuiteName, useAmazon,
            dangerousSettingsJson, shouldShowInAppMessagesAutomatically, Purchases.EntitlementVerificationMode.Disabled, pendingTransactionsForPrepaidPlansEnabled,
            diagnosticsEnabled, automaticDeviceIdentifierCollectionEnabled, preferredUILocaleOverride);
    }

    public void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy, Purchases.StoreKitVersion storeKitVersion,
        string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson, bool shouldShowInAppMessagesAutomatically,
        Purchases.EntitlementVerificationMode entitlementVerificationMode, bool pendingTransactionsForPrepaidPlansEnabled,
        bool diagnosticsEnabled, bool automaticDeviceIdentifierCollectionEnabled, string preferredUILocaleOverride)
    {
        CallPurchases("setup", apiKey, appUserId, gameObject, purchasesAreCompletedBy.Name(), userDefaultsSuiteName, useAmazon, shouldShowInAppMessagesAutomatically,
            dangerousSettingsJson, entitlementVerificationMode.Name(), pendingTransactionsForPrepaidPlansEnabled, diagnosticsEnabled,
            automaticDeviceIdentifierCollectionEnabled, preferredUILocaleOverride);
    }

    public void RestorePurchases(string requestId = null)
    {
        CallPurchases("restorePurchases", requestId);
    }

    public void LogIn(string appUserId, string requestId = null)
    {
        CallPurchases("logIn", appUserId, requestId);
    }

    public void LogOut(string requestId = null)
    {
        CallPurchases("logOut", requestId);
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

    public void GetCustomerInfo(string requestId = null)
    {
        CallPurchases("getCustomerInfo", requestId);
    }

    public void GetOfferings(string requestId = null)
    {
        CallPurchases("getOfferings", requestId);
    }

    public void GetCurrentOfferingForPlacement(string placementIdentifier, string requestId = null)
    {
        CallPurchases("getCurrentOfferingForPlacement", placementIdentifier, requestId);
    }

    public void SyncAttributesAndOfferingsIfNeeded(string requestId = null)
    {
        CallPurchases("syncAttributesAndOfferingsIfNeeded", requestId);
    }

    public void SyncPurchases(string requestId = null)
    {
        CallPurchases("syncPurchases", requestId);
    }

    public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID,
        string isoCurrencyCode, double price)
    {
        CallPurchases("syncAmazonPurchase", productID, receiptID, amazonUserID, isoCurrencyCode, price);
    }

    public void GetAmazonLWAConsentStatus(string requestId = null)
    {
        CallPurchases("getAmazonLWAConsentStatus", requestId);
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

    public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers, string requestId = null)
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };
        CallPurchases("checkTrialOrIntroductoryPriceEligibility", JsonUtility.ToJson(request), requestId);
    }

    public void InvalidateCustomerInfoCache()
    {
        CallPurchases("invalidateCustomerInfoCache");
    }

    public void OverridePreferredUILocale(string locale)
    {
        CallPurchases("overridePreferredUILocale", locale);
    }

    public void PresentCodeRedemptionSheet()
    {
        // NOOP
    }

    public void RecordPurchase(string productID, string requestId = null)
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

    public void SetOnesignalUserID(string onesignalUserID)
    {
        CallPurchases("setOnesignalUserID", onesignalUserID);
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

    public void SetAppsFlyerConversionData(string conversionDataJson)
    {
        CallPurchases("setAppsFlyerConversionData", conversionDataJson);
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

    public void CanMakePayments(Purchases.BillingFeature[] features, string requestId = null)
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
        CallPurchases("canMakePayments", JsonUtility.ToJson(request), requestId);
    }

    public void GetPromotionalOffer(string productIdentifier, string discountIdentifier, string requestId = null)
    {
        CallPurchases("getPromotionalOffer", productIdentifier, discountIdentifier, requestId);
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

    public void ParseAsWebPurchaseRedemption(string urlString, string requestId = null)
    {
        CallPurchases("parseAsWebPurchaseRedemption", urlString, requestId);
    }

    public void RedeemWebPurchase(Purchases.WebPurchaseRedemption webPurchaseRedemption, string requestId = null)
    {
        CallPurchases("redeemWebPurchase", webPurchaseRedemption.RedemptionLink, requestId);
    }

    public void GetVirtualCurrencies(string requestId = null)
    {
        CallPurchases("getVirtualCurrencies", requestId);
    }

    public string GetCachedVirtualCurrencies()
    {
        return CallPurchases<string>("getCachedVirtualCurrencies");
    }

    public void InvalidateVirtualCurrenciesCache()
    {
        CallPurchases("invalidateVirtualCurrenciesCache");
    }

    public void GetEligibleWinBackOffersForProduct(Purchases.StoreProduct storeProduct, string requestId = null)
    {
        CallPurchases("getEligibleWinBackOffersForProduct", storeProduct.Identifier, requestId);
    }

    public void GetEligibleWinBackOffersForPackage(Purchases.Package package, string requestId = null)
    {
        CallPurchases("getEligibleWinBackOffersForPackage", package.StoreProduct.Identifier, requestId);
    }

    public void PurchaseProductWithWinBackOffer(Purchases.StoreProduct storeProduct,
        Purchases.WinBackOffer winBackOffer, string requestId = null)
    {
        CallPurchases("purchaseProductWithWinBackOffer", storeProduct.Identifier, winBackOffer.Identifier, requestId);
    }

    public void PurchasePackageWithWinBackOffer(Purchases.Package package, Purchases.WinBackOffer winBackOffer,
        string requestId = null)
    {
        CallPurchases("purchasePackageWithWinBackOffer", package.Identifier,
            package.PresentedOfferingContext.ToJsonString(), winBackOffer.Identifier, requestId);
    }

    public void TrackCustomPaywallImpression(Purchases.CustomPaywallImpressionParams parameters)
    {
        var offeringId = parameters.OfferingId;
        CallPurchases("trackCustomPaywallImpression", parameters.PaywallId, offeringId,
            parameters.PresentedOfferingContext?.ToJsonString());
    }

    public void TrackAdDisplayed(AdDisplayedData data) =>
        CallPurchases("trackAdDisplayed", data.ToJsonString());

    public void TrackAdOpened(AdOpenedData data) =>
        CallPurchases("trackAdOpened", data.ToJsonString());

    public void TrackAdRevenue(AdRevenueData data) =>
        CallPurchases("trackAdRevenue", data.ToJsonString());

    public void TrackAdLoaded(AdLoadedData data) =>
        CallPurchases("trackAdLoaded", data.ToJsonString());

    public void TrackAdFailedToLoad(AdFailedToLoadData data) =>
        CallPurchases("trackAdFailedToLoad", data.ToJsonString());

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

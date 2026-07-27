using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using RevenueCat;
using UnityEngine;

#if UNITY_IOS || UNITY_VISIONOS
public class PurchasesWrapperiOS : IPurchasesWrapper
{
    [DllImport("__Internal")]
    private static extern void _RCSetupPurchases(string gameObject, string apiKey, string appUserId, string purchasesAreCompletedBy,
                                                 string storeKitVersion, string userDefaultsSuiteName,
                                                 string dangerousSettingsJson, bool shouldShowInAppMessagesAutomatically,
                                                 string entitlementVerificationMode, bool diagnosticsEnabled,
                                                 bool automaticDeviceIdentifierCollectionEnabled, string preferredUILocaleOverride);
    public void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy,
        Purchases.StoreKitVersion storeKitVersion, string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
        bool shouldShowInAppMessagesAutomatically, bool pendingTransactionsForPrepaidPlansEnabled, bool diagnosticsEnabled,
        bool automaticDeviceIdentifierCollectionEnabled, string preferredUILocaleOverride)
    {
        Setup(gameObject, apiKey, appUserId, purchasesAreCompletedBy, storeKitVersion,
            userDefaultsSuiteName, useAmazon, dangerousSettingsJson, shouldShowInAppMessagesAutomatically,
            Purchases.EntitlementVerificationMode.Disabled, pendingTransactionsForPrepaidPlansEnabled, diagnosticsEnabled,
            automaticDeviceIdentifierCollectionEnabled, preferredUILocaleOverride);
    }

    public void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy,
        Purchases.StoreKitVersion storeKitVersion, string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
        bool shouldShowInAppMessagesAutomatically, Purchases.EntitlementVerificationMode entitlementVerificationMode,
        bool pendingTransactionsForPrepaidPlansEnabled, bool diagnosticsEnabled, bool automaticDeviceIdentifierCollectionEnabled,
        string preferredUILocaleOverride)
    {
        _RCSetupPurchases(gameObject, apiKey, appUserId, purchasesAreCompletedBy.Name(), storeKitVersion.Name(),
            userDefaultsSuiteName, dangerousSettingsJson, shouldShowInAppMessagesAutomatically, entitlementVerificationMode.Name(),
            diagnosticsEnabled, automaticDeviceIdentifierCollectionEnabled, preferredUILocaleOverride);
    }

    [DllImport("__Internal")]
    private static extern void _RCGetStorefront(string requestId);
    public void GetStorefront(string requestId = null)
    {
        _RCGetStorefront(requestId);
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class ProductsRequest
    {
        public string[] productIdentifiers;
    }

    [DllImport("__Internal")]
    private static extern void _RCGetProducts(string productIdentifiersJson, string type, string requestId);
    public void GetProducts(string[] productIdentifiers, string type = "subs", string requestId = null)
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };

        _RCGetProducts(JsonUtility.ToJson(request), type, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCPurchaseProduct(string productIdentifier, string signedDiscountTimestamp,
        string requestId);
    public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        bool googleIsPersonalizedPrice = false, string presentedOfferingIdentifier = null,
        Purchases.PromotionalOffer discount = null, string requestId = null)
    {
        string discountTimestamp = null;
        if (discount != null)
        {
            discountTimestamp = discount.Timestamp.ToString();
        }
        _RCPurchaseProduct(productIdentifier, discountTimestamp, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCPurchasePackage(string packageIdentifier, string presentedOfferingContextJSON,
        string signedDiscountTimestamp, string requestId);
    public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        bool googleIsPersonalizedPrice = false, Purchases.PromotionalOffer discount = null,
        string requestId = null)
    {
        string discountTimestamp = null;
        if (discount != null)
        {
            discountTimestamp = discount.Timestamp.ToString();
        }

        _RCPurchasePackage(packageToPurchase.Identifier, packageToPurchase.PresentedOfferingContext.ToJsonString(),
            discountTimestamp, requestId);
    }

    public void PurchaseSubscriptionOption(Purchases.SubscriptionOption subscriptionOption,
        Purchases.GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false,
        string requestId = null)
    {
        // No-Op
    }

    [DllImport("__Internal")]
    private static extern void _RCRestorePurchases(string requestId);
    public void RestorePurchases(string requestId = null)
    {
        _RCRestorePurchases(requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCSyncPurchases(string requestId);
    public void SyncPurchases(string requestId = null)
    {
        _RCSyncPurchases(requestId);
    }

    public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID,
        string isoCurrencyCode, double price)
    {
        // No-Op
    }

    public void GetAmazonLWAConsentStatus(string requestId = null)
    {
        // No-Op
    }

    [DllImport("__Internal")]
    private static extern void _RCLogIn(string appUserId, string requestId);
    public void LogIn(string appUserId, string requestId = null)
    {
        _RCLogIn(appUserId, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCLogOut(string requestId);
    public void LogOut(string requestId = null)
    {
        _RCLogOut(requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAllowSharingStoreAccount(bool allow);
    public void SetAllowSharingStoreAccount(bool allow)
    {
        _RCSetAllowSharingStoreAccount(allow);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetDebugLogsEnabled(bool enabled);
    public void SetDebugLogsEnabled(bool enabled)
    {
        _RCSetDebugLogsEnabled(enabled);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetLogLevel(string level);
    public void SetLogLevel(Purchases.LogLevel level)
    {
        _RCSetLogLevel(level.Name());
    }

    [DllImport("__Internal")]
    private static extern void _RCSetLogHandler();
    public void SetLogHandler()
    {
        _RCSetLogHandler();
    }

    [DllImport("__Internal")]
    private static extern void _RCSetProxyURLString(string proxyURL);
    public void SetProxyURL(string proxyURL)
    {
        _RCSetProxyURLString(proxyURL);
    }

    [DllImport("__Internal")]
    private static extern string _RCGetAppUserID();
    public string GetAppUserId()
    {
        return _RCGetAppUserID();
    }

    [DllImport("__Internal")]
    private static extern void _RCGetCustomerInfo(string requestId);
    public void GetCustomerInfo(string requestId = null)
    {
        _RCGetCustomerInfo(requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCGetOfferings(string requestId);
    public void GetOfferings(string requestId = null)
    {
        _RCGetOfferings(requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCGetCurrentOfferingForPlacement(string placementIdentifier, string requestId);
    public void GetCurrentOfferingForPlacement(string placementIdentifier, string requestId = null)
    {
        _RCGetCurrentOfferingForPlacement(placementIdentifier, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCSyncAttributesAndOfferingsIfNeeded(string requestId);
    public void SyncAttributesAndOfferingsIfNeeded(string requestId = null)
    {
        _RCSyncAttributesAndOfferingsIfNeeded(requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCEnableAdServicesAttributionTokenCollection();
    public void EnableAdServicesAttributionTokenCollection()
    {
        _RCEnableAdServicesAttributionTokenCollection();
    }

    [DllImport("__Internal")]
    private static extern bool _RCIsAnonymous();
    public bool IsAnonymous()
    {
        return _RCIsAnonymous();
    }

    [DllImport("__Internal")]
    private static extern bool _RCIsConfigured();
    public bool IsConfigured()
    {
        return _RCIsConfigured();
    }

    [DllImport("__Internal")]
    private static extern void _RCCheckTrialOrIntroductoryPriceEligibility(string productIdentifiersJson,
        string requestId);
    public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers, string requestId = null)
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };

        _RCCheckTrialOrIntroductoryPriceEligibility(JsonUtility.ToJson(request), requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCInvalidateCustomerInfoCache();
    public void InvalidateCustomerInfoCache()
    {
        _RCInvalidateCustomerInfoCache();
    }

    [DllImport("__Internal")]
    private static extern void _RCOverridePreferredUILocale(string locale);
    public void OverridePreferredUILocale(string locale)
    {
        _RCOverridePreferredUILocale(locale);
    }

    [DllImport("__Internal")]
    private static extern void _RCPresentCodeRedemptionSheet();
    public void PresentCodeRedemptionSheet()
    {
        _RCPresentCodeRedemptionSheet();
    }

    [DllImport("__Internal")]
    private static extern void _RCRecordPurchase(string productID, string requestId);
    public void RecordPurchase(string productID, string requestId = null)
    {
        _RCRecordPurchase(productID, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetSimulatesAskToBuyInSandbox(bool enabled);
    public void SetSimulatesAskToBuyInSandbox(bool enabled)
    {
        _RCSetSimulatesAskToBuyInSandbox(enabled);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAttributes(string attributesJson);
    public void SetAttributes(string attributesJson)
    {
        _RCSetAttributes(attributesJson);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetEmail(string email);
    public void SetEmail(string email)
    {
        _RCSetEmail(email);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetPhoneNumber(string phoneNumber);
    public void SetPhoneNumber(string phoneNumber)
    {
        _RCSetPhoneNumber(phoneNumber);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetDisplayName(string displayName);
    public void SetDisplayName(string displayName)
    {
        _RCSetDisplayName(displayName);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetPushToken(string token);
    public void SetPushToken(string token)
    {
        _RCSetPushToken(token);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAdjustID(string adjustID);
    public void SetAdjustID(string adjustID)
    {
        _RCSetAdjustID(adjustID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAppsflyerID(string appsflyerID);
    public void SetAppsflyerID(string appsflyerID)
    {
        _RCSetAppsflyerID(appsflyerID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetFBAnonymousID(string fbAnonymousID);
    public void SetFBAnonymousID(string fbAnonymousID)
    {
        _RCSetFBAnonymousID(fbAnonymousID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetMparticleID(string mparticleID);
    public void SetMparticleID(string mparticleID)
    {
        _RCSetMparticleID(mparticleID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetOnesignalID(string onesignalID);
    public void SetOnesignalID(string onesignalID)
    {
        _RCSetOnesignalID(onesignalID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetOnesignalUserID(string onesignalUserID);
    public void SetOnesignalUserID(string onesignalUserID)
    {
        _RCSetOnesignalUserID(onesignalUserID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAirshipChannelID(string airshipChannelID);
    public void SetAirshipChannelID(string airshipChannelID)
    {
        _RCSetAirshipChannelID(airshipChannelID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetCleverTapID(string cleverTapID);
    public void SetCleverTapID(string cleverTapID)
    {
        _RCSetCleverTapID(cleverTapID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetMixpanelDistinctID(string mixpanelDistinctID);
    public void SetMixpanelDistinctID(string mixpanelDistinctID)
    {
        _RCSetMixpanelDistinctID(mixpanelDistinctID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetFirebaseAppInstanceID(string firebaseAppInstanceID);
    public void SetFirebaseAppInstanceID(string firebaseAppInstanceID)
    {
        _RCSetFirebaseAppInstanceID(firebaseAppInstanceID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetMediaSource(string mediaSource);
    public void SetMediaSource(string mediaSource)
    {
        _RCSetMediaSource(mediaSource);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetCampaign(string campaign);
    public void SetCampaign(string campaign)
    {
        _RCSetCampaign(campaign);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAdGroup(string adGroup);
    public void SetAdGroup(string adGroup)
    {
        _RCSetAdGroup(adGroup);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAd(string ad);
    public void SetAd(string ad)
    {
        _RCSetAd(ad);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetKeyword(string keyword);
    public void SetKeyword(string keyword)
    {
        _RCSetKeyword(keyword);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetCreative(string creative);
    public void SetCreative(string creative)
    {
        _RCSetCreative(creative);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAppsFlyerConversionData(string conversionDataJson);
    public void SetAppsFlyerConversionData(string conversionDataJson)
    {
        _RCSetAppsFlyerConversionData(conversionDataJson);
    }

    [DllImport("__Internal")]
    private static extern void _RCCollectDeviceIdentifiers();
    public void CollectDeviceIdentifiers()
    {
        _RCCollectDeviceIdentifiers();
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class CanMakePaymentsRequest
    {
        public int[] features;
    }

    [DllImport("__Internal")]
    private static extern void _RCCanMakePayments(string featuresJson, string requestId);
    public void CanMakePayments(Purchases.BillingFeature[] features, string requestId = null)
    {
        int[] featuresAsInts = new int[features.Length];
        for (int i = 0; i < features.Length; i++) {
            Purchases.BillingFeature feature = features[i];
            featuresAsInts[i] = (int)feature;
        }

        var request = new CanMakePaymentsRequest
        {
            features = featuresAsInts
        };

        _RCCanMakePayments(JsonUtility.ToJson(request), requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCGetPromotionalOffer(string productIdentifier, string discountIdentifier,
        string requestId);
    public void GetPromotionalOffer(string productIdentifier, string discountIdentifier, string requestId = null)
    {
        _RCGetPromotionalOffer(productIdentifier, discountIdentifier, requestId);
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class ShowInAppMessagesRequest
    {
        public int[] messageTypes;
    }

    [DllImport("__Internal")]
    private static extern void _RCShowInAppMessages(string messagesJson);
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

        _RCShowInAppMessages(JsonUtility.ToJson(request));
    }

    [DllImport("__Internal")]
    private static extern void _RCParseAsWebPurchaseRedemption(string urlString, string requestId);
    public void ParseAsWebPurchaseRedemption(string urlString, string requestId = null)
    {
        _RCParseAsWebPurchaseRedemption(urlString, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCRedeemWebPurchase(string resultJson, string requestId);
    public void RedeemWebPurchase(Purchases.WebPurchaseRedemption webPurchaseRedemption, string requestId = null)
    {
        _RCRedeemWebPurchase(webPurchaseRedemption.RedemptionLink, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCGetVirtualCurrencies(string requestId);
    public void GetVirtualCurrencies(string requestId = null)
    {
        _RCGetVirtualCurrencies(requestId);
    }

    [DllImport("__Internal")]
    private static extern string _RCGetCachedVirtualCurrencies();
    public string GetCachedVirtualCurrencies()
    {
        return _RCGetCachedVirtualCurrencies();
    }

    [DllImport("__Internal")]
    private static extern void _RCInvalidateVirtualCurrenciesCache();
    public void InvalidateVirtualCurrenciesCache()
    {
        _RCInvalidateVirtualCurrenciesCache();
    }

    [DllImport("__Internal")]
    private static extern void _RCGetEligibleWinBackOffersForProduct(string productIdentifier, string requestId);
    public void GetEligibleWinBackOffersForProduct(Purchases.StoreProduct storeProduct, string requestId = null)
    {
        _RCGetEligibleWinBackOffersForProduct(storeProduct.Identifier, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCGetEligibleWinBackOffersForPackage(string productIdentifier, string requestId);
    public void GetEligibleWinBackOffersForPackage(Purchases.Package package, string requestId = null)
    {
        _RCGetEligibleWinBackOffersForPackage(package.StoreProduct.Identifier, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCPurchaseProductWithWinBackOffer(string productIdentifier,
        string winBackOfferIdentifier, string requestId);
    public void PurchaseProductWithWinBackOffer(Purchases.StoreProduct storeProduct,
        Purchases.WinBackOffer winBackOffer, string requestId = null)
    {
        _RCPurchaseProductWithWinBackOffer(storeProduct.Identifier, winBackOffer.Identifier, requestId);
    }

    [DllImport("__Internal")]
    private static extern void _RCPurchasePackageWithWinBackOffer(string packageIdentifier,
        string presentedOfferingContextJson, string winBackOfferIdentifier, string requestId);
    public void PurchasePackageWithWinBackOffer(Purchases.Package package, Purchases.WinBackOffer winBackOffer,
        string requestId = null)
    {
        _RCPurchasePackageWithWinBackOffer(package.Identifier, package.PresentedOfferingContext.ToJsonString(),
            winBackOffer.Identifier, requestId);
    }
    [DllImport("__Internal")]
    private static extern void _RCTrackCustomPaywallImpression(string paywallId, string offeringId, string presentedOfferingContextJSON);
    public void TrackCustomPaywallImpression(Purchases.CustomPaywallImpressionParams parameters)
    {
        var offeringId = parameters.OfferingId;
        _RCTrackCustomPaywallImpression(parameters.PaywallId, offeringId,
            parameters.PresentedOfferingContext?.ToJsonString());
    }

    [DllImport("__Internal")]
    private static extern void _RCTrackAdDisplayed(string dataJson);
    public void TrackAdDisplayed(AdDisplayedData data) =>
        _RCTrackAdDisplayed(data.ToJsonString());

    [DllImport("__Internal")]
    private static extern void _RCTrackAdOpened(string dataJson);
    public void TrackAdOpened(AdOpenedData data) =>
        _RCTrackAdOpened(data.ToJsonString());

    [DllImport("__Internal")]
    private static extern void _RCTrackAdRevenue(string dataJson);
    public void TrackAdRevenue(AdRevenueData data) =>
        _RCTrackAdRevenue(data.ToJsonString());

    [DllImport("__Internal")]
    private static extern void _RCTrackAdLoaded(string dataJson);
    public void TrackAdLoaded(AdLoadedData data) =>
        _RCTrackAdLoaded(data.ToJsonString());

    [DllImport("__Internal")]
    private static extern void _RCTrackAdFailedToLoad(string dataJson);
    public void TrackAdFailedToLoad(AdFailedToLoadData data) =>
        _RCTrackAdFailedToLoad(data.ToJsonString());
}
#endif

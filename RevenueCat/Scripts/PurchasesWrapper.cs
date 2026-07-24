using System;
using System.Collections.Generic;
using RevenueCat;

public interface IPurchasesWrapper
{
    void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy,
        Purchases.StoreKitVersion storeKitVersion, string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
        bool shouldShowInAppMessagesAutomatically, bool pendingTransactionsForPrepaidPlansEnabled, bool diagnosticsEnabled,
        bool automaticDeviceIdentifierCollectionEnabled, string preferredUILocaleOverride);

    void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy,
        Purchases.StoreKitVersion storeKitVersion, string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
        bool shouldShowInAppMessagesAutomatically, Purchases.EntitlementVerificationMode entitlementVerificationMode,
        bool pendingTransactionsForPrepaidPlansEnabled, bool diagnosticsEnabled, bool automaticDeviceIdentifierCollectionEnabled,
        string preferredUILocaleOverride);

    void GetStorefront(string requestId = null);
    void GetProducts(string[] productIdentifiers, string type = "subs", string requestId = null);

    void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        bool googleIsPersonalizedPrice = false, string presentedOfferingIdentifier = null,
        Purchases.PromotionalOffer discount = null, string requestId = null);

    void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        bool googleIsPersonalizedPrice = false, Purchases.PromotionalOffer discount = null, string requestId = null);

    void PurchaseSubscriptionOption(Purchases.SubscriptionOption subscriptionOption,
        Purchases.GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false,
        string requestId = null);

    void RestorePurchases(string requestId = null);
    void LogIn(string appUserId, string requestId = null);
    void LogOut(string requestId = null);
    void SetAllowSharingStoreAccount(bool allow);
    void SetDebugLogsEnabled(bool enabled);
    void SetLogLevel(Purchases.LogLevel level);
    void SetLogHandler();
    void SetProxyURL(string proxyURL);
    string GetAppUserId();
    void GetCustomerInfo(string requestId = null);
    void GetOfferings(string requestId = null);
    void GetCurrentOfferingForPlacement(string placementIdentifier, string requestId = null);
    void SyncAttributesAndOfferingsIfNeeded(string requestId = null);
    void SyncPurchases(string requestId = null);

    void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID, string isoCurrencyCode,
        double price);

    void GetAmazonLWAConsentStatus(string requestId = null);
    void EnableAdServicesAttributionTokenCollection();
    bool IsAnonymous();
    bool IsConfigured();
    void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers, string requestId = null);
    void InvalidateCustomerInfoCache();
    void OverridePreferredUILocale(string locale);
    void PresentCodeRedemptionSheet();
    void RecordPurchase(string productID, string requestId = null);
    void SetSimulatesAskToBuyInSandbox(bool enabled);
    void SetAttributes(string attributesJson);
    void SetEmail(string email);
    void SetPhoneNumber(string phoneNumber);
    void SetDisplayName(string displayName);
    void SetPushToken(string token);
    void SetAdjustID(string adjustID);
    void SetAppsflyerID(string appsflyerID);
    void SetFBAnonymousID(string fbAnonymousID);
    void SetMparticleID(string mparticleID);
    void SetOnesignalID(string onesignalID);
    void SetOnesignalUserID(string onesignalUserID);
    void SetAirshipChannelID(string airshipChannelID);
    void SetCleverTapID(string cleverTapID);
    void SetMixpanelDistinctID(string mixpanelDistinctID);
    void SetFirebaseAppInstanceID(string firebaseAppInstanceID);
    void SetMediaSource(string mediaSource);
    void SetCampaign(string campaign);
    void SetAdGroup(string adGroup);
    void SetAd(string ad);
    void SetKeyword(string keyword);
    void SetCreative(string creative);
    void SetAppsFlyerConversionData(string conversionDataJson);
    void CollectDeviceIdentifiers();
    void CanMakePayments(Purchases.BillingFeature[] features, string requestId = null);
    void GetPromotionalOffer(string productIdentifier, string discountIdentifier, string requestId = null);
    void ShowInAppMessages(Purchases.InAppMessageType[] messageTypes);
    void ParseAsWebPurchaseRedemption(string urlString, string requestId = null);
    void RedeemWebPurchase(Purchases.WebPurchaseRedemption webPurchaseRedemption, string requestId = null);
    void GetVirtualCurrencies(string requestId = null);
    string GetCachedVirtualCurrencies();
    void InvalidateVirtualCurrenciesCache();
    void GetEligibleWinBackOffersForProduct(Purchases.StoreProduct storeProduct, string requestId = null);
    void GetEligibleWinBackOffersForPackage(Purchases.Package package, string requestId = null);
    void PurchaseProductWithWinBackOffer(Purchases.StoreProduct storeProduct, Purchases.WinBackOffer winBackOffer,
        string requestId = null);
    void PurchasePackageWithWinBackOffer(Purchases.Package package, Purchases.WinBackOffer winBackOffer,
        string requestId = null);
    void TrackCustomPaywallImpression(Purchases.CustomPaywallImpressionParams parameters);
    void TrackAdDisplayed(AdDisplayedData data);
    void TrackAdOpened(AdOpenedData data);
    void TrackAdRevenue(AdRevenueData data);
    void TrackAdLoaded(AdLoadedData data);
    void TrackAdFailedToLoad(AdFailedToLoadData data);
}

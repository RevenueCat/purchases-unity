﻿using System;
using System.Collections.Generic;

public interface IPurchasesWrapper
{
    void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy,
        Purchases.StoreKitVersion storeKitVersion, string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
        bool shouldShowInAppMessagesAutomatically, bool pendingTransactionsForPrepaidPlansEnabled);

    void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy,
        Purchases.StoreKitVersion storeKitVersion, string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
        bool shouldShowInAppMessagesAutomatically, Purchases.EntitlementVerificationMode entitlementVerificationMode,
        bool pendingTransactionsForPrepaidPlansEnabled);

    void GetProducts(string[] productIdentifiers, string type = "subs");

    void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        bool googleIsPersonalizedPrice = false, string presentedOfferingIdentifier = null,
        Purchases.PromotionalOffer discount = null);

    void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        bool googleIsPersonalizedPrice = false, Purchases.PromotionalOffer discount = null);

    void PurchaseSubscriptionOption(Purchases.SubscriptionOption subscriptionOption,
        Purchases.GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false);

    void RestorePurchases();
    void LogIn(string appUserId);
    void LogOut();
    void SetAllowSharingStoreAccount(bool allow);
    void SetDebugLogsEnabled(bool enabled);
    void SetLogLevel(Purchases.LogLevel level);
    void SetLogHandler();
    void SetProxyURL(string proxyURL);
    string GetAppUserId();
    void GetCustomerInfo();
    void GetOfferings();
    void GetCurrentOfferingForPlacement(string placementIdentifier);
    void SyncAttributesAndOfferingsIfNeeded();
    void SyncPurchases();

    void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID, string isoCurrencyCode,
        double price);

    void GetAmazonLWAConsentStatus();
    void EnableAdServicesAttributionTokenCollection();
    bool IsAnonymous();
    bool IsConfigured();
    void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers);
    void InvalidateCustomerInfoCache();
    void PresentCodeRedemptionSheet();
    void RecordPurchase(string productID);
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
    void CollectDeviceIdentifiers();
    void CanMakePayments(Purchases.BillingFeature[] features);
    void GetPromotionalOffer(string productIdentifier, string discountIdentifier);
    void ShowInAppMessages(Purchases.InAppMessageType[] messageTypes);
    void ParseAsWebPurchaseRedemption(string urlString);
    void RedeemWebPurchase(Purchases.WebPurchaseRedemption webPurchaseRedemption);
    void GetEligibleWinBackOffersForProduct(Purchases.StoreProduct storeProduct);
    void GetEligibleWinBackOffersForPackage(Purchases.Package package);
    void PurchaseProductWithWinBackOffer(Purchases.StoreProduct storeProduct, Purchases.WinBackOffer winBackOffer);
    void PurchasePackageWithWinBackOffer(Purchases.Package package, Purchases.WinBackOffer winBackOffer);
}

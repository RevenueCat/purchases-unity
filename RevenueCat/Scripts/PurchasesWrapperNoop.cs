using System.Collections.Generic;
using RevenueCat;

public partial class Purchases
{
    private class PurchasesWrapperNoop : IPurchasesWrapper
    {
        public void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy,
            Purchases.StoreKitVersion storeKitVersion, string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
            bool shouldShowInAppMessagesAutomatically, bool pendingTransactionsForPrepaidPlansEnabled, bool diagnosticsEnabled,
            bool automaticDeviceIdentifierCollectionEnabled, string preferredUILocaleOverride)
        {
        }

        public void Setup(string gameObject, string apiKey, string appUserId, Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy,
            Purchases.StoreKitVersion storeKitVersion, string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
            bool shouldShowInAppMessagesAutomatically, Purchases.EntitlementVerificationMode entitlementVerificationMode,
            bool pendingTransactionsForPrepaidPlansEnabled, bool diagnosticsEnabled, bool automaticDeviceIdentifierCollectionEnabled,
            string preferredUILocaleOverride)
        {
        }

        public void GetStorefront(string requestId = null)
        {
        }

        public void GetProducts(string[] productIdentifiers, string type = "subs", string requestId = null)
        {
        }

        public void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null)
        {
        }

        public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false, string presentedOfferingIdentifier = null,
            Purchases.PromotionalOffer discount = null, string requestId = null)
        {
        }

        public void PurchasePackage(Package packageToPurchase, string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false, Purchases.PromotionalOffer discount = null,
            string requestId = null)
        {
        }

        public void PurchaseSubscriptionOption(Purchases.SubscriptionOption subscriptionOption,
            Purchases.GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false,
            string requestId = null)
        {
        }

        public void RestorePurchases(string requestId = null)
        {
        }

        public void LogIn(string appUserId, string requestId = null)
        {
        }

        public void LogOut(string requestId = null)
        {
        }

        public void SetAllowSharingStoreAccount(bool allow)
        {
        }

        public string GetAppUserId()
        {
            return null;
        }

        public void SetDebugLogsEnabled(bool enabled)
        {
        }

        public void SetLogLevel(LogLevel level)
        {
        }

        public void SetLogHandler()
        {
        }

        public void SetProxyURL(string proxyURL)
        {
        }

        public void GetCustomerInfo(string requestId = null)
        {
        }

        public void GetOfferings(string requestId = null)
        {
        }

        public void GetCurrentOfferingForPlacement(string placementIdentifier, string requestId = null)
        {
        }

        public void SyncAttributesAndOfferingsIfNeeded(string requestId = null)
        {
        }

        public void SyncPurchases(string requestId = null)
        {
        }

        public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID,
            string isoCurrencyCode, double price)
        {
        }
        
        public void GetAmazonLWAConsentStatus(string requestId = null)
        {
        }

        public void EnableAdServicesAttributionTokenCollection()
        {
        }

        public bool IsAnonymous()
        {
            return false;
        }

        public bool IsConfigured()
        {
            return false;
        }

        public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers, string requestId = null)
        {
        }

        public void InvalidateCustomerInfoCache()
        {
        }

        public void OverridePreferredUILocale(string locale)
        {
        }

        public void PresentCodeRedemptionSheet()
        {
        }

        public void RecordPurchase(string productID, string requestId = null)
        {
        }

        public void SetSimulatesAskToBuyInSandbox(bool enabled)
        {
        }

        public void SetAttributes(string attributesJson)
        {
        }

        public void SetEmail(string email)
        {
        }

        public void SetPhoneNumber(string phoneNumber)
        {
        }

        public void SetDisplayName(string displayName)
        {
        }

        public void SetPushToken(string token)
        {
        }

        public void SetAdjustID(string adjustID)
        {
        }

        public void SetAppsflyerID(string appsflyerID)
        {
        }

        public void SetFBAnonymousID(string fbAnonymousID)
        {
        }

        public void SetMparticleID(string mparticleID)
        {
        }

        public void SetOnesignalID(string onesignalID)
        {
        }

        public void SetOnesignalUserID(string onesignalUserID)
        {
        }

        public void SetAirshipChannelID(string airshipChannelID)
        {
        }

        public void SetCleverTapID(string cleverTapID)
        {
        }

        public void SetMixpanelDistinctID(string mixpanelDistinctID)
        {
        }

        public void SetFirebaseAppInstanceID(string firebaseAppInstanceID)
        {
        }

        public void SetMediaSource(string mediaSource)
        {
        }

        public void SetCampaign(string campaign)
        {
        }

        public void SetAdGroup(string adGroup)
        {
        }

        public void SetAd(string ad)
        {
        }

        public void SetKeyword(string keyword)
        {
        }

        public void SetCreative(string creative)
        {
        }

        public void SetAppsFlyerConversionData(string conversionDataJson)
        {
        }

        public void CollectDeviceIdentifiers()
        {
        }

        public void CanMakePayments(Purchases.BillingFeature[] features, string requestId = null)
        {
        }

        public void GetPromotionalOffer(string productIdentifier, string discountIdentifier, string requestId = null)
        {
        }

        public void ShowInAppMessages(Purchases.InAppMessageType[] messageTypes)
        {
        }

        public void ParseAsWebPurchaseRedemption(string urlString, string requestId = null)
        {
        }

        public void RedeemWebPurchase(Purchases.WebPurchaseRedemption webPurchaseRedemption, string requestId = null)
        {
        }

        public void GetVirtualCurrencies(string requestId = null)
        {
        }

        public string GetCachedVirtualCurrencies()
        {
            return null;
        }

        public void InvalidateVirtualCurrenciesCache()
        {
        }

        public void GetEligibleWinBackOffersForProduct(Purchases.StoreProduct storeProduct, string requestId = null)
        {
        }

        public void GetEligibleWinBackOffersForPackage(Purchases.Package package, string requestId = null)
        {
        }

        public void PurchaseProductWithWinBackOffer(Purchases.StoreProduct storeProduct,
            Purchases.WinBackOffer winBackOffer, string requestId = null)
        {
        }

        public void PurchasePackageWithWinBackOffer(Purchases.Package package, Purchases.WinBackOffer winBackOffer,
            string requestId = null)
        {
        }

        public void TrackCustomPaywallImpression(Purchases.CustomPaywallImpressionParams parameters)
        {
        }

        public void TrackAdDisplayed(AdDisplayedData data) { }
        public void TrackAdOpened(AdOpenedData data) { }
        public void TrackAdRevenue(AdRevenueData data) { }
        public void TrackAdLoaded(AdLoadedData data) { }
        public void TrackAdFailedToLoad(AdFailedToLoadData data) { }
    }
}

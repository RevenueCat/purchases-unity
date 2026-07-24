using System.Collections.Generic;
using RevenueCat;

namespace RevenueCat.Tests
{
    internal sealed class PurchasesWrapperSpy : IPurchasesWrapper
    {
        internal sealed class Invocation
        {
            internal readonly string Method;
            internal readonly object[] Arguments;

            internal Invocation(string method, object[] arguments)
            {
                Method = method;
                Arguments = arguments;
            }
        }

        internal readonly List<Invocation> Invocations = new List<Invocation>();
        internal Invocation LastInvocation => Invocations[Invocations.Count - 1];

        private void Record(string method, params object[] arguments)
        {
            Invocations.Add(new Invocation(method, arguments));
        }

        public void Setup(string gameObject, string apiKey, string appUserId,
            Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy, Purchases.StoreKitVersion storeKitVersion,
            string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
            bool shouldShowInAppMessagesAutomatically, bool pendingTransactionsForPrepaidPlansEnabled,
            bool diagnosticsEnabled, bool automaticDeviceIdentifierCollectionEnabled, string preferredUILocaleOverride)
        {
            Record(nameof(Setup), gameObject, apiKey, appUserId, purchasesAreCompletedBy, storeKitVersion,
                userDefaultsSuiteName, useAmazon, dangerousSettingsJson, shouldShowInAppMessagesAutomatically,
                pendingTransactionsForPrepaidPlansEnabled, diagnosticsEnabled,
                automaticDeviceIdentifierCollectionEnabled, preferredUILocaleOverride);
        }

        public void Setup(string gameObject, string apiKey, string appUserId,
            Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy, Purchases.StoreKitVersion storeKitVersion,
            string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson,
            bool shouldShowInAppMessagesAutomatically,
            Purchases.EntitlementVerificationMode entitlementVerificationMode,
            bool pendingTransactionsForPrepaidPlansEnabled, bool diagnosticsEnabled,
            bool automaticDeviceIdentifierCollectionEnabled, string preferredUILocaleOverride)
        {
            Record(nameof(Setup), gameObject, apiKey, appUserId, purchasesAreCompletedBy, storeKitVersion,
                userDefaultsSuiteName, useAmazon, dangerousSettingsJson, shouldShowInAppMessagesAutomatically,
                entitlementVerificationMode, pendingTransactionsForPrepaidPlansEnabled, diagnosticsEnabled,
                automaticDeviceIdentifierCollectionEnabled, preferredUILocaleOverride);
        }

        public void GetStorefront() => Record(nameof(GetStorefront));
        public void GetProducts(string[] productIdentifiers, string type = "subs") =>
            Record(nameof(GetProducts), productIdentifiers, type);

        public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
            Purchases.ProrationMode prorationMode =
                Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false, string presentedOfferingIdentifier = null,
            Purchases.PromotionalOffer discount = null) =>
            Record(nameof(PurchaseProduct), productIdentifier, type, oldSku, prorationMode,
                googleIsPersonalizedPrice, presentedOfferingIdentifier, discount);

        public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null,
            Purchases.ProrationMode prorationMode =
                Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false, Purchases.PromotionalOffer discount = null) =>
            Record(nameof(PurchasePackage), packageToPurchase, oldSku, prorationMode,
                googleIsPersonalizedPrice, discount);

        public void PurchaseSubscriptionOption(Purchases.SubscriptionOption subscriptionOption,
            Purchases.GoogleProductChangeInfo googleProductChangeInfo = null,
            bool googleIsPersonalizedPrice = false) =>
            Record(nameof(PurchaseSubscriptionOption), subscriptionOption, googleProductChangeInfo,
                googleIsPersonalizedPrice);

        public void RestorePurchases() => Record(nameof(RestorePurchases));
        public void LogIn(string appUserId) => Record(nameof(LogIn), appUserId);
        public void LogOut() => Record(nameof(LogOut));
        public void SetAllowSharingStoreAccount(bool allow) => Record(nameof(SetAllowSharingStoreAccount), allow);
        public void SetDebugLogsEnabled(bool enabled) => Record(nameof(SetDebugLogsEnabled), enabled);
        public void SetLogLevel(Purchases.LogLevel level) => Record(nameof(SetLogLevel), level);
        public void SetLogHandler() => Record(nameof(SetLogHandler));
        public void SetProxyURL(string proxyURL) => Record(nameof(SetProxyURL), proxyURL);
        public string GetAppUserId()
        {
            Record(nameof(GetAppUserId));
            return null;
        }

        public void GetCustomerInfo() => Record(nameof(GetCustomerInfo));
        public void GetOfferings() => Record(nameof(GetOfferings));
        public void GetCurrentOfferingForPlacement(string placementIdentifier) =>
            Record(nameof(GetCurrentOfferingForPlacement), placementIdentifier);

        public void SyncAttributesAndOfferingsIfNeeded() => Record(nameof(SyncAttributesAndOfferingsIfNeeded));
        public void SyncPurchases() => Record(nameof(SyncPurchases));

        public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID,
            string isoCurrencyCode, double price) =>
            Record(nameof(SyncAmazonPurchase), productID, receiptID, amazonUserID, isoCurrencyCode, price);

        public void GetAmazonLWAConsentStatus() => Record(nameof(GetAmazonLWAConsentStatus));
        public void EnableAdServicesAttributionTokenCollection() =>
            Record(nameof(EnableAdServicesAttributionTokenCollection));

        public bool IsAnonymous()
        {
            Record(nameof(IsAnonymous));
            return false;
        }

        public bool IsConfigured()
        {
            Record(nameof(IsConfigured));
            return false;
        }

        public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers) =>
            Record(nameof(CheckTrialOrIntroductoryPriceEligibility), productIdentifiers);

        public void InvalidateCustomerInfoCache() => Record(nameof(InvalidateCustomerInfoCache));
        public void OverridePreferredUILocale(string locale) => Record(nameof(OverridePreferredUILocale), locale);
        public void PresentCodeRedemptionSheet() => Record(nameof(PresentCodeRedemptionSheet));
        public void RecordPurchase(string productID) => Record(nameof(RecordPurchase), productID);
        public void SetSimulatesAskToBuyInSandbox(bool enabled) =>
            Record(nameof(SetSimulatesAskToBuyInSandbox), enabled);

        public void SetAttributes(string attributesJson) => Record(nameof(SetAttributes), attributesJson);
        public void SetEmail(string email) => Record(nameof(SetEmail), email);
        public void SetPhoneNumber(string phoneNumber) => Record(nameof(SetPhoneNumber), phoneNumber);
        public void SetDisplayName(string displayName) => Record(nameof(SetDisplayName), displayName);
        public void SetPushToken(string token) => Record(nameof(SetPushToken), token);
        public void SetAdjustID(string adjustID) => Record(nameof(SetAdjustID), adjustID);
        public void SetAppsflyerID(string appsflyerID) => Record(nameof(SetAppsflyerID), appsflyerID);
        public void SetFBAnonymousID(string fbAnonymousID) => Record(nameof(SetFBAnonymousID), fbAnonymousID);
        public void SetMparticleID(string mparticleID) => Record(nameof(SetMparticleID), mparticleID);
        public void SetOnesignalID(string onesignalID) => Record(nameof(SetOnesignalID), onesignalID);
        public void SetOnesignalUserID(string onesignalUserID) => Record(nameof(SetOnesignalUserID), onesignalUserID);
        public void SetAirshipChannelID(string airshipChannelID) =>
            Record(nameof(SetAirshipChannelID), airshipChannelID);

        public void SetCleverTapID(string cleverTapID) => Record(nameof(SetCleverTapID), cleverTapID);
        public void SetMixpanelDistinctID(string mixpanelDistinctID) =>
            Record(nameof(SetMixpanelDistinctID), mixpanelDistinctID);

        public void SetFirebaseAppInstanceID(string firebaseAppInstanceID) =>
            Record(nameof(SetFirebaseAppInstanceID), firebaseAppInstanceID);

        public void SetMediaSource(string mediaSource) => Record(nameof(SetMediaSource), mediaSource);
        public void SetCampaign(string campaign) => Record(nameof(SetCampaign), campaign);
        public void SetAdGroup(string adGroup) => Record(nameof(SetAdGroup), adGroup);
        public void SetAd(string ad) => Record(nameof(SetAd), ad);
        public void SetKeyword(string keyword) => Record(nameof(SetKeyword), keyword);
        public void SetCreative(string creative) => Record(nameof(SetCreative), creative);
        public void SetAppsFlyerConversionData(string conversionDataJson) =>
            Record(nameof(SetAppsFlyerConversionData), conversionDataJson);

        public void CollectDeviceIdentifiers() => Record(nameof(CollectDeviceIdentifiers));
        public void CanMakePayments(Purchases.BillingFeature[] features) =>
            Record(nameof(CanMakePayments), features);

        public void GetPromotionalOffer(string productIdentifier, string discountIdentifier) =>
            Record(nameof(GetPromotionalOffer), productIdentifier, discountIdentifier);

        public void ShowInAppMessages(Purchases.InAppMessageType[] messageTypes) =>
            Record(nameof(ShowInAppMessages), messageTypes);

        public void ParseAsWebPurchaseRedemption(string urlString) =>
            Record(nameof(ParseAsWebPurchaseRedemption), urlString);

        public void RedeemWebPurchase(Purchases.WebPurchaseRedemption webPurchaseRedemption) =>
            Record(nameof(RedeemWebPurchase), webPurchaseRedemption);

        public void GetVirtualCurrencies() => Record(nameof(GetVirtualCurrencies));

        public string GetCachedVirtualCurrencies()
        {
            Record(nameof(GetCachedVirtualCurrencies));
            return null;
        }

        public void InvalidateVirtualCurrenciesCache() => Record(nameof(InvalidateVirtualCurrenciesCache));
        public void GetEligibleWinBackOffersForProduct(Purchases.StoreProduct storeProduct) =>
            Record(nameof(GetEligibleWinBackOffersForProduct), storeProduct);

        public void GetEligibleWinBackOffersForPackage(Purchases.Package package) =>
            Record(nameof(GetEligibleWinBackOffersForPackage), package);

        public void PurchaseProductWithWinBackOffer(Purchases.StoreProduct storeProduct,
            Purchases.WinBackOffer winBackOffer) =>
            Record(nameof(PurchaseProductWithWinBackOffer), storeProduct, winBackOffer);

        public void PurchasePackageWithWinBackOffer(Purchases.Package package,
            Purchases.WinBackOffer winBackOffer) =>
            Record(nameof(PurchasePackageWithWinBackOffer), package, winBackOffer);

        public void TrackCustomPaywallImpression(Purchases.CustomPaywallImpressionParams parameters) =>
            Record(nameof(TrackCustomPaywallImpression), parameters);

        public void TrackAdDisplayed(AdDisplayedData data) => Record(nameof(TrackAdDisplayed), data);
        public void TrackAdOpened(AdOpenedData data) => Record(nameof(TrackAdOpened), data);
        public void TrackAdRevenue(AdRevenueData data) => Record(nameof(TrackAdRevenue), data);
        public void TrackAdLoaded(AdLoadedData data) => Record(nameof(TrackAdLoaded), data);
        public void TrackAdFailedToLoad(AdFailedToLoadData data) => Record(nameof(TrackAdFailedToLoad), data);
    }
}

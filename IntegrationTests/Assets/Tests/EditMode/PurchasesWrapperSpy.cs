using System;
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
            internal readonly string RequestId;

            internal Invocation(string method, object[] arguments, string requestId = null)
            {
                Method = method;
                Arguments = arguments;
                RequestId = requestId;
            }
        }

        internal readonly List<Invocation> Invocations = new List<Invocation>();

        internal Invocation LastInvocation =>
            Invocations.Count > 0
                ? Invocations[Invocations.Count - 1]
                : throw new InvalidOperationException(
                    $"No {nameof(IPurchasesWrapper)} calls were recorded on the spy.");

        private void Record(string method, params object[] arguments)
        {
            Invocations.Add(new Invocation(method, arguments));
        }

        private void RecordRequest(string method, string requestId, params object[] arguments)
        {
            Invocations.Add(new Invocation(method, arguments, requestId));
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

        public void GetStorefront(string requestId = null) =>
            RecordRequest(nameof(GetStorefront), requestId);
        public void GetProducts(string[] productIdentifiers, string type = "subs", string requestId = null) =>
            RecordRequest(nameof(GetProducts), requestId, productIdentifiers, type);

        public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
            Purchases.ProrationMode prorationMode =
                Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false, string presentedOfferingIdentifier = null,
            Purchases.PromotionalOffer discount = null, string requestId = null) =>
            RecordRequest(nameof(PurchaseProduct), requestId, productIdentifier, type, oldSku, prorationMode,
                googleIsPersonalizedPrice, presentedOfferingIdentifier, discount);

        public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null,
            Purchases.ProrationMode prorationMode =
                Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false, Purchases.PromotionalOffer discount = null,
            string requestId = null) =>
            RecordRequest(nameof(PurchasePackage), requestId, packageToPurchase, oldSku, prorationMode,
                googleIsPersonalizedPrice, discount);

        public void PurchaseSubscriptionOption(Purchases.SubscriptionOption subscriptionOption,
            Purchases.GoogleProductChangeInfo googleProductChangeInfo = null,
            bool googleIsPersonalizedPrice = false, string requestId = null) =>
            RecordRequest(nameof(PurchaseSubscriptionOption), requestId, subscriptionOption, googleProductChangeInfo,
                googleIsPersonalizedPrice);

        public void RestorePurchases(string requestId = null) =>
            RecordRequest(nameof(RestorePurchases), requestId);
        public void LogIn(string appUserId, string requestId = null) =>
            RecordRequest(nameof(LogIn), requestId, appUserId);
        public void LogOut(string requestId = null) =>
            RecordRequest(nameof(LogOut), requestId);
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

        public void GetCustomerInfo(string requestId = null) =>
            RecordRequest(nameof(GetCustomerInfo), requestId);
        public void GetOfferings(string requestId = null) =>
            RecordRequest(nameof(GetOfferings), requestId);
        public void GetCurrentOfferingForPlacement(string placementIdentifier, string requestId = null) =>
            RecordRequest(nameof(GetCurrentOfferingForPlacement), requestId, placementIdentifier);

        public void SyncAttributesAndOfferingsIfNeeded(string requestId = null) =>
            RecordRequest(nameof(SyncAttributesAndOfferingsIfNeeded), requestId);
        public void SyncPurchases(string requestId = null) =>
            RecordRequest(nameof(SyncPurchases), requestId);

        public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID,
            string isoCurrencyCode, double price) =>
            Record(nameof(SyncAmazonPurchase), productID, receiptID, amazonUserID, isoCurrencyCode, price);

        public void GetAmazonLWAConsentStatus(string requestId = null) =>
            RecordRequest(nameof(GetAmazonLWAConsentStatus), requestId);
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

        // The cast keeps `string[]` from binding as the `params object[]` array itself (array covariance), so the
        // identifiers are recorded as one argument, like every other method here.
        public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers, string requestId = null) =>
            RecordRequest(nameof(CheckTrialOrIntroductoryPriceEligibility), requestId, (object)productIdentifiers);

        public void InvalidateCustomerInfoCache() => Record(nameof(InvalidateCustomerInfoCache));
        public void OverridePreferredUILocale(string locale) => Record(nameof(OverridePreferredUILocale), locale);
        public void PresentCodeRedemptionSheet() => Record(nameof(PresentCodeRedemptionSheet));
        public void RecordPurchase(string productID, string requestId = null) =>
            RecordRequest(nameof(RecordPurchase), requestId, productID);
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
        public void SetSingularDeviceID(string singularDeviceID) => Record(nameof(SetSingularDeviceID), singularDeviceID);
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
        public void CanMakePayments(Purchases.BillingFeature[] features, string requestId = null) =>
            RecordRequest(nameof(CanMakePayments), requestId, features);

        public void GetPromotionalOffer(string productIdentifier, string discountIdentifier, string requestId = null) =>
            RecordRequest(nameof(GetPromotionalOffer), requestId, productIdentifier, discountIdentifier);

        public void ShowInAppMessages(Purchases.InAppMessageType[] messageTypes) =>
            Record(nameof(ShowInAppMessages), messageTypes);

        public void ParseAsWebPurchaseRedemption(string urlString, string requestId = null) =>
            RecordRequest(nameof(ParseAsWebPurchaseRedemption), requestId, urlString);

        public void RedeemWebPurchase(Purchases.WebPurchaseRedemption webPurchaseRedemption,
            string requestId = null) =>
            RecordRequest(nameof(RedeemWebPurchase), requestId, webPurchaseRedemption);

        public void GetVirtualCurrencies(string requestId = null) =>
            RecordRequest(nameof(GetVirtualCurrencies), requestId);

        public string GetCachedVirtualCurrencies()
        {
            Record(nameof(GetCachedVirtualCurrencies));
            return null;
        }

        public void InvalidateVirtualCurrenciesCache() => Record(nameof(InvalidateVirtualCurrenciesCache));
        public void GetEligibleWinBackOffersForProduct(Purchases.StoreProduct storeProduct, string requestId = null) =>
            RecordRequest(nameof(GetEligibleWinBackOffersForProduct), requestId, storeProduct);

        public void GetEligibleWinBackOffersForPackage(Purchases.Package package, string requestId = null) =>
            RecordRequest(nameof(GetEligibleWinBackOffersForPackage), requestId, package);

        public void PurchaseProductWithWinBackOffer(Purchases.StoreProduct storeProduct,
            Purchases.WinBackOffer winBackOffer, string requestId = null) =>
            RecordRequest(nameof(PurchaseProductWithWinBackOffer), requestId, storeProduct, winBackOffer);

        public void PurchasePackageWithWinBackOffer(Purchases.Package package,
            Purchases.WinBackOffer winBackOffer, string requestId = null) =>
            RecordRequest(nameof(PurchasePackageWithWinBackOffer), requestId, package, winBackOffer);

        public void TrackCustomPaywallImpression(Purchases.CustomPaywallImpressionParams parameters) =>
            Record(nameof(TrackCustomPaywallImpression), parameters);

        public void TrackAdDisplayed(AdDisplayedData data) => Record(nameof(TrackAdDisplayed), data);
        public void TrackAdOpened(AdOpenedData data) => Record(nameof(TrackAdOpened), data);
        public void TrackAdRevenue(AdRevenueData data) => Record(nameof(TrackAdRevenue), data);
        public void TrackAdLoaded(AdLoadedData data) => Record(nameof(TrackAdLoaded), data);
        public void TrackAdFailedToLoad(AdFailedToLoadData data) => Record(nameof(TrackAdFailedToLoad), data);

        public void GenerateRewardVerificationToken(string impressionId) =>
            Record(nameof(GenerateRewardVerificationToken), impressionId);

        public void PollRewardVerification(string clientTransactionId,
            RevenueCat.RewardedAdTrackingMetadata trackingMetadata = null) =>
            Record(nameof(PollRewardVerification), clientTransactionId, trackingMetadata);
    }
}

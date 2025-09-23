using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RevenueCat
{
    internal class PurchasesWrapperNoop : IPurchasesWrapper
    {
#pragma warning disable CS0067 // Event is never used
        public event Action<CustomerInfo> OnCustomerInfoUpdated;
        public event Action<RevenueCatLogMessage> OnLogMessage;
#pragma warning restore CS0067 // Event is never used
        public void Configure(PurchasesConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public Task<Storefront> GetStorefrontAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<StoreProduct>> GetProductsAsync(string[] productIdentifiers, string type = "subs", CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> PurchaseProductAsync(
            string productIdentifier,
            string type = "subs",
            string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false,
            string offeringIdentifier = null,
            PromotionalOffer discount = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> PurchasePackageAsync(
            Package packageToPurchase,
            string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false,
            PromotionalOffer discount = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> PurchaseSubscriptionOptionAsync(SubscriptionOption subscriptionOption, GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerInfo> RestorePurchasesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResult> LogInAsync(string appUserId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerInfo> LogOutAsync()
        {
            throw new NotImplementedException();
        }

        public void SetAllowSharingStoreAccount(bool allow)
        {
            throw new NotImplementedException();
        }

        public Task<Offerings> GetOfferingsAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Offering> GetCurrentOfferingForPlacementAsync(string placementIdentifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Offerings> SyncAttributesAndOfferingsIfNeededAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID, string isoCurrencyCode, double price)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetAmazonLWAConsentStatusAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void SetLogLevel(LogLevel level)
        {
            throw new NotImplementedException();
        }

        public void SetDebugLogsEnabled(bool enabled)
        {
            throw new NotImplementedException();
        }

        public void SetProxyURL(string proxyURL)
        {
            throw new NotImplementedException();
        }

        public string GetAppUserId()
        {
            throw new NotImplementedException();
        }

        public Task<CustomerInfo> GetCustomerInfoAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<CustomerInfo> SyncPurchasesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void EnableAdServicesAttributionTokenCollection()
        {
            throw new NotImplementedException();
        }

        public bool IsAnonymous()
        {
            throw new NotImplementedException();
        }

        public bool IsConfigured()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<string, IntroEligibility>> CheckTrialOrIntroductoryPriceEligibilityAsync(string[] productIdentifiers, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void InvalidateCustomerInfoCache()
        {
            throw new NotImplementedException();
        }

        public void PresentCodeRedemptionSheet()
        {
            throw new NotImplementedException();
        }

        public void RecordPurchase(string productID)
        {
            throw new NotImplementedException();
        }

        public void SetSimulatesAskToBuyInSandbox(bool enabled)
        {
            throw new NotImplementedException();
        }

        public void SetAttributes(Dictionary<string, string> attributes)
        {
            throw new NotImplementedException();
        }

        public void SetEmail(string email)
        {
            throw new NotImplementedException();
        }

        public void SetPhoneNumber(string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public void SetDisplayName(string displayName)
        {
            throw new NotImplementedException();
        }

        public void SetPushToken(string token)
        {
            throw new NotImplementedException();
        }

        public void SetAdjustID(string adjustID)
        {
            throw new NotImplementedException();
        }

        public void SetAppsflyerID(string appsflyerID)
        {
            throw new NotImplementedException();
        }

        public void SetFBAnonymousID(string fbAnonymousID)
        {
            throw new NotImplementedException();
        }

        public void SetMparticleID(string mparticleID)
        {
            throw new NotImplementedException();
        }

        public void SetOnesignalID(string onesignalID)
        {
            throw new NotImplementedException();
        }

        public void SetAirshipChannelID(string airshipChannelID)
        {
            throw new NotImplementedException();
        }

        public void SetCleverTapID(string cleverTapID)
        {
            throw new NotImplementedException();
        }

        public void SetMixpanelDistinctID(string mixpanelDistinctID)
        {
            throw new NotImplementedException();
        }

        public void SetFirebaseAppInstanceID(string firebaseAppInstanceID)
        {
            throw new NotImplementedException();
        }

        public void SetMediaSource(string mediaSource)
        {
            throw new NotImplementedException();
        }

        public void SetCampaign(string campaign)
        {
            throw new NotImplementedException();
        }

        public void SetAdGroup(string adGroup)
        {
            throw new NotImplementedException();
        }

        public void SetAd(string ad)
        {
            throw new NotImplementedException();
        }

        public void SetKeyword(string keyword)
        {
            throw new NotImplementedException();
        }

        public void SetCreative(string creative)
        {
            throw new NotImplementedException();
        }

        public void CollectDeviceIdentifiers()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CanMakePaymentsAsync(BillingFeature[] features, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PromotionalOffer> GetPromotionalOfferAsync(string productIdentifier, string discountIdentifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void ShowInAppMessages(InAppMessageType[] messageTypes)
        {
            throw new NotImplementedException();
        }

        public Task<WebPurchaseRedemption> ParseAsWebPurchaseRedemptionAsync(string urlString, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<WebPurchaseRedemptionResult> RedeemWebPurchaseAsync(WebPurchaseRedemption webPurchaseRedemption, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<VirtualCurrencies> GetVirtualCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public VirtualCurrencies GetCachedVirtualCurrencies()
        {
            throw new NotImplementedException();
        }

        public void InvalidateVirtualCurrenciesCache()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<WinBackOffer>> GetEligibleWinBackOffersForProductAsync(StoreProduct storeProduct, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<WinBackOffer>> GetEligibleWinBackOffersForPackageAsync(Package package, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> PurchaseProductWithWinBackOfferAsync(StoreProduct storeProduct, WinBackOffer winBackOffer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseResult> PurchasePackageWithWinBackOfferAsync(Package package, WinBackOffer winBackOffer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

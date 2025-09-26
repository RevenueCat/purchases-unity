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
        }

        public Task<Storefront> GetStorefrontAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Storefront>(null);
        }

        public Task<IReadOnlyList<StoreProduct>> GetProductsAsync(string[] productIdentifiers, string type = "subs", CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<StoreProduct>>(Array.Empty<StoreProduct>());
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
            return Task.FromResult<PurchaseResult>(null);
        }

        public Task<PurchaseResult> PurchasePackageAsync(
            Package packageToPurchase,
            string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false,
            PromotionalOffer discount = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PurchaseResult>(null);
        }

        public Task<PurchaseResult> PurchaseSubscriptionOptionAsync(SubscriptionOption subscriptionOption, GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PurchaseResult>(null);
        }

        public Task<CustomerInfo> RestorePurchasesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CustomerInfo>(null);
        }

        public Task<LoginResult> LogInAsync(string appUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<LoginResult>(null);
        }

        public Task<CustomerInfo> LogOutAsync()
        {
            return Task.FromResult<CustomerInfo>(null);
        }

        public void SetAllowSharingStoreAccount(bool allow)
        {
        }

        public Task<Offerings> GetOfferingsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Offerings>(null);
        }

        public Task<Offering> GetCurrentOfferingForPlacementAsync(string placementIdentifier, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Offering>(null);
        }

        public Task<Offerings> SyncAttributesAndOfferingsIfNeededAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Offerings>(null);
        }

        public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID, string isoCurrencyCode, double price)
        {
        }

        public Task<bool> GetAmazonLWAConsentStatusAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public void SetLogLevel(LogLevel level)
        {
        }

        public void SetDebugLogsEnabled(bool enabled)
        {
        }

        public void SetProxyURL(string proxyURL)
        {
        }

        public string GetAppUserId()
        {
            return null;
        }

        public Task<CustomerInfo> GetCustomerInfoAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CustomerInfo>(null);
        }

        public Task<CustomerInfo> SyncPurchasesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CustomerInfo>(null);
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

        public Task<IReadOnlyDictionary<string, IntroEligibility>> CheckTrialOrIntroductoryPriceEligibilityAsync(string[] productIdentifiers, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyDictionary<string, IntroEligibility>>(new Dictionary<string, IntroEligibility>());
        }

        public void InvalidateCustomerInfoCache()
        {
        }

        public void PresentCodeRedemptionSheet()
        {
        }

        public void RecordPurchase(string productID)
        {
        }

        public void SetSimulatesAskToBuyInSandbox(bool enabled)
        {
        }

        public void SetAttributes(Dictionary<string, string> attributes)
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

        public void CollectDeviceIdentifiers()
        {
        }

        public Task<bool> CanMakePaymentsAsync(BillingFeature[] features, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<PromotionalOffer> GetPromotionalOfferAsync(string productIdentifier, string discountIdentifier, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PromotionalOffer>(null);
        }

        public void ShowInAppMessages(InAppMessageType[] messageTypes)
        {
        }

        public Task<WebPurchaseRedemption> ParseAsWebPurchaseRedemptionAsync(string urlString, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<WebPurchaseRedemption>(null);
        }

        public Task<WebPurchaseRedemptionResult> RedeemWebPurchaseAsync(WebPurchaseRedemption webPurchaseRedemption, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<WebPurchaseRedemptionResult>(null);
        }

        public Task<VirtualCurrencies> GetVirtualCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<VirtualCurrencies>(null);
        }

        public VirtualCurrencies GetCachedVirtualCurrencies()
        {
            return null;
        }

        public void InvalidateVirtualCurrenciesCache()
        {
        }

        public Task<IReadOnlyList<WinBackOffer>> GetEligibleWinBackOffersForProductAsync(StoreProduct storeProduct, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<WinBackOffer>>(Array.Empty<WinBackOffer>());
        }

        public Task<IReadOnlyList<WinBackOffer>> GetEligibleWinBackOffersForPackageAsync(Package package, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<WinBackOffer>>(Array.Empty<WinBackOffer>());
        }

        public Task<PurchaseResult> PurchaseProductWithWinBackOfferAsync(StoreProduct storeProduct, WinBackOffer winBackOffer, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PurchaseResult>(null);
        }

        public Task<PurchaseResult> PurchasePackageWithWinBackOfferAsync(Package package, WinBackOffer winBackOffer, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PurchaseResult>(null);
        }

        public void Dispose()
        {
        }
    }
}

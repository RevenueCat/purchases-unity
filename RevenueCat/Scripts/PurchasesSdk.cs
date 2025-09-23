using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RevenueCat
{
    public static class PurchasesSdk
    {
        #region Instance

        internal static IPurchasesWrapper Instance => _instance ??= CreatePlatformWrapper();
        private static IPurchasesWrapper _instance;

        private static IPurchasesWrapper CreatePlatformWrapper()
        {
            // ReSharper disable once JoinDeclarationAndInitializer
            IPurchasesWrapper wrapper;
#if UNITY_IOS && !UNITY_EDITOR
            wrapper = new IOSPurchasesWrapper();
#elif UNITY_ANDROID && !UNITY_EDITOR
            wrapper = new AndroidPurchasesWrapper();
#else
            // Editor/other: unsupported implementation
            wrapper = new PurchasesWrapperNoop();
#endif
            return wrapper;
        }

        private static void Reset()
        {
            _instance?.Dispose();
            _instance = null;
            Configuration = null;
        }

        #endregion Instance

        internal static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new DefaultNamingStrategy()),
            }
        };

        internal static JsonSerializer JsonSerializer { get; } = JsonSerializer.Create(JsonSerializerSettings);

        public static PurchasesConfiguration Configuration { get; private set; }

        public static event Action<CustomerInfo> OnCustomerInfoUpdated
        {
            add => Instance.OnCustomerInfoUpdated += value;
            remove => Instance.OnCustomerInfoUpdated -= value;
        }

        public static event Action<RevenueCatLogMessage> OnLogMessage
        {
            add => Instance.OnLogMessage += value;
            remove => Instance.OnLogMessage -= value;
        }

        public static void Configure(PurchasesConfiguration configuration)
        {
            configuration ??= new PurchasesConfiguration();

            if (Configuration != null)
            {
                Reset();
            }

            Configuration = configuration;
            Instance.Configure(Configuration);
        }

        public static void Configure(string userId)
        {
            if (Configuration != null)
            {
                Reset();
            }

            Configuration = new PurchasesConfiguration(userId);
            Instance.Configure(Configuration);
        }

        /// <summary>
        /// Fetches the <c>Storefront</c> for the customer's current store account.
        /// If there is an error, the callback will be called with a null value.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Storefront"/>.</returns>
        public static Task<Storefront> GetStorefrontAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.GetStorefrontAsync(cancellationToken);
        }

        /// <summary>
        /// Fetches the <c>StoreProducts</c> for your IAPs for given productIdentifiers.
        /// This method is called automatically with products pre-configured through Unity IDE UI.
        /// You can optionally call this if you want to fetch more products.
        /// </summary>
        /// <param name="productIdentifiers"></param>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<IReadOnlyList<StoreProduct>> GetProductsAsync(
            string[] productIdentifiers,
            string type = "subs",
            CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.GetProductsAsync(productIdentifiers, type, cancellationToken);
        }

        public static Task<PurchaseResult> PurchaseProductAsync(
            string productIdentifier,
            string type = "subs",
            string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false,
            string presentedOfferingIdentifier = null,
            PromotionalOffer discount = null,
            CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.PurchaseProductAsync(
                productIdentifier,
                type,
                oldSku,
                prorationMode,
                googleIsPersonalizedPrice,
                presentedOfferingIdentifier,
                discount,
                cancellationToken);
        }

        public static Task<PurchaseResult> PurchaseDiscountedProductAsync(string productIdentifier, PromotionalOffer discount, CancellationToken cancellationToken)
        {
            return PurchaseProductAsync(productIdentifier, discount: discount, cancellationToken: cancellationToken);
        }

        public static Task<PurchaseResult> PurchasePackageAsync(
            Package packageToPurchase,
            string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false,
            PromotionalOffer discount = null,
            CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.PurchasePackageAsync(
                packageToPurchase,
                oldSku,
                prorationMode,
                googleIsPersonalizedPrice,
                discount,
                cancellationToken);
        }

        public static Task<PurchaseResult> PurchaseDiscountedPackage(Package packageToPurchase, PromotionalOffer discount, CancellationToken cancellationToken = default)
        {
            return PurchasePackageAsync(packageToPurchase, discount: discount, cancellationToken: cancellationToken);
        }

        public static Task<PurchaseResult> PurchaseSubscriptionOptionAsync(
            SubscriptionOption subscriptionOption,
            GoogleProductChangeInfo googleProductChangeInfo = null,
            bool googleIsPersonalizedPrice = false,
            CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.PurchaseSubscriptionOptionAsync(
                subscriptionOption,
                googleProductChangeInfo,
                googleIsPersonalizedPrice,
                cancellationToken);
        }

        public static Task<CustomerInfo> RestorePurchasesAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.RestorePurchasesAsync(cancellationToken);
        }

        public static Task<LoginResult> LogInAsync(string appUserId, CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();

            if (string.IsNullOrWhiteSpace(appUserId))
            {
                throw new ArgumentException("appUserId cannot be null or empty.", nameof(appUserId));
            }

            return Instance.LogInAsync(appUserId, cancellationToken);
        }

        public static Task<CustomerInfo> LogOutAsync()
        {
            ValidateConfiguration();
            return Instance.LogOutAsync();
        }

        [Obsolete("Configure behavior through the RevenueCat dashboard instead. " +
                  "If you have configured the 'Legacy' restore behavior in the " +
                  "RevenueCat Dashboard and are currently setting this to true, " +
                  "keep this setting active.")]
        public static void SetAllowSharingStoreAccount(bool allow)
        {
            ValidateConfiguration();
            Instance.SetAllowSharingStoreAccount(allow);
        }

        public static Task<Offerings> GetOfferingsAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.GetOfferingsAsync(cancellationToken);
        }

        public static Task<Offering> GetCurrentOfferingForPlacementAsync(string placementIdentifier, CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.GetCurrentOfferingForPlacementAsync(placementIdentifier, cancellationToken);
        }

        public static Task<Offerings> SyncAttributesAndOfferingsIfNeededAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.SyncAttributesAndOfferingsIfNeededAsync(cancellationToken);
        }

        public static void SyncAmazonPurchase(string productId, string receiptId, string amazonUserId, string isoCurrencyCode, double price)
        {
            ValidateConfiguration();
            Instance.SyncAmazonPurchase(productId, receiptId, amazonUserId, isoCurrencyCode, price);
        }

        public static Task<bool> GetAmazonLWAConsentStatusAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.GetAmazonLWAConsentStatusAsync(cancellationToken);
        }

        public static void SetLogLevel(LogLevel level)
        {
            ValidateConfiguration();
            Instance.SetLogLevel(level);
        }

        public static void SetDebugLogsEnabled(bool enabled)
        {
            ValidateConfiguration();
            Instance.SetDebugLogsEnabled(enabled);
        }

        public static void SetProxyURL(string proxyURL)
        {
            ValidateConfiguration();
            Instance.SetProxyURL(proxyURL);
        }

        public static string GetAppUserId()
        {
            ValidateConfiguration();
            return Instance.GetAppUserId();
        }

        public static Task<CustomerInfo> GetCustomerInfoAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.GetCustomerInfoAsync(cancellationToken);
        }

        public static Task<CustomerInfo> SyncPurchasesAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.SyncPurchasesAsync(cancellationToken);
        }

        public static void EnableAdServicesAttributionTokenCollection()
        {
            ValidateConfiguration();
            Instance.EnableAdServicesAttributionTokenCollection();
        }

        public static bool IsAnonymous()
        {
            ValidateConfiguration();
            return Instance.IsAnonymous();
        }

        public static bool IsConfigured()
        {
            ValidateConfiguration();
            return Instance.IsConfigured();
        }

        public static Task<IReadOnlyDictionary<string, IntroEligibility>> CheckTrialOrIntroductoryPriceEligibilityAsync(
            string[] productIdentifiers,
            CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.CheckTrialOrIntroductoryPriceEligibilityAsync(productIdentifiers, cancellationToken);
        }

        public static void InvalidateCustomInfoCache()
        {
            ValidateConfiguration();
            Instance.InvalidateCustomerInfoCache();
        }

        public static void PresentCodeRedemptionSheet()
        {
            ValidateConfiguration();
            Instance.PresentCodeRedemptionSheet();
        }

        public static void RecordPurchase(string productId)
        {
            ValidateConfiguration();
            Instance.RecordPurchase(productId);
        }

        public static void SetSimulatesAskToBuyInSandbox(bool enabled)
        {
            ValidateConfiguration();
            Instance.SetSimulatesAskToBuyInSandbox(enabled);
        }

        public static void SetAttributes(string attributesJson)
        {
            ValidateConfiguration();
            Instance.SetAttributes(attributesJson);
        }

        public static void SetEmail(string email)
        {
            ValidateConfiguration();
            Instance.SetEmail(email);
        }

        public static void SetPhoneNumber(string phoneNumber)
        {
            ValidateConfiguration();
            Instance.SetPhoneNumber(phoneNumber);
        }

        public static void SetDisplayName(string displayName)
        {
            ValidateConfiguration();
            Instance.SetDisplayName(displayName);
        }

        public static void SetPushToken(string token)
        {
            ValidateConfiguration();
            Instance.SetPushToken(token);
        }

        public static void SetAdjustID(string adjustID)
        {
            ValidateConfiguration();
            Instance.SetAdjustID(adjustID);
        }

        public static void SetAppsflyerID(string appsFlyerID)
        {
            ValidateConfiguration();
            Instance.SetAppsflyerID(appsFlyerID);
        }

        public static void SetFBAnonymousID(string fbAnonymousID)
        {
            ValidateConfiguration();
            Instance.SetFBAnonymousID(fbAnonymousID);
        }

        public static void SetMparticleID(string mparticleID)
        {
            ValidateConfiguration();
            Instance.SetMparticleID(mparticleID);
        }

        public static void SetOnesignalID(string onesignalID)
        {
            ValidateConfiguration();
            Instance.SetOnesignalID(onesignalID);
        }

        public static void SetAirshipChannelID(string airshipChannelID)
        {
            ValidateConfiguration();
            Instance.SetAirshipChannelID(airshipChannelID);
        }

        public static void SetCleverTapID(string cleverTapID)
        {
            ValidateConfiguration();
            Instance.SetCleverTapID(cleverTapID);
        }

        public static void SetMixpanelDistinctID(string mixpanelDistinctID)
        {
            ValidateConfiguration();
            Instance.SetMixpanelDistinctID(mixpanelDistinctID);
        }

        public static void SetFirebaseAppInstanceID(string firebaseAppInstanceID)
        {
            ValidateConfiguration();
            Instance.SetFirebaseAppInstanceID(firebaseAppInstanceID);
        }

        public static void SetMediaSource(string mediaSource)
        {
            ValidateConfiguration();
            Instance.SetMediaSource(mediaSource);
        }

        public static void SetCampaign(string campaign)
        {
            ValidateConfiguration();
            Instance.SetCampaign(campaign);
        }

        public static void SetAdGroup(string adGroup)
        {
            ValidateConfiguration();
            Instance.SetAdGroup(adGroup);
        }

        public static void SetAd(string ad)
        {
            ValidateConfiguration();
            Instance.SetAd(ad);
        }

        public static void SetKeyword(string keyword)
        {
            ValidateConfiguration();
            Instance.SetKeyword(keyword);
        }

        public static void SetCreative(string creative)
        {
            ValidateConfiguration();
            Instance.SetCreative(creative);
        }

        public static void CollectDeviceIdentifiers()
        {
            ValidateConfiguration();
            Instance.CollectDeviceIdentifiers();
        }

        public static Task<bool> CanMakePaymentsAsync(
            BillingFeature[] features,
            CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.CanMakePaymentsAsync(features, cancellationToken);
        }

        public static Task<PromotionalOffer> GetPromotionalOffer(
            string productIdentifier,
            string discountIdentifier)
        {
            ValidateConfiguration();
            return Instance.GetPromotionalOffer(productIdentifier, discountIdentifier);
        }

        public static void ShowInAppMessages(InAppMessageType[] messageTypes)
        {
            ValidateConfiguration();
            Instance.ShowInAppMessages(messageTypes);
        }

        public static Task<WebPurchaseRedemption> ParseAsWebPurchaseRedemptionAsync(
            string urlString,
            CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.ParseAsWebPurchaseRedemptionAsync(urlString, cancellationToken);
        }

        public static Task<WebPurchaseRedemptionResult> RedeemWebPurchaseAsync(
            WebPurchaseRedemption webPurchaseRedemption,
            CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.RedeemWebPurchaseAsync(webPurchaseRedemption, cancellationToken);
        }

        public static Task<VirtualCurrencies> GetVirtualCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            return Instance.GetVirtualCurrenciesAsync(cancellationToken);
        }

        public static VirtualCurrencies GetCachedVirtualCurrencies()
        {
            ValidateConfiguration();
            return Instance.GetCachedVirtualCurrencies();
        }

        public static void InvalidateVirtualCurrenciesCache()
        {
            ValidateConfiguration();
            Instance.InvalidateVirtualCurrenciesCache();
        }

        public static void GetEligibleWinBackOffersForProduct(StoreProduct storeProduct)
        {
            ValidateConfiguration();
            Instance.GetEligibleWinBackOffersForProduct(storeProduct);
        }

        public static void GetEligibleWinBackOffersForPackage(Package package)
        {
            ValidateConfiguration();
            Instance.GetEligibleWinBackOffersForPackage(package);
        }

        public static void PurchaseProductWithWinBackOffer(StoreProduct storeProduct, WinBackOffer winBackOffer)
        {
            ValidateConfiguration();
            Instance.PurchaseProductWithWinBackOffer(storeProduct, winBackOffer);
        }

        public static void PurchasePackageWithWinBackOffer(Package package, WinBackOffer winBackOffer)
        {
            ValidateConfiguration();
            Instance.PurchasePackageWithWinBackOffer(package, winBackOffer);
        }

        private static void ValidateConfiguration()
        {
            if (Configuration == null)
            {
                throw new InvalidOperationException("Configure must be called before using the SDK.");
            }
        }
    }
}

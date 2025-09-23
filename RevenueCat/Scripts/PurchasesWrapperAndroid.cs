#if UNITY_ANDROID // && !UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCat
{
    public class PurchasesWrapperAndroid : IPurchasesWrapper
    {
        private static readonly object _lock = new object();
        private static ConcurrentDictionary<string, object> _pendingTasks = new ConcurrentDictionary<string, object>();

        private LogHandler _logHandler;
        private CustomerInfoHandler _customerInfoHandler;

        public event Action<CustomerInfo> OnCustomerInfoUpdated;
        public event Action<RevenueCatLogMessage> OnLogMessage;

        ~PurchasesWrapperAndroid()
        {
            Dispose();
        }

        public void Dispose()
        {
            // clear event handlers to clean up memory
            OnCustomerInfoUpdated = null;
            OnLogMessage = null;
        }

        public void Configure(PurchasesConfiguration configuration)
        {
            _customerInfoHandler = new CustomerInfoHandler(OnCustomerInfoUpdated);
            _logHandler = new LogHandler(OnLogMessage);
            CallPurchases("configure",
                _customerInfoHandler,
                _logHandler,
                configuration.ApiKey,
                configuration.AppUserId,
                configuration.PurchasesAreCompletedBy.ToString(),
                configuration.UserDefaultsSuiteName,
                configuration.UseAmazon,
                configuration.ShouldShowInAppMessagesAutomatically,
                configuration.DangerousSettings.AutoSyncPurchases,
                configuration.EntitlementVerificationMode.ToString(),
                configuration.PendingTransactionsForPrepaidPlansEnabled);
        }

        public Task<Storefront> GetStorefrontAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetStorefrontAsync), out var value) &&
                    value is AndroidPurchasesCallback<Storefront> pending)
                {
                    return pending.Task;
                }
                var callback = new AndroidPurchasesCallback<Storefront>(cancellationToken);
                _pendingTasks[nameof(GetStorefrontAsync)] = callback;
                CallPurchases("getStorefront", callback);
                return callback.Task;
            }
        }

        public Task<IReadOnlyList<StoreProduct>> GetProductsAsync(
            string[] productIdentifiers,
            string type = "subs",
            CancellationToken cancellation = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetProductsAsync), out var value) &&
                    value is AndroidPurchasesCallback<IReadOnlyList<StoreProduct>> pending)
                {
                    return pending.Task;
                }
                var callback = new AndroidPurchasesCallback<IReadOnlyList<StoreProduct>>(cancellation);
                _pendingTasks[nameof(GetProductsAsync)] = callback;
                CallPurchases("getProducts", callback, JsonConvert.SerializeObject(new { productIdentifiers }), type);
                return callback.Task;
            }
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
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(PurchaseProductAsync), out var value) &&
                    value is AndroidPurchasesCallback<PurchaseResult> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<PurchaseResult>(cancellationToken);
                _pendingTasks[nameof(PurchaseProductAsync)] = callback;

                if (oldSku == null)
                {
                    CallPurchases(
                        "purchaseProduct",
                        callback,
                        productIdentifier,
                        type);
                }
                else
                {
                    CallPurchases(
                        "purchaseProduct",
                        callback,
                        productIdentifier,
                        type,
                        oldSku,
                        (int)prorationMode,
                        googleIsPersonalizedPrice,
                        JsonConvert.SerializeObject(new { offeringIdentifier }));
                }

                return callback.Task;
            }
        }

        public Task<PurchaseResult> PurchasePackageAsync(
            Package packageToPurchase,
            string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false,
            PromotionalOffer discount = null,
            CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(PurchasePackageAsync), out var value) &&
                    value is AndroidPurchasesCallback<PurchaseResult> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<PurchaseResult>(cancellationToken);
                _pendingTasks[nameof(PurchasePackageAsync)] = callback;

                if (oldSku == null)
                {
                    CallPurchases(
                        "purchasePackage",
                        callback,
                        packageToPurchase.Identifier,
                        JsonConvert.SerializeObject(packageToPurchase.PresentedOfferingContext));
                }
                else
                {
                    CallPurchases("purchasePackage",
                        callback,
                        packageToPurchase.Identifier,
                        JsonConvert.SerializeObject(packageToPurchase.PresentedOfferingContext),
                        oldSku,
                        (int)prorationMode,
                        googleIsPersonalizedPrice);
                }

                return callback.Task;
            }
        }


        public Task<PurchaseResult> PurchaseSubscriptionOptionAsync(
            SubscriptionOption subscriptionOption,
            GoogleProductChangeInfo googleProductChangeInfo = null,
            bool googleIsPersonalizedPrice = false,
            CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(PurchaseSubscriptionOptionAsync), out var value) &&
                    value is AndroidPurchasesCallback<PurchaseResult> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<PurchaseResult>(cancellationToken);
                _pendingTasks[nameof(PurchaseSubscriptionOptionAsync)] = callback;

                if (googleProductChangeInfo == null)
                {
                    CallPurchases("purchaseSubscriptionOption",
                        callback,
                        subscriptionOption.ProductId,
                        subscriptionOption.Id,
                        null,
                        0,
                        googleIsPersonalizedPrice,
                        JsonConvert.SerializeObject(subscriptionOption.PresentedOfferingContext));
                }
                else
                {
                    CallPurchases("purchaseSubscriptionOption",
                        callback,
                        subscriptionOption.ProductId,
                        subscriptionOption.Id,
                        googleProductChangeInfo.OldProductIdentifier,
                        (int)googleProductChangeInfo.ProrationMode,
                        googleIsPersonalizedPrice,
                        JsonConvert.SerializeObject(subscriptionOption.PresentedOfferingContext));
                }

                return callback.Task;
            }
        }

        public Task<CustomerInfo> RestorePurchasesAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(RestorePurchasesAsync), out var value) &&
                    value is AndroidPurchasesCallback<CustomerInfo> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<CustomerInfo>(cancellationToken);
                _pendingTasks[nameof(RestorePurchasesAsync)] = callback;
                CallPurchases("restorePurchases", callback);
                return callback.Task;
            }
        }

        public Task<LoginResult> LogInAsync(string appUserId, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(LogInAsync), out var value) &&
                    value is AndroidPurchasesCallback<LoginResult> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<LoginResult>(cancellationToken);
                _pendingTasks[nameof(LogInAsync)] = callback;
                CallPurchases("logIn", callback, appUserId);
                return callback.Task;
            }
        }

        public Task<CustomerInfo> LogOutAsync()
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(LogOutAsync), out var value) &&
                    value is AndroidPurchasesCallback<CustomerInfo> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<CustomerInfo>(CancellationToken.None);
                _pendingTasks[nameof(LogOutAsync)] = callback;
                CallPurchases("logOut", callback);
                return callback.Task;
            }
        }

        public void SetAllowSharingStoreAccount(bool allow)
        {
            CallPurchases("setAllowSharingStoreAccount", allow);
        }

        public Task<Offerings> GetOfferingsAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetOfferingsAsync), out var value) &&
                    value is AndroidPurchasesCallback<Offerings> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<Offerings>(cancellationToken);
                _pendingTasks[nameof(GetOfferingsAsync)] = callback;
                CallPurchases("getOfferings", callback);
                return callback.Task;
            }
        }

        public Task<Offering> GetCurrentOfferingForPlacementAsync(string placementIdentifier, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetCurrentOfferingForPlacementAsync), out var value) &&
                    value is AndroidPurchasesCallback<Offering> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<Offering>(cancellationToken);
                _pendingTasks[nameof(GetCurrentOfferingForPlacementAsync)] = callback;
                CallPurchases("getCurrentOfferingForPlacement", callback, placementIdentifier);
                return callback.Task;
            }
        }

        public Task<Offerings> SyncAttributesAndOfferingsIfNeededAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(SyncAttributesAndOfferingsIfNeededAsync), out var value) &&
                    value is AndroidPurchasesCallback<Offerings> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<Offerings>(cancellationToken);
                _pendingTasks[nameof(SyncAttributesAndOfferingsIfNeededAsync)] = callback;
                CallPurchases("syncAttributesAndOfferingsIfNeeded", callback);
                return callback.Task;
            }
        }

        public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID, string isoCurrencyCode, double price)
        {
            CallPurchases("syncAmazonPurchase", productID, receiptID, amazonUserID, isoCurrencyCode, price);
        }

        public Task<bool> GetAmazonLWAConsentStatusAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetAmazonLWAConsentStatusAsync), out var value) &&
                    value is AndroidPurchasesCallback<bool> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<bool>(cancellationToken);
                _pendingTasks[nameof(GetAmazonLWAConsentStatusAsync)] = callback;
                CallPurchases("getAmazonLWAConsentStatus", callback);
                return callback.Task;
            }
        }

        public void SetLogLevel(LogLevel level)
        {
            CallPurchases("setLogLevel", level.ToString());
        }

        public void SetDebugLogsEnabled(bool enabled)
        {
            CallPurchases("setDebugLogsEnabled", enabled);
        }

        public void SetProxyURL(string proxyURL)
        {
            CallPurchases("setProxyURL", proxyURL);
        }

        public string GetAppUserId()
        {
            try
            {
                return CallPurchases<string>("getAppUserID");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public Task<CustomerInfo> GetCustomerInfoAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetCustomerInfoAsync), out var value) &&
                    value is AndroidPurchasesCallback<CustomerInfo> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<CustomerInfo>(cancellationToken);
                _pendingTasks[nameof(GetCustomerInfoAsync)] = callback;
                CallPurchases("getCustomerInfo", callback);
                return callback.Task;
            }
        }

        public Task<CustomerInfo> SyncPurchasesAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(SyncPurchasesAsync), out var value) &&
                    value is AndroidPurchasesCallback<CustomerInfo> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<CustomerInfo>(cancellationToken);
                _pendingTasks[nameof(SyncPurchasesAsync)] = callback;
                CallPurchases("syncPurchases", callback);
                return callback.Task;
            }
        }

        public void EnableAdServicesAttributionTokenCollection()
        {
            // NOOP
        }

        public bool IsAnonymous()
        {
            try
            {
                return CallPurchases<bool>("isAnonymous");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public bool IsConfigured()
        {
            try
            {
                return CallPurchases<bool>("isConfigured");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public Task<IReadOnlyDictionary<string, IntroEligibility>> CheckTrialOrIntroductoryPriceEligibilityAsync(string[] productIdentifiers, CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<IReadOnlyDictionary<string, IntroEligibility>>(cancellationToken);
            CallPurchases("checkTrialOrIntroductoryPriceEligibility", callback, JsonConvert.SerializeObject(new { productIdentifiers }));
            return callback.Task;
        }

        public void InvalidateCustomerInfoCache()
        {
            CallPurchases("invalidateCustomerInfoCache");
        }

        public void PresentCodeRedemptionSheet()
        {
            // NOOP
        }

        public void RecordPurchase(string productID)
        {
            // NOOP
        }

        public void SetSimulatesAskToBuyInSandbox(bool enabled)
        {
            // NOOP
        }

        public void SetAttributes(Dictionary<string, string> attributes)
        {
            CallPurchases("setAttributes", JsonConvert.SerializeObject(attributes));
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

        public void CollectDeviceIdentifiers()
        {
            CallPurchases("collectDeviceIdentifiers");
        }

        public Task<bool> CanMakePaymentsAsync(BillingFeature[] features, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(CanMakePaymentsAsync), out var value) &&
                    value is AndroidPurchasesCallback<bool> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<bool>(cancellationToken);
                _pendingTasks[nameof(CanMakePaymentsAsync)] = callback;
                CallPurchases("canMakePayments", callback, JsonConvert.SerializeObject(new { features }));
                return callback.Task;
            }
        }

        public Task<PromotionalOffer> GetPromotionalOfferAsync(string productIdentifier, string discountIdentifier, CancellationToken cancellationToken = default)
        {
            // TODO verify NOOP ?
            // CallPurchases("getPromotionalOffer", productIdentifier, discountIdentifier);
            return Task.FromResult<PromotionalOffer>(null);
        }

        public void ShowInAppMessages(InAppMessageType[] messageTypes)
        {
            CallPurchases("showInAppMessages", JsonConvert.SerializeObject(new { messageTypes }));
        }

        public Task<WebPurchaseRedemption> ParseAsWebPurchaseRedemptionAsync(string urlString, CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<WebPurchaseRedemption>(cancellationToken);
            CallPurchases("parseAsWebPurchaseRedemption", callback, urlString);
            return callback.Task;
        }

        public Task<WebPurchaseRedemptionResult> RedeemWebPurchaseAsync(WebPurchaseRedemption webPurchaseRedemption, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(RedeemWebPurchaseAsync), out var value) &&
                    value is AndroidPurchasesCallback<WebPurchaseRedemptionResult> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<WebPurchaseRedemptionResult>(cancellationToken);
                _pendingTasks[nameof(RedeemWebPurchaseAsync)] = callback;
                CallPurchases("redeemWebPurchase", callback, webPurchaseRedemption.RedemptionLink);
                return callback.Task;
            }
        }

        public Task<VirtualCurrencies> GetVirtualCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetVirtualCurrenciesAsync), out var value) &&
                    value is AndroidPurchasesCallback<VirtualCurrencies> pending)
                {
                    return pending.Task;
                }

                var callback = new AndroidPurchasesCallback<VirtualCurrencies>(cancellationToken);
                _pendingTasks[nameof(GetVirtualCurrenciesAsync)] = callback;
                CallPurchases("getVirtualCurrencies", callback);
                return callback.Task;
            }
        }

        public VirtualCurrencies GetCachedVirtualCurrencies()
        {
            try
            {
                var json = CallPurchases<string>("getCachedVirtualCurrencies");
                return !string.IsNullOrWhiteSpace(json)
                    ? JsonConvert.DeserializeObject<VirtualCurrencies>(json)
                    : null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public void InvalidateVirtualCurrenciesCache()
        {
            CallPurchases("invalidateVirtualCurrenciesCache");
        }

        public Task<IReadOnlyList<WinBackOffer>> GetEligibleWinBackOffersForProductAsync(StoreProduct storeProduct, CancellationToken cancellationToken = default)
        {
            // NOOP
            return Task.FromResult<IReadOnlyList<WinBackOffer>>(null);
        }

        public Task<IReadOnlyList<WinBackOffer>> GetEligibleWinBackOffersForPackageAsync(Package package, CancellationToken cancellationToken = default)
        {
            // NOOP
            return Task.FromResult<IReadOnlyList<WinBackOffer>>(null);
        }

        public Task<PurchaseResult> PurchaseProductWithWinBackOfferAsync(StoreProduct storeProduct, WinBackOffer winBackOffer, CancellationToken cancellationToken = default)
        {
            // NOOP
            return Task.FromResult<PurchaseResult>(null);
        }

        public Task<PurchaseResult> PurchasePackageWithWinBackOfferAsync(Package package, WinBackOffer winBackOffer, CancellationToken cancellationToken = default)
        {
            // NOOP
            return Task.FromResult<PurchaseResult>(null);
        }

        private const string PurchasesJavaClass = "com.revenuecat.unity.Purchases";

        private static void CallPurchases(string methodName, params object[] args)
        {
            using (var purchases = new AndroidJavaClass(PurchasesJavaClass))
            {
                purchases.CallStatic(methodName, args);
            }
        }

        private static TReturnType CallPurchases<TReturnType>(string methodName, params object[] args)
        {
            using (var purchases = new AndroidJavaClass(PurchasesJavaClass))
            {
                return purchases.CallStatic<TReturnType>(methodName, args);
            }
        }
    }
}
#endif // UNITY_ANDROID && !UNITY_EDITOR

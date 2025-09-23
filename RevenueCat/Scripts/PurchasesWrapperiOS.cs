#if UNITY_IOS || UNITY_VISIONOS
using AOT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCat
{
    public class PurchasesWrapperiOS : IPurchasesWrapper
    {
        private PurchasesWrapperiOS()
        {
            if (_instance != null)
            {
                throw new Exception($"There should never be more than one instance of {nameof(PurchasesWrapperiOS)}");
            }

            _instance = this;
        }

        ~PurchasesWrapperiOS()
        {
            _instance = null;
            Dispose();
        }

        public void Dispose()
        {
            OnCustomerInfoUpdated = null;
            OnLogMessage = null;
        }

        private static PurchasesWrapperiOS _instance;
        private static readonly object _lock = new object();
        private static ConcurrentDictionary<string, object> _pendingTasks = new ConcurrentDictionary<string, object>();

        public event Action<CustomerInfo> OnCustomerInfoUpdated;
        public event Action<RevenueCatLogMessage> OnLogMessage;

        private delegate void StaticCallbackDelegate(IntPtr dataPtr);
        private delegate void NativeInteropDelegate(IntPtr methodPtr, IntPtr dataPtr);

        [MonoPInvokeCallback(typeof(StaticCallbackDelegate))]
        private static void CustomerInfoReceived(IntPtr dataPtr)
        {
            if (dataPtr == IntPtr.Zero) { return; }

            try
            {
                var json = Marshal.PtrToStringAuto(dataPtr) ?? string.Empty;
                var customerInfo = JsonConvert.DeserializeObject<CustomerInfo>(json);
                PurchasesSdk.MainThreadSynchronizationContext.Post(_ =>
                {
                    _instance.OnCustomerInfoUpdated?.Invoke(customerInfo);
                }, null);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        [MonoPInvokeCallback(typeof(StaticCallbackDelegate))]
        private static void LogReceived(IntPtr dataPtr)
        {
            if (_instance == null) { return; }
            try
            {
                var json = Marshal.PtrToStringAuto(dataPtr) ?? string.Empty;
                var logMessage = JsonConvert.DeserializeObject<RevenueCatLogMessage>(json);
                PurchasesSdk.MainThreadSynchronizationContext.Post(_ =>
                {
                    _instance.OnLogMessage?.Invoke(logMessage);
                }, null);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        [MonoPInvokeCallback(typeof(NativeInteropDelegate))]
        private static void NativeInteropCallback(IntPtr methodPtr, IntPtr dataPtr)
        {
            lock (_lock)
            {
                if (methodPtr == IntPtr.Zero) { return; }

                try
                {
                    var method = Marshal.PtrToStringAuto(methodPtr) ?? string.Empty;

                    if (_pendingTasks.TryRemove(method, out var value) && value is TaskCompletionSource<object> tcs)
                    {
                        var json = Marshal.PtrToStringAuto(dataPtr) ?? string.Empty;
                        var jObject = JObject.Parse(json);

                        if (jObject["error"] != null)
                        {
                            var error = jObject["error"].ToObject<Error>();
                            PurchasesSdk.MainThreadSynchronizationContext.Post(_ =>
                            {
                                tcs.SetException(new Exception(error.ToString()));
                            }, null);
                        }
                        else
                        {
                            var result = jObject["result"]?.ToObject(value.GetType().GetGenericArguments()[0]);
                            PurchasesSdk.MainThreadSynchronizationContext.Post(_ =>
                            {
                                tcs.SetResult(result);
                            }, null);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCSetupPurchases(
                StaticCallbackDelegate nativeInterop,
                StaticCallbackDelegate logHandler,
                NativeInteropDelegate nativeInteropHandler,
                string apiKey,
                string appUserId,
                string purchasesAreCompletedBy,
                string storeKitVersion,
                string userDefaultsSuiteName,
                string dangerousSettingsJson,
                bool shouldShowInAppMessagesAutomatically,
                string entitlementVerificationMode);

        public void Configure(PurchasesConfiguration configuration)
        {
            _RCSetupPurchases(
                CustomerInfoReceived,
                LogReceived,
                NativeInteropCallback,
                configuration.ApiKey,
                configuration.AppUserId,
                configuration.PurchasesAreCompletedBy.ToString(),
                configuration.StoreKitVersion.Name(),
                configuration.UserDefaultsSuiteName,
                JsonConvert.SerializeObject(configuration.DangerousSettings),
                configuration.ShouldShowInAppMessagesAutomatically,
                configuration.EntitlementVerificationMode.ToString());
        }


        [DllImport("__Internal")]
        private static extern void _RCGetStorefront();

        public Task<Storefront> GetStorefrontAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetStorefrontAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<Storefront> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<Storefront>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(GetStorefrontAsync)] = tcs;
                _RCGetStorefront();
                return tcs.Task;
            }
        }


        [DllImport("__Internal")]
        private static extern void _RCGetProducts(string productIdentifiersJson, string type);

        public Task<IReadOnlyList<StoreProduct>> GetProductsAsync(
            string[] productIdentifiers,
            string type = "subs",
            CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetProductsAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<IReadOnlyList<StoreProduct>> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<IReadOnlyList<StoreProduct>>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(GetProductsAsync)] = tcs;
                _RCGetProducts(JsonConvert.SerializeObject(new { productIdentifiers }), type);
                return tcs.Task;
            }
        }


        [DllImport("__Internal")]
        private static extern void _RCPurchaseProduct(string productIdentifier, string signedDiscountTimestamp);

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
                if (_pendingTasks.TryGetValue(nameof(PurchaseProductAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<PurchaseResult> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<PurchaseResult>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(PurchaseProductAsync)] = tcs;
                _RCPurchaseProduct(productIdentifier, discount?.TimestampUnixMilliseconds.ToString());
                return tcs.Task;
            }
        }


        [DllImport("__Internal")]
        private static extern void _RCPurchasePackage(string packageIdentifier, string presentedOfferingContextJSON, string signedDiscountTimestamp);

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
                if (_pendingTasks.TryGetValue(nameof(PurchasePackageAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<PurchaseResult> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<PurchaseResult>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(PurchasePackageAsync)] = tcs;
                _RCPurchasePackage(
                    packageToPurchase.Identifier,
                    JsonConvert.SerializeObject(packageToPurchase.PresentedOfferingContext),
                    discount?.TimestampUnixMilliseconds.ToString());
                return tcs.Task;
            }
        }

        public Task<PurchaseResult> PurchaseSubscriptionOptionAsync(
            SubscriptionOption subscriptionOption,
            GoogleProductChangeInfo googleProductChangeInfo = null,
            bool googleIsPersonalizedPrice = false,
            CancellationToken cancellationToken = default)
        {
            // NOOP
            return Task.FromResult<PurchaseResult>(null);
        }

        [DllImport("__Internal")]
        private static extern void _RCRestorePurchases();

        public Task<CustomerInfo> RestorePurchasesAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(RestorePurchasesAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<CustomerInfo> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<CustomerInfo>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(RestorePurchasesAsync)] = tcs;
                _RCRestorePurchases();
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCLogIn(string appUserId);

        public Task<LoginResult> LogInAsync(string appUserId, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(LogInAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<LoginResult> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<LoginResult>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(LogInAsync)] = tcs;
                _RCLogIn(appUserId);
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCLogOut();

        public Task<CustomerInfo> LogOutAsync()
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(LogOutAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<CustomerInfo> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<CustomerInfo>();
                _pendingTasks[nameof(LogOutAsync)] = tcs;
                _RCLogOut();
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCSyncAttributesAndOfferingsIfNeeded();

        public Task<Offerings> SyncAttributesAndOfferingsIfNeededAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(SyncAttributesAndOfferingsIfNeededAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<Offerings> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<Offerings>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(SyncAttributesAndOfferingsIfNeededAsync)] = tcs;
                _RCSyncAttributesAndOfferingsIfNeeded();
                return tcs.Task;
            }
        }

        public void SyncAmazonPurchase(
            string productID,
            string receiptID,
            string amazonUserID,
            string isoCurrencyCode,
            double price)
        {
            // No-Op
        }

        public Task<bool> GetAmazonLWAConsentStatusAsync(CancellationToken cancellationToken = default)
        {
            // No-Op
            return Task.FromResult(false);
        }

        [DllImport("__Internal")]
        private static extern void _RCSetAllowSharingStoreAccount(bool allow);

        public void SetAllowSharingStoreAccount(bool allow)
        {
            _RCSetAllowSharingStoreAccount(allow);
        }

        [DllImport("__Internal")]
        private static extern void _RCGetOfferings();

        public Task<Offerings> GetOfferingsAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetOfferingsAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<Offerings> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<Offerings>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(GetOfferingsAsync)] = tcs;
                _RCGetOfferings();
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCGetCurrentOfferingForPlacement(string placementIdentifier);

        public Task<Offering> GetCurrentOfferingForPlacementAsync(string placementIdentifier, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetCurrentOfferingForPlacementAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<Offering> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<Offering>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(GetCurrentOfferingForPlacementAsync)] = tcs;
                _RCGetCurrentOfferingForPlacement(placementIdentifier);
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCSetDebugLogsEnabled(bool enabled);

        public void SetDebugLogsEnabled(bool enabled)
        {
            _RCSetDebugLogsEnabled(enabled);
        }

        [DllImport("__Internal")]
        private static extern void _RCSetLogLevel(string level);

        public void SetLogLevel(LogLevel level)
        {
            _RCSetLogLevel(level.ToString());
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
            try
            {
                return _RCGetAppUserID();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCGetCustomerInfo();

        public Task<CustomerInfo> GetCustomerInfoAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetCustomerInfoAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<CustomerInfo> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<CustomerInfo>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(GetCustomerInfoAsync)] = tcs;
                _RCGetCustomerInfo();
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCSyncPurchases();

        public Task<CustomerInfo> SyncPurchasesAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(SyncPurchasesAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<CustomerInfo> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<CustomerInfo>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(SyncPurchasesAsync)] = tcs;
                _RCSyncPurchases();
                return tcs.Task;
            }
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
            try
            {
                return _RCIsAnonymous();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        [DllImport("__Internal")]
        private static extern bool _RCIsConfigured();

        public bool IsConfigured()
        {
            try
            {
                return _RCIsConfigured();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCCheckTrialOrIntroductoryPriceEligibility(string productIdentifiersJson);

        public Task<IReadOnlyDictionary<string, IntroEligibility>> CheckTrialOrIntroductoryPriceEligibilityAsync(
            string[] productIdentifiers,
            CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(CheckTrialOrIntroductoryPriceEligibilityAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<IReadOnlyDictionary<string, IntroEligibility>> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<IReadOnlyDictionary<string, IntroEligibility>>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(CheckTrialOrIntroductoryPriceEligibilityAsync)] = tcs;
                _RCCheckTrialOrIntroductoryPriceEligibility(JsonConvert.SerializeObject(new { productIdentifiers }));
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCInvalidateCustomerInfoCache();

        public void InvalidateCustomerInfoCache()
        {
            _RCInvalidateCustomerInfoCache();
        }

        [DllImport("__Internal")]
        private static extern void _RCPresentCodeRedemptionSheet();

        public void PresentCodeRedemptionSheet()
        {
            _RCPresentCodeRedemptionSheet();
        }

        [DllImport("__Internal")]
        private static extern void _RCRecordPurchase(string productID);

        public void RecordPurchase(string productID)
        {
            _RCRecordPurchase(productID);
        }

        [DllImport("__Internal")]
        private static extern void _RCSetSimulatesAskToBuyInSandbox(bool enabled);

        public void SetSimulatesAskToBuyInSandbox(bool enabled)
        {
            _RCSetSimulatesAskToBuyInSandbox(enabled);
        }

        public void SetAttributes(Dictionary<string, string> attributes)
        {
            _RCSetAttributes(JsonConvert.SerializeObject(attributes));
        }

        [DllImport("__Internal")]
        private static extern void _RCSetAttributes(string attributesJson);

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
        private static extern void _RCCollectDeviceIdentifiers();

        public void CollectDeviceIdentifiers()
        {
            _RCCollectDeviceIdentifiers();
        }

        [DllImport("__Internal")]
        private static extern void _RCCanMakePayments(string featuresJson);

        public Task<bool> CanMakePaymentsAsync(BillingFeature[] features, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(CanMakePaymentsAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<bool> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<bool>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(CanMakePaymentsAsync)] = tcs;
                _RCCanMakePayments(JsonConvert.SerializeObject(new { features }));
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCGetPromotionalOffer(string productIdentifier, string discountIdentifier);

        public Task<PromotionalOffer> GetPromotionalOfferAsync(string productIdentifier, string discountIdentifier, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetPromotionalOfferAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<PromotionalOffer> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<PromotionalOffer>();
                _pendingTasks[nameof(GetPromotionalOfferAsync)] = tcs;
                _RCGetPromotionalOffer(productIdentifier, discountIdentifier);
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCShowInAppMessages(string messagesJson);

        public void ShowInAppMessages(InAppMessageType[] messageTypes)
        {
            _RCShowInAppMessages(JsonConvert.SerializeObject(messageTypes));
        }

        [DllImport("__Internal")]
        private static extern void _RCParseAsWebPurchaseRedemption(string urlString);

        public Task<WebPurchaseRedemption> ParseAsWebPurchaseRedemptionAsync(string urlString, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(ParseAsWebPurchaseRedemptionAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<WebPurchaseRedemption> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<WebPurchaseRedemption>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(ParseAsWebPurchaseRedemptionAsync)] = tcs;
                _RCParseAsWebPurchaseRedemption(urlString);
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCRedeemWebPurchase(string resultJson);

        public Task<WebPurchaseRedemptionResult> RedeemWebPurchaseAsync(WebPurchaseRedemption webPurchaseRedemption, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(RedeemWebPurchaseAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<WebPurchaseRedemptionResult> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<WebPurchaseRedemptionResult>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(RedeemWebPurchaseAsync)] = tcs;
                _RCRedeemWebPurchase(webPurchaseRedemption.RedemptionLink);
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCGetVirtualCurrencies();

        public Task<VirtualCurrencies> GetVirtualCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetVirtualCurrenciesAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<VirtualCurrencies> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<VirtualCurrencies>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(GetVirtualCurrenciesAsync)] = tcs;
                _RCGetVirtualCurrencies();
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern string _RCGetCachedVirtualCurrencies();

        public VirtualCurrencies GetCachedVirtualCurrencies()
        {
            try
            {
                var json = _RCGetCachedVirtualCurrencies();
                return !string.IsNullOrWhiteSpace(json)
                    ? JsonConvert.DeserializeObject<VirtualCurrencies>(json)
                    : null;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCInvalidateVirtualCurrenciesCache();

        public void InvalidateVirtualCurrenciesCache()
        {
            _RCInvalidateVirtualCurrenciesCache();
        }

        [DllImport("__Internal")]
        private static extern void _RCGetEligibleWinBackOffersForProduct(string productIdentifier);

        public Task<IReadOnlyList<WinBackOffer>> GetEligibleWinBackOffersForProductAsync(StoreProduct storeProduct, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetEligibleWinBackOffersForProductAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<IReadOnlyList<WinBackOffer>> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<IReadOnlyList<WinBackOffer>>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(GetEligibleWinBackOffersForProductAsync)] = tcs;
                _RCGetEligibleWinBackOffersForProduct(storeProduct.Identifier);
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCGetEligibleWinBackOffersForPackage(string productIdentifier);

        public Task<IReadOnlyList<WinBackOffer>> GetEligibleWinBackOffersForPackageAsync(Package package, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(GetEligibleWinBackOffersForPackageAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<IReadOnlyList<WinBackOffer>> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<IReadOnlyList<WinBackOffer>>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(GetEligibleWinBackOffersForPackageAsync)] = tcs;
                _RCGetEligibleWinBackOffersForPackage(package.Identifier);
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCPurchaseProductWithWinBackOffer(string productIdentifier, string winBackOfferIdentifier);

        public Task<PurchaseResult> PurchaseProductWithWinBackOfferAsync(StoreProduct storeProduct, WinBackOffer winBackOffer, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(PurchaseProductWithWinBackOfferAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<PurchaseResult> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<PurchaseResult>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(PurchaseProductWithWinBackOfferAsync)] = tcs;
                _RCPurchaseProductWithWinBackOffer(storeProduct.Identifier, winBackOffer.Identifier);
                return tcs.Task;
            }
        }

        [DllImport("__Internal")]
        private static extern void _RCPurchasePackageWithWinBackOffer(
            string packageIdentifier,
            string presentedOfferingContextJson,
            string winBackOfferIdentifier);

        public Task<PurchaseResult> PurchasePackageWithWinBackOfferAsync(Package package, WinBackOffer winBackOffer, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_pendingTasks.TryGetValue(nameof(PurchasePackageWithWinBackOfferAsync), out var pendingTcs) &&
                    pendingTcs is TaskCompletionSource<PurchaseResult> pending)
                {
                    return pending.Task;
                }
                var tcs = new TaskCompletionSource<PurchaseResult>();
                cancellationToken.Register(() => tcs.SetCanceled());
                _pendingTasks[nameof(PurchasePackageWithWinBackOfferAsync)] = tcs;
                _RCPurchasePackageWithWinBackOffer(
                    package.Identifier,
                    JsonConvert.SerializeObject(package.PresentedOfferingContext),
                    winBackOffer.Identifier);
                return tcs.Task;
            }
        }
    }
}
#endif // UNITY_IOS || UNITY_VISIONOS

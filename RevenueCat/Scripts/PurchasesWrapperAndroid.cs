#if UNITY_ANDROID && !UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCat
{
    public class PurchasesWrapperAndroid : IPurchasesWrapper
    {
        private LogHandler _logHandler;
        private CustomerInfoHandler _customerInfoHandler;

        public event Action<CustomerInfo> OnCustomerInfoUpdated;
        public event Action<RevenueCatLogMessage> OnLogMessage;

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
            var callback = new AndroidPurchasesCallback<Storefront>(cancellationToken);
            CallPurchases("getStorefront", callback);
            return callback.Task;
        }

        public Task<IReadOnlyList<StoreProduct>> GetProductsAsync(
            string[] productIdentifiers,
            string type = "subs",
            CancellationToken cancellation = default)
        {
            var callback = new AndroidPurchasesCallback<IReadOnlyList<StoreProduct>>(cancellation);
            CallPurchases("getProducts", callback, JsonConvert.SerializeObject(new { productIdentifiers }), type);
            return callback.Task;
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
            var callback = new AndroidPurchasesCallback<PurchaseResult>(cancellationToken);

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

        public Task<PurchaseResult> PurchasePackageAsync(
            Package packageToPurchase,
            string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false,
            PromotionalOffer discount = null,
            CancellationToken cnCancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<PurchaseResult>();
            var presentedOfferingContextJSON = JsonConvert.SerializeObject(packageToPurchase.PresentedOfferingContext);

            if (oldSku == null)
            {
                CallPurchases(
                    "purchasePackage",
                    callback,
                    packageToPurchase.Identifier,
                    presentedOfferingContextJSON);
            }
            else
            {
                CallPurchases("purchasePackage",
                    callback,
                    packageToPurchase.Identifier,
                    presentedOfferingContextJSON,
                    oldSku,
                    (int)prorationMode,
                    googleIsPersonalizedPrice);
            }

            return callback.Task;
        }


        public Task<PurchaseResult> PurchaseSubscriptionOptionAsync(
            SubscriptionOption subscriptionOption,
            GoogleProductChangeInfo googleProductChangeInfo = null,
            bool googleIsPersonalizedPrice = false,
            CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<PurchaseResult>(cancellationToken);
            var presentedOfferingContextJSON = JsonConvert.SerializeObject(subscriptionOption.PresentedOfferingContext);

            if (googleProductChangeInfo == null)
            {
                CallPurchases("purchaseSubscriptionOption",
                    callback,
                    subscriptionOption.ProductId,
                    subscriptionOption.Id,
                    null,
                    0,
                    googleIsPersonalizedPrice,
                    presentedOfferingContextJSON);
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
                    presentedOfferingContextJSON);
            }

            return callback.Task;
        }


        public void RestorePurchases()
        {
            CallPurchases("restorePurchases");
        }

        public Task<CustomerInfo> LogInAsync(string appUserId, CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<CustomerInfo>(cancellationToken);
            CallPurchases("logIn", appUserId);
            return callback.Task;
        }

        public void LogOut()
        {
            CallPurchases("logOut");
        }

        public void SetAllowSharingStoreAccount(bool allow)
        {
            CallPurchases("setAllowSharingStoreAccount", allow);
        }

        public Task<Offerings> GetOfferingsAsync(CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<Offerings>();
            CallPurchases("getOfferings", callback);
            return callback.Task;
        }

        public Task<Offering> GetCurrentOfferingForPlacementAsync(string placementIdentifier, CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<Offering>(cancellationToken);
            CallPurchases("getCurrentOfferingForPlacement", callback, placementIdentifier);
            return callback.Task;
        }

        public Task<Offerings> SyncAttributesAndOfferingsIfNeededAsync(CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<Offerings>(cancellationToken);
            CallPurchases("syncAttributesAndOfferingsIfNeeded");
            return callback.Task;
        }

        public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID, string isoCurrencyCode, double price)
        {
            CallPurchases("syncAmazonPurchase", productID, receiptID, amazonUserID, isoCurrencyCode, price);
        }

        public Task<bool> GetAmazonLWAConsentStatusAsync(CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<bool>(cancellationToken);
            CallPurchases("getAmazonLWAConsentStatus", callback);
            return callback.Task;
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
            return CallPurchases<string>("getAppUserID");
        }

        public Task<CustomerInfo> GetCustomerInfoAsync(CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<CustomerInfo>(cancellationToken);
            CallPurchases("getCustomerInfo");
            return callback.Task;
        }

        public void SyncPurchases()
        {
            CallPurchases("syncPurchases");
        }

        public void EnableAdServicesAttributionTokenCollection()
        {
            // NOOP
        }

        public bool IsAnonymous()
        {
            return CallPurchases<bool>("isAnonymous");
        }

        public bool IsConfigured()
        {
            return CallPurchases<bool>("isConfigured");
        }

        public Task<IReadOnlyDictionary<string, IntroEligibility>> CheckTrialOrIntroductoryPriceEligibilityAsync(string[] productIdentifiers, CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<IReadOnlyDictionary<string, IntroEligibility>>();
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

        public void SetAttributes(string attributesJson)
        {
            CallPurchases("setAttributes", attributesJson);
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
            var callback = new AndroidPurchasesCallback<bool>(cancellationToken);
            CallPurchases("canMakePayments", callback, JsonConvert.SerializeObject(new { features }));
            return callback.Task;
        }

        public Task<PromotionalOffer> GetPromotionalOffer(string productIdentifier, string discountIdentifier)
        {
            // TODO verify NOOP ?
            // CallPurchases("getPromotionalOffer", productIdentifier, discountIdentifier);
            return null;
        }

        public void ShowInAppMessages(InAppMessageType[] messageTypes)
        {
            CallPurchases("showInAppMessages", JsonConvert.SerializeObject(new { messageTypes }));
        }

        public Task<WebPurchaseRedemption> ParseAsWebPurchaseRedemptionAsync(string urlString, CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<WebPurchaseRedemption>(cancellationToken);
            CallPurchases("parseAsWebPurchaseRedemption", urlString);
            return callback.Task;
        }

        public Task<WebPurchaseRedemptionResult> RedeemWebPurchaseAsync(WebPurchaseRedemption webPurchaseRedemption, CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<WebPurchaseRedemptionResult>();
            CallPurchases("redeemWebPurchase", webPurchaseRedemption.RedemptionLink);
            return callback.Task;
        }

        public Task<VirtualCurrencies> GetVirtualCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            var callback = new AndroidPurchasesCallback<VirtualCurrencies>(cancellationToken);
            CallPurchases("getVirtualCurrencies");
            return callback.Task;
        }

        public VirtualCurrencies GetCachedVirtualCurrencies()
        {
            var json = CallPurchases<string>("getCachedVirtualCurrencies");
            return JsonConvert.DeserializeObject<VirtualCurrencies>(json);
        }

        public void InvalidateVirtualCurrenciesCache()
        {
            CallPurchases("invalidateVirtualCurrenciesCache");
        }

        public void GetEligibleWinBackOffersForProduct(StoreProduct storeProduct)
        {
            // NOOP
        }

        public void GetEligibleWinBackOffersForPackage(Package package)
        {
            // NOOP
        }

        public void PurchaseProductWithWinBackOffer(StoreProduct storeProduct, WinBackOffer winBackOffer)
        {
            // NOOP
        }

        public void PurchasePackageWithWinBackOffer(Package package, WinBackOffer winBackOffer)
        {
            // NOOP
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

        public void Dispose()
        {
        }
    }
}
#endif // UNITY_ANDROID && !UNITY_EDITOR

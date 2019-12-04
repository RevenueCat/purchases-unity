using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

#pragma warning disable CS0649

public class Purchases : MonoBehaviour
{

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum AttributionNetwork
    {
        APPLE_SEARCH_ADS = 0,
        ADJUST = 1,
        APPSFLYER = 2,
        BRANCH = 3,
        TENJIN = 4,
        FACEBOOK = 5
    }

    public enum ProrationMode
    {
        UnknownSubscriptionUpgradeDowngradePolicy = 0,

        /// Replacement takes effect immediately, and the remaining time will be
        /// prorated and credited to the user. This is the current default behavior.
        ImmediateWithTimeProration = 1,

        /// Replacement takes effect immediately, and the billing cycle remains the
        /// same. The price for the remaining period will be charged. This option is
        /// only available for subscription upgrade.
        ImmediateAndChargeProratedPrice = 2,

        /// Replacement takes effect immediately, and the new price will be charged on
        /// next recurrence time. The billing cycle stays the same.
        ImmediateWithoutProration = 3,

        /// Replacement takes effect when the old plan expires, and the new price will
        /// be charged at the same time.
        Deferred = 4
    }

    public delegate void GetProductsFunc(List<Product> products, Error error);

    public delegate void MakePurchaseFunc(string productIdentifier, PurchaserInfo purchaserInfo, bool userCancelled, Error error);

    public delegate void PurchaserInfoFunc(PurchaserInfo purchaserInfo, Error error);

    public delegate void GetEntitlementsFunc(Dictionary<string, object> entitlements, Error error);

    public delegate void GetOfferingsFunc(Offerings offerings, Error error);

    public abstract class UpdatedPurchaserInfoListener : MonoBehaviour
    {
        public abstract void PurchaserInfoReceived(PurchaserInfo purchaserInfo);
    }

    private class PurchasesWrapperNoop : IPurchasesWrapper
    {
        public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode)
        {

        }

        public void AddAttributionData(int network, string data, string networkUserId)
        {

        }

        public void GetProducts(string[] productIdentifiers, string type = "subs")
        {

        }

        public void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null)
        {

        }

        public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null, ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
        {

        }

        public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null, ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
        {

        }

        public void RestoreTransactions()
        {

        }

        public void CreateAlias(string newAppUserId)
        {

        }

        public void Identify(string appUserId)
        {

        }

        public void Reset()
        {

        }

        public void SetFinishTransactions(bool finishTransactions)
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

        public void GetPurchaserInfo()
        {

        }

        public void GetOfferings()
        {

        }

        public void SyncPurchases()
        {

        }

        public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
        {

        }


        public bool IsAnonymous()
        {
            return false;
        }
    }

    /*
     * PurchaserInfo encapsulate the current status of subscriber. 
     * Use it to determine which entitlements to unlock, typically by checking 
     * ActiveSubscriptions or via LatestExpirationDate. 
     * 
     * Note: All DateTimes are in UTC, be sure to compare them with 
     * DateTime.UtcNow
     */
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class PurchaserInfo
    {
        private readonly PurchaserInfoResponse _response;

        public PurchaserInfo(PurchaserInfoResponse response)
        {
            _response = response;
        }

        public EntitlementInfos Entitlements
        {
            get { return new EntitlementInfos(_response.entitlements); }
        }

        public List<string> ActiveSubscriptions
        {
            get { return _response.activeSubscriptions; }
        }

        public List<string> AllPurchasedProductIdentifiers
        {
            get { return _response.allPurchasedProductIdentifiers; }
        }

        public DateTime? LatestExpirationDate
        {
            get
            {
                if (_response.latestExpirationDate != 0L)
                {
                    return FromUnixTime(_response.latestExpirationDate);
                }
                else
                {
                    return null;
                }
            }
        }

        public DateTime FirstSeen
        {
            get { return FromUnixTime(_response.firstSeen); }
        }

        public string OriginalAppUserId
        {
            get { return _response.originalAppUserId; }
        }

        public DateTime RequestDate
        {
            get { return FromUnixTime(_response.requestDate); }
        }

        public Dictionary<string, DateTime?> AllExpirationDates
        {
            get
            {
                var allExpirations = new Dictionary<string, DateTime?>();
                for (var i = 0; i < _response.allExpirationDateKeys.Count; i++)
                {
                    if (_response.allExpirationDateValues[i] != 0L)
                    {
                        var date = FromUnixTime(_response.allExpirationDateValues[i]);
                        allExpirations[_response.allExpirationDateKeys[i]] = date;
                    }
                    else
                    {
                        allExpirations[_response.allExpirationDateKeys[i]] = null;
                    }
                }

                return allExpirations;
            }
        }

        public Dictionary<string, DateTime> AllPurchaseDates
        {
            get
            {
                var allPurchases = new Dictionary<string, DateTime>();
                for (var i = 0; i < _response.allPurchaseDateKeys.Count; i++)
                {
                    var date = FromUnixTime(_response.allPurchaseDateValues[i]);
                    allPurchases[_response.allPurchaseDateKeys[i]] = date;
                }

                return allPurchases;
            }
        }

        public string OriginalApplicationVersion
        {
            get { return _response.originalApplicationVersion; }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    }


    public class EntitlementInfo
    {
        public readonly string Identifier;
        public readonly bool IsActive;
        public readonly bool WillRenew;
        public readonly string PeriodType;
        public readonly DateTime LatestPurchaseDate;
        public readonly DateTime OriginalPurchaseDate;
        public readonly DateTime? ExpirationDate;
        public readonly string Store;
        public readonly string ProductIdentifier;
        public readonly bool IsSandbox;
        [CanBeNull] public readonly DateTime? UnsubscribeDetectedAt;
        [CanBeNull] public readonly DateTime? BillingIssueDetectedAt;

        public EntitlementInfo(EntitlementInfoResponse response)
        {
            Identifier = response.identifier;
            IsActive = response.isActive;
            WillRenew = response.willRenew;
            PeriodType = response.periodType;
            LatestPurchaseDate = FromUnixTime(response.latestPurchaseDate);
            OriginalPurchaseDate = FromUnixTime(response.originalPurchaseDate);
            if (response.expirationDate != 0L)
            {
                ExpirationDate = FromUnixTime(response.expirationDate);
            }
            Store = response.store;
            ProductIdentifier = response.productIdentifier;
            IsSandbox = response.isSandbox;
            if (response.unsubscribeDetectedAt != 0L)
            {
                UnsubscribeDetectedAt = FromUnixTime(response.unsubscribeDetectedAt);
            }
            if (response.billingIssueDetectedAt != 0L)
            {
                BillingIssueDetectedAt = FromUnixTime(response.billingIssueDetectedAt);
            }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    }

    public class EntitlementInfos
    {
        public readonly Dictionary<string, EntitlementInfo> All;
        public readonly Dictionary<string, EntitlementInfo> Active;

        public EntitlementInfos(EntitlementInfosResponse response)
        {
            All = new Dictionary<string, EntitlementInfo>();
            for (var i = 0; i < response.allKeys.Count; i++)
            {
                All[response.allKeys[i]] = new EntitlementInfo(response.allValues[i]);
            }
            Active = new Dictionary<string, EntitlementInfo>();
            for (var i = 0; i < response.activeKeys.Count; i++)
            {
                Active[response.activeKeys[i]] = new EntitlementInfo(response.activeValues[i]);
            }
        }

    }

    public class Offerings
    {
        public readonly Dictionary<string, Offering> All;
        public readonly Offering Current;

        public Offerings(OfferingsResponse response)
        {
            All = new Dictionary<string, Offering>();
            for (var i = 0; i < response.allKeys.Count; i++)
            {
                All[response.allKeys[i]] = new Offering(response.allValues[i]);
            }
            Current = new Offering(response.current);
        }

    }

    public class Package
    {
        public readonly string Identifier;
        public readonly string PackageType;
        public readonly Product Product;
        public readonly string OfferingIdentifier;

        public Package(PackageResponse response)
        {
            Identifier = response.identifier;
            PackageType = response.packageType;
            Product = response.product;
            OfferingIdentifier = response.offeringIdentifier;
        }
    }

    public class Offering
    {
        public string Identifier;
        public string ServerDescription;
        public List<Package> AvailablePackages;
        public Package Lifetime;
        public Package Annual;
        public Package SixMonth;
        public Package ThreeMonth;
        public Package TwoMonth;
        public Package Monthly;
        public Package Weekly;

        public Offering(OfferingResponse response)
        {
            Identifier = response.identifier;
            ServerDescription = response.serverDescription;
            AvailablePackages = new List<Package>();
            foreach (var packageResponse in response.availablePackages)
            {
                AvailablePackages.Add(new Package(packageResponse));
            }
            Lifetime = new Package(response.lifetime);
            Annual = new Package(response.annual);
            SixMonth = new Package(response.sixMonth);
            ThreeMonth = new Package(response.threeMonth);
            TwoMonth = new Package(response.twoMonth);
            Monthly = new Package(response.monthly);
            Weekly = new Package(response.weekly);
        }
    }

    [Tooltip("Your RevenueCat API Key. Get from https://app.revenuecat.com/")]
    // ReSharper disable once InconsistentNaming
    public string revenueCatAPIKey;

    [Tooltip(
        "App user id. Pass in your own ID if your app has accounts. If blank, RevenueCat will generate a user ID for you.")]
    // ReSharper disable once InconsistentNaming
    public string appUserID;

    [Tooltip("List of product identifiers.")]
    public string[] productIdentifiers;

    [Tooltip("A subclass of Purchases.UpdatedPurchaserInfoListener component. Use your custom subclass to define how to handle updated purchaser information.")]
    public UpdatedPurchaserInfoListener listener;

    [Tooltip("An optional boolean. Set this to TRUE if you have your own IAP implementation and want to use only RevenueCat's backend. Default is FALSE.")]
    public bool observerMode;

    private IPurchasesWrapper _wrapper;

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _wrapper = new PurchasesWrapperAndroid();
#elif UNITY_IPHONE && !UNITY_EDITOR
        _wrapper = new PurchasesWrapperiOS();
#else
        _wrapper = new PurchasesWrapperNoop();
#endif
        Setup(string.IsNullOrEmpty(appUserID) ? null : appUserID);
        GetProducts(productIdentifiers, null);
    }

    // Call this if you want to reset with a new user id
    private void Setup(string newUserId)
    {
        _wrapper.Setup(gameObject.name, revenueCatAPIKey, newUserId, observerMode);
    }

    private GetProductsFunc ProductsCallback { get; set; }

    // Optionally call this if you want to fetch more products,
    // called automatically with pre-configured products
    // ReSharper disable once MemberCanBePrivate.Global
    public void GetProducts(string[] products, GetProductsFunc callback)
    {
        ProductsCallback = callback;
        _wrapper.GetProducts(products);
    }

    private MakePurchaseFunc MakePurchaseCallback { get; set; }

    // Call this to initiate a purchase
    [ObsoleteAttribute("This method will soon be deprecated. Use PurchaseProduct instead.")]
    public void MakePurchase(string productIdentifier, MakePurchaseFunc callback,
        string type = "subs", string oldSku = null)
    {
        _wrapper.PurchaseProduct(productIdentifier, type, oldSku);
    }

    public void PurchaseProduct(string productIdentifier, MakePurchaseFunc callback,
        string type = "subs", string oldSku = null, ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchaseProduct(productIdentifier, type, oldSku, prorationMode);
    }

    public void PurchasePackage(Package package, MakePurchaseFunc callback, string oldSku = null, ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchasePackage(package, oldSku, prorationMode);
    }

    private PurchaserInfoFunc RestoreTransactionsCallback { get; set; }

    public void RestoreTransactions(PurchaserInfoFunc callback)
    {
        RestoreTransactionsCallback = callback;
        _wrapper.RestoreTransactions();
    }

    public void AddAttributionData(string dataJson, AttributionNetwork network, string networkUserId = null)
    {
        _wrapper.AddAttributionData((int)network, dataJson, networkUserId);
    }

    private PurchaserInfoFunc CreateAliasCallback { get; set; }

    // ReSharper disable once UnusedMember.Global
    public void CreateAlias(string newAppUserId, PurchaserInfoFunc callback)
    {
        CreateAliasCallback = callback;
        _wrapper.CreateAlias(newAppUserId);
    }

    private PurchaserInfoFunc IdentifyCallback { get; set; }

    public void Identify(string appUserId, PurchaserInfoFunc callback)
    {
        IdentifyCallback = callback;
        _wrapper.Identify(appUserId);
    }

    private PurchaserInfoFunc ResetCallback { get; set; }

    // ReSharper disable once Unity.IncorrectMethodSignature
    // ReSharper disable once UnusedMember.Global
    public void Reset(PurchaserInfoFunc callback)
    {
        ResetCallback = callback;
        _wrapper.Reset();
    }

    // ReSharper disable once UnusedMember.Global
    public void SetFinishTransactions(bool finishTransactions)
    {
        _wrapper.SetFinishTransactions(finishTransactions);
    }

    // ReSharper disable once UnusedMember.Global
    public void SetAllowSharingStoreAccount(bool allow)
    {
        _wrapper.SetAllowSharingStoreAccount(allow);
    }

    // ReSharper disable once UnusedMember.Global
    public string GetAppUserId()
    {
        return _wrapper.GetAppUserId();
    }

    // ReSharper disable once UnusedMember.Global
    public bool IsAnonymous()
    {
        return _wrapper.IsAnonymous();
    }

    // ReSharper disable once UnusedMember.Global
    public void SetDebugLogsEnabled(bool logsEnabled)
    {
        _wrapper.SetDebugLogsEnabled(logsEnabled);
    }

    private PurchaserInfoFunc GetPurchaserInfoCallback { get; set; }

    // ReSharper disable once UnusedMember.Global
    public void GetPurchaserInfo(PurchaserInfoFunc callback)
    {
        GetPurchaserInfoCallback = callback;
        _wrapper.GetPurchaserInfo();
    }

    private GetEntitlementsFunc GetEntitlementsCallback { get; set; }

    [ObsoleteAttribute("This method has been replaced with GetOfferings.")]
    public void GetEntitlements(GetEntitlementsFunc callback)
    {

    }

    private GetOfferingsFunc GetOfferingsCallback { get; set; }

    public void GetOfferings(GetOfferingsFunc callback)
    {
        GetOfferingsCallback = callback;
        _wrapper.GetOfferings();
    }

    public void SyncPurchases()
    {
        _wrapper.SyncPurchases();
    }

    // ReSharper disable once UnusedMember.Global
    public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
    {
        _wrapper.SetAutomaticAppleSearchAdsAttributionCollection(enabled);
    }

    // ReSharper disable once UnusedMember.Local
    private void _receiveProducts(string productsJson)
    {
        Debug.Log("_receiveProducts " + productsJson);

        if (ProductsCallback == null) return;

        var response = JsonUtility.FromJson<ReceiveProductsResponse>(productsJson);
        var error = response.error.message != null ? response.error : null;

        if (error != null)
        {
            ProductsCallback(null, error);
        }
        else
        {
            ProductsCallback(response.products, null);
        }
        ProductsCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _getPurchaserInfo(string purchaserInfoJson)
    {
        Debug.Log("_getPurchaserInfo " + purchaserInfoJson);
        ReceivePurchaserInfoMethod(purchaserInfoJson, GetPurchaserInfoCallback);
        GetPurchaserInfoCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _makePurchase(string makePurchaseResponseJson)
    {
        Debug.Log("_makePurchase " + makePurchaseResponseJson);

        if (MakePurchaseCallback == null) return;

        var response = JsonUtility.FromJson<MakePurchaseResponse>(makePurchaseResponseJson);

        var error = response.error.message != null ? response.error : null;
        var info = response.purchaserInfo.activeSubscriptions != null
            ? new PurchaserInfo(response.purchaserInfo)
            : null;

        if (error != null)
        {
            MakePurchaseCallback(null, null, response.userCancelled, error);
        }
        else
        {
            MakePurchaseCallback(response.productIdentifier, info, false, null);
        }
        MakePurchaseCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _createAlias(string purchaserInfoJson)
    {
        Debug.Log("_createAlias " + purchaserInfoJson);
        ReceivePurchaserInfoMethod(purchaserInfoJson, CreateAliasCallback);
        CreateAliasCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _receivePurchaserInfo(string purchaserInfoJson)
    {
        Debug.Log("_receivePurchaserInfo " + purchaserInfoJson);

        if (listener == null) return;

        var response = JsonUtility.FromJson<ReceivePurchaserInfoResponse>(purchaserInfoJson);
        var info = response.purchaserInfo.activeSubscriptions != null
                    ? new PurchaserInfo(response.purchaserInfo)
                    : null;
        if (info != null)
        {
            listener.PurchaserInfoReceived(info);
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void _restoreTransactions(string purchaserInfoJson)
    {
        Debug.Log("_restoreTransactions " + purchaserInfoJson);
        ReceivePurchaserInfoMethod(purchaserInfoJson, RestoreTransactionsCallback);
        RestoreTransactionsCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _identify(string purchaserInfoJson)
    {
        Debug.Log("_identify " + purchaserInfoJson);
        ReceivePurchaserInfoMethod(purchaserInfoJson, IdentifyCallback);
        IdentifyCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _reset(string purchaserInfoJson)
    {
        Debug.Log("_reset " + purchaserInfoJson);
        ReceivePurchaserInfoMethod(purchaserInfoJson, ResetCallback);
        ResetCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _getOfferings(string offeringsJson)
    {
        Debug.Log("_getOfferings " + offeringsJson);
        if (GetOfferingsCallback == null) return;
        var response = JsonUtility.FromJson<GetOfferingsResponse>(offeringsJson);
        var error = response.error.message != null ? response.error : null;
        if (error != null)
        {
            GetOfferingsCallback(null, error);
        }
        else
        {
            GetOfferingsCallback(new Offerings(response.offerings), null);
        }
        GetEntitlementsCallback = null;
    }

    private static void ReceivePurchaserInfoMethod(string arguments, PurchaserInfoFunc callback)
    {
        if (callback == null) return;
        var response = JsonUtility.FromJson<ReceivePurchaserInfoResponse>(arguments);

        var error = response.error.message != null ? response.error : null;
        var info = response.purchaserInfo.activeSubscriptions != null
            ? new PurchaserInfo(response.purchaserInfo)
            : null;
        if (error != null)
        {
            callback(null, error);
        }
        else
        {
            callback(info, null);
        }
    }

    [Serializable]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public class Error
    {
        public string message;
        public int code;
        public string underlyingErrorMessage;
        public string readableErrorCode;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public class Product
    {
        public string title;
        public string identifier;
        public string description;
        public float price;
        public string priceString;
        [CanBeNull] public string currencyCode;
        public float introPrice;
        [CanBeNull] public string introPriceString;
        [CanBeNull] public string introPricePeriod;
        [CanBeNull] public string introPricePeriodUnit;
        [CanBeNull] public int introPricePeriodNumberOfUnits;
        [CanBeNull] public int introPriceCycles;
    }

    [Serializable]
    private class ReceiveProductsResponse
    {
        public List<Product> products;
        public Error error;
    }

    [Serializable]
    private class ReceivePurchaserInfoResponse
    {
        public PurchaserInfoResponse purchaserInfo;
        public Error error;
    }

    [Serializable]
    private class MakePurchaseResponse
    {
        public string productIdentifier;
        public PurchaserInfoResponse purchaserInfo;
        public bool userCancelled;
        public Error error;
    }

    [Serializable]
    public class PurchaserInfoResponse
    {
        public EntitlementInfosResponse entitlements;
        public List<string> activeSubscriptions;
        public List<string> allPurchasedProductIdentifiers;
        public long latestExpirationDate;
        public long firstSeen;
        public string originalAppUserId;
        public long requestDate;
        public List<string> allExpirationDateKeys;
        public List<long> allExpirationDateValues;
        public List<string> allPurchaseDateKeys;
        public List<long> allPurchaseDateValues;
        public string originalApplicationVersion;
    }

    [Serializable]
    public class OfferingsResponse
    {
        public List<string> allKeys;
        public List<OfferingResponse> allValues;
        public OfferingResponse current;
    }

    [Serializable]
    public class GetOfferingsResponse
    {
        public OfferingsResponse offerings;
        public Error error;
    }

    [Serializable]
    public class OfferingResponse
    {
        public string identifier;
        public string serverDescription;
        public List<PackageResponse> availablePackages;
        public PackageResponse lifetime;
        public PackageResponse annual;
        public PackageResponse sixMonth;
        public PackageResponse threeMonth;
        public PackageResponse twoMonth;
        public PackageResponse monthly;
        public PackageResponse weekly;
    }

    [Serializable]
    public class PackageResponse
    {
        public string identifier;
        public string packageType;
        public Product product;
        public string offeringIdentifier;
    }

    [Serializable]
    public class EntitlementInfosResponse
    {
        public List<string> allKeys;
        public List<EntitlementInfoResponse> allValues;
        public List<string> activeKeys;
        public List<EntitlementInfoResponse> activeValues;
    }

    [Serializable]
    public class EntitlementInfoResponse
    {
        public string identifier;
        public bool isActive;
        public bool willRenew;
        public string periodType;
        public long latestPurchaseDate;
        public long originalPurchaseDate;
        [CanBeNull] public long expirationDate;
        public string store;
        public string productIdentifier;
        public bool isSandbox;
        [CanBeNull] public long unsubscribeDetectedAt;
        [CanBeNull] public long billingIssueDetectedAt;
    }
}
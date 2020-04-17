using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RevenueCat.MiniJSON;

#pragma warning disable CS0649

public partial class Purchases : MonoBehaviour
{
    public delegate void GetProductsFunc(List<Product> products, Error error);

    public delegate void MakePurchaseFunc(string productIdentifier, PurchaserInfo purchaserInfo, bool userCancelled, Error error);

    public delegate void PurchaserInfoFunc(PurchaserInfo purchaserInfo, Error error);

    public delegate void GetEntitlementsFunc(Dictionary<string, object> entitlements, Error error);

    public delegate void GetOfferingsFunc(Offerings offerings, Error error);

    public delegate void CheckTrialOrIntroductoryPriceEligibilityFunc(Dictionary<string, IntroEligibility> products);


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
            Product = new Product(response.product);
            OfferingIdentifier = response.offeringIdentifier;
        }
    }

    public class Offering
    {
        public readonly string Identifier;
        public readonly string ServerDescription;
        public readonly List<Package> AvailablePackages;
        [CanBeNull] public readonly Package Lifetime;
        [CanBeNull] public readonly Package Annual;
        [CanBeNull] public readonly Package SixMonth;
        [CanBeNull] public readonly Package ThreeMonth;
        [CanBeNull] public readonly Package TwoMonth;
        [CanBeNull] public readonly Package Monthly;
        [CanBeNull] public readonly Package Weekly;

        public Offering(OfferingResponse response)
        {
            Identifier = response.identifier;
            ServerDescription = response.serverDescription;
            AvailablePackages = new List<Package>();
            foreach (var packageResponse in response.availablePackages)
            {
                AvailablePackages.Add(new Package(packageResponse));
            }
            if (response.lifetime.identifier != null)
            {
                Lifetime = new Package(response.lifetime);
            }
            if (response.annual.identifier != null)
            {
                Annual = new Package(response.annual);
            }
            if (response.sixMonth.identifier != null)
            {
                SixMonth = new Package(response.sixMonth);
            }
            if (response.threeMonth.identifier != null)
            {
                ThreeMonth = new Package(response.threeMonth);
            }
            if (response.twoMonth.identifier != null)
            {
                TwoMonth = new Package(response.twoMonth);
            }
            if (response.monthly.identifier != null)
            {
                Monthly = new Package(response.monthly);
            }
            if (response.weekly.identifier != null)
            {
                Weekly = new Package(response.weekly);
            }
        }
    }

    public class IntroEligibility
    {
        /// The introductory price eligibility status
        public readonly IntroEligibilityStatus Status;

        /// Description of the status
        public readonly string Description;

        public IntroEligibility(IntroEligibilityResponse response)
        {
            Status = (IntroEligibilityStatus)response.status;
            Description = response.description;
        }

        public override string ToString()
        {
            return "{ status:" + Status + "; description:" + Description + " }";
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
    public void GetProducts(string[] products, GetProductsFunc callback, string type = "subs")
    {
        ProductsCallback = callback;
        _wrapper.GetProducts(products, type);
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

    public void AddAttributionData(Dictionary<string, object> data, AttributionNetwork network, string networkUserId = null)
    {
        _wrapper.AddAttributionData((int)network, Json.Serialize(data), networkUserId);
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
    private CheckTrialOrIntroductoryPriceEligibilityFunc CheckTrialOrIntroductoryPriceEligibilityCallback { get; set; }
    public void CheckTrialOrIntroductoryPriceEligibility(string[] products, CheckTrialOrIntroductoryPriceEligibilityFunc callback)
    {
        CheckTrialOrIntroductoryPriceEligibilityCallback = callback;
        _wrapper.CheckTrialOrIntroductoryPriceEligibility(products);
    }

    public void InvalidatePurchaserInfoCache()
    {
        _wrapper.InvalidatePurchaserInfoCache();
    }

    public void SetAttributes(Dictionary<string, string> attributes)
    {
        _wrapper.SetAttributes(attributes);
    }

    public void SetEmail(string email)
    {
        _wrapper.SetEmail(email);
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        _wrapper.SetPhoneNumber(phoneNumber);
    }

    public void SetDisplayName(string displayName)
    {
        _wrapper.SetDisplayName(displayName);
    }

    public void SetPushToken(string token)
    {
        _wrapper.SetPushToken(token);
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
            var products = new List<Product>();
            foreach (var productResponse in response.products)
            {
                products.Add(new Product(productResponse));
            }
            ProductsCallback(products, null);
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
    private void _checkTrialOrIntroductoryPriceEligibility(string json)
    {
        Debug.Log("_checkTrialOrIntroductoryPriceEligibilit " + json);

        if (CheckTrialOrIntroductoryPriceEligibilityCallback == null) return;

        var responseMap = JsonUtility.FromJson<MapResponse<string, IntroEligibilityResponse>>(json);

        var dictionary = new Dictionary<string, IntroEligibility>();
        for (var i = 0; i < responseMap.keys.Count; i++)
        {
            dictionary[responseMap.keys[i]] = new IntroEligibility(responseMap.values[i]);
        }

        CheckTrialOrIntroductoryPriceEligibilityCallback(dictionary);

        CheckTrialOrIntroductoryPriceEligibilityCallback = null;
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
    private class ReceiveProductsResponse
    {
        public List<ProductResponse> products;
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
        public long latestExpirationDateMillis;
        public long firstSeenMillis;
        public string originalAppUserId;
        public long requestDateMillis;
        public List<string> allExpirationDatesMillisKeys;
        public List<long> allExpirationDatesMillisValues;
        public List<string> allPurchaseDatesMillisKeys;
        public List<long> allPurchaseDatesMillisValues;
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
        [CanBeNull] public PackageResponse lifetime;
        [CanBeNull] public PackageResponse annual;
        [CanBeNull] public PackageResponse sixMonth;
        [CanBeNull] public PackageResponse threeMonth;
        [CanBeNull] public PackageResponse twoMonth;
        [CanBeNull] public PackageResponse monthly;
        [CanBeNull] public PackageResponse weekly;
    }

    [Serializable]
    public class PackageResponse
    {
        public string identifier;
        public string packageType;
        public ProductResponse product;
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
        public long latestPurchaseDateMillis;
        public long originalPurchaseDateMillis;
        [CanBeNull] public long expirationDateMillis;
        public string store;
        public string productIdentifier;
        public bool isSandbox;
        [CanBeNull] public long unsubscribeDetectedAtMillis;
        [CanBeNull] public long billingIssueDetectedAtMillis;
    }

    [Serializable]
    public class IntroEligibilityResponse
    {
        public int status;
        public string description;
    }

    [Serializable]
    public class MapResponse<K, V>
    {
        public List<K> keys;
        public List<V> values;
    }

    [Serializable]
    public class ProductResponse
    {
        public string title;
        public string identifier;
        public string description;
        public float price;
        public string price_string;
        [CanBeNull] public string currency_code;
        public float intro_price;
        public string intro_price_string;
        public string intro_price_period;
        public string intro_price_period_unit;
        [CanBeNull] public int intro_price_period_number_of_units;
        [CanBeNull] public int intro_price_cycles;
    }
}
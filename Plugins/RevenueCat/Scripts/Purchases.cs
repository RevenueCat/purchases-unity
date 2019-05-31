using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0649

public class Purchases : MonoBehaviour
{

    public delegate void GetProductsFunc(List<Product> products, Error error);

    public delegate void MakePurchaseFunc(string productIdentifier, PurchaserInfo purchaserInfo, bool userCancelled, Error error);

    public delegate void PurchaserInfoFunc(PurchaserInfo purchaserInfo, Error error);

    public delegate void GetEntitlementsFunc(Dictionary<string, Entitlement> entitlements, Error error);

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

        public void GetEntitlements()
        {

        }

        public void SyncPurchases()
        {

        }
        
        public void SetAutomaticAttributionCollection(bool enabled)
        {

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

        public List<string> ActiveSubscriptions
        {
            get { return _response.activeSubscriptions; }
        }

        public List<string> AllPurchasedProductIdentifiers
        {
            get { return _response.allPurchasedProductIdentifiers; }
        }

        public DateTime LatestExpirationDate
        {
            get { return FromUnixTime(_response.latestExpirationDate); }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public Dictionary<string, DateTime> AllExpirationDates
        {
            get
            {
                var allExpirations = new Dictionary<string, DateTime>();
                for (var i = 0; i < _response.allExpirationDateKeys.Count; i++)
                {
                    var date = FromUnixTime(_response.allExpirationDateValues[i]);
                    allExpirations[_response.allExpirationDateKeys[i]] = date;
                }

                return allExpirations;
            }
        }
    }

    public class Entitlement
    {

        public readonly Dictionary<string, Product> offerings;

        public Entitlement(EntitlementResponse response)
        {
            offerings = new Dictionary<string, Product>();
            foreach (var offering in response.offerings)
            {
                Debug.Log("offering " + offering.product);
                if (offering.product.identifier != null)
                {
                    offerings.Add(offering.offeringId, offering.product);
                }
            }
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
    public void MakePurchase(string productIdentifier, MakePurchaseFunc callback,
        string type = "subs", string oldSku = null)
    {
        MakePurchaseCallback = callback;
        _wrapper.MakePurchase(productIdentifier, type, oldSku);
    }

    private PurchaserInfoFunc RestoreTransactionsCallback { get; set; }

    public void RestoreTransactions(PurchaserInfoFunc callback)
    {
        RestoreTransactionsCallback = callback;
        _wrapper.RestoreTransactions();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum AttributionNetwork
    {
        APPLE_SEARCH_ADS = 0,
        ADJUST = 1,
        APPSFLYER = 2,
        BRANCH = 3,
        TENJIN = 4
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

    public void GetEntitlements(GetEntitlementsFunc callback)
    {
        GetEntitlementsCallback = callback;
        _wrapper.GetEntitlements();
    }

    public void SyncPurchases()
    {
        _wrapper.SyncPurchases();
    }

    // ReSharper disable once UnusedMember.Global
    public void SetAutomaticAttributionCollection(bool enabled)
    {
        _wrapper.SetAutomaticAttributionCollection(enabled);
    }

    // ReSharper disable once UnusedMember.Local
    private void _receiveProducts(string productsJson)
    {
        Debug.Log("_receiveProducts " + productsJson);

        if (ProductsCallback == null) return;

        var response = JsonUtility.FromJson<ProductResponse>(productsJson);
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
    private void _getEntitlements(string entitlementsJson)
    {
        Debug.Log("_getEntitlements " + entitlementsJson);
        if (GetEntitlementsCallback == null) return;
        var response = JsonUtility.FromJson<EntitlementsResponse>(entitlementsJson);
        var error = response.error.message != null ? response.error : null;
        if (error != null)
        {
            GetEntitlementsCallback(null, error);
        }
        else
        {
            var entitlements = new Dictionary<string, Entitlement>();
            foreach (var entitlementResponse in response.entitlements)
            {
                Debug.Log(entitlementResponse.entitlementId);
                entitlements.Add(entitlementResponse.entitlementId, new Entitlement(entitlementResponse));
            }
            GetEntitlementsCallback(entitlements, null);
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
    }

    [Serializable]
    private class ProductResponse
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
        public List<string> activeSubscriptions;
        public List<string> allPurchasedProductIdentifiers;
        public long latestExpirationDate;
        public List<string> allExpirationDateKeys;
        public List<long> allExpirationDateValues;
    }

    [Serializable]
    public class EntitlementsResponse
    {
        public List<EntitlementResponse> entitlements;
        public Error error;
    }

    [Serializable]
    public class Offering
    {
        public string offeringId;
        public Product product;
    }

    [Serializable]
    public class EntitlementResponse
    {
        public string entitlementId;
        public List<Offering> offerings;
    }

}
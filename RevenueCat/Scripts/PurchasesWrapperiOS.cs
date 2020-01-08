using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_IOS || UNITY_TVOS || UNITY_STANDALONE_OSX
public class PurchasesWrapperiOS : IPurchasesWrapper
{

    #if UNITY_IOS || UNITY_TVOS
    private const string Import = "__Internal";
    #else
    private const string Import = "Purchases";
    #endif

    #if UNITY_STANDALONE_OSX
    public delegate void UnityCallbackDelegate(IntPtr objectName, IntPtr commandName, IntPtr commandData);
    
    [DllImport("Purchases")]
    public static extern void ConnectCallback([MarshalAs(UnmanagedType.FunctionPtr)]UnityCallbackDelegate callback);
    #endif

    [DllImport(Import)]
    private static extern void _RCSetupPurchases(string gameObject, string apiKey, string appUserId, bool observerMode);
    public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode)
    {
        _RCSetupPurchases(gameObject, apiKey, appUserId, observerMode);
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class ProductsRequest
    {
        public string[] productIdentifiers;
    }

    [DllImport(Import)]
    private static extern void _RCGetProducts(string productIdentifiersJson, string type);
    public void GetProducts(string[] productIdentifiers, string type = "subs")
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };

        _RCGetProducts(JsonUtility.ToJson(request), type);
    }

    [DllImport(Import)]
    private static extern void _RCPurchaseProduct(string productIdentifier);
    public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        _RCPurchaseProduct(productIdentifier);
    }

    [DllImport(Import)]
    private static extern void _RCPurchasePackage(string packageIdentifier, string offeringIdentifier);
    public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        _RCPurchasePackage(packageToPurchase.Identifier, packageToPurchase.OfferingIdentifier);
    }

    [DllImport(Import)]
    private static extern void _RCRestoreTransactions();
    public void RestoreTransactions()
    {
        _RCRestoreTransactions();
    }

    [DllImport(Import)]
    private static extern void _RCAddAttributionData(int network, string data, string networkUserId);
    public void AddAttributionData(int network, string data, string networkUserId)
    {
        _RCAddAttributionData(network, data, networkUserId);
    }

    [DllImport(Import)]
    private static extern void _RCCreateAlias(string newAppUserId);
    public void CreateAlias(string newAppUserId)
    {
        _RCCreateAlias(newAppUserId);
    }

    [DllImport(Import)]
    private static extern void _RCIdentify(string appUserId);
    public void Identify(string appUserId)
    {
        _RCIdentify(appUserId);
    }

    [DllImport(Import)]
    private static extern void _RCReset();
    public void Reset()
    {
        _RCReset();
    }

    [DllImport(Import)]
    private static extern void _RCSetFinishTransactions(bool finishTransactions);
    public void SetFinishTransactions(bool finishTransactions)
    {
        _RCSetFinishTransactions(finishTransactions);
    }

    [DllImport(Import)]
    private static extern void _RCSetAllowSharingStoreAccount(bool allow);
    public void SetAllowSharingStoreAccount(bool allow)
    {
        _RCSetAllowSharingStoreAccount(allow);
    }

    [DllImport(Import)]
    private static extern void _RCSetDebugLogsEnabled(bool enabled);
    public void SetDebugLogsEnabled(bool enabled)
    {
        _RCSetDebugLogsEnabled(enabled);
    }

    [DllImport(Import)]
    private static extern string _RCGetAppUserID();
    public string GetAppUserId()
    {
        return _RCGetAppUserID();
    }

    [DllImport(Import)]
    private static extern void _RCGetPurchaserInfo();
    public void GetPurchaserInfo()
    {
        _RCGetPurchaserInfo();
    }

    [DllImport(Import)]
    private static extern void _RCGetOfferings();
    public void GetOfferings()
    {
        _RCGetOfferings();
    }

    [DllImport(Import)]
    private static extern void _RCSyncPurchases();
    public void SyncPurchases()
    {
        // NOOP
    }

    [DllImport(Import)]
    private static extern void _RCSetAutomaticAppleSearchAdsAttributionCollection(bool enabled);
    public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
    {
        _RCSetAutomaticAppleSearchAdsAttributionCollection(enabled);
    }

    [DllImport(Import)]
    private static extern bool _RCIsAnonymous();
    public bool IsAnonymous()
    {
        return _RCIsAnonymous();
    }

    [DllImport(Import)]
    private static extern void _RCCheckTrialOrIntroductoryPriceEligibility(string productIdentifiersJson);
    public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers)
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };

        _RCCheckTrialOrIntroductoryPriceEligibility(JsonUtility.ToJson(request));
    }

}
#endif
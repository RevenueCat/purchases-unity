using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_IOS
public class PurchasesWrapperiOS : IPurchasesWrapper
{
    [DllImport("__Internal")]
    private static extern void _RCSetupPurchases(string gameObject, string apiKey, string appUserId, bool observerMode, string userDefaultsSuiteName);
    public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode, string userDefaultsSuiteName)
    {
        _RCSetupPurchases(gameObject, apiKey, appUserId, observerMode, userDefaultsSuiteName);
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class ProductsRequest
    {
        public string[] productIdentifiers;
    }

    [DllImport("__Internal")]
    private static extern void _RCGetProducts(string productIdentifiersJson, string type);
    public void GetProducts(string[] productIdentifiers, string type = "subs")
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };

        _RCGetProducts(JsonUtility.ToJson(request), type);
    }

    [DllImport("__Internal")]
    private static extern void _RCPurchaseProduct(string productIdentifier);
    public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        _RCPurchaseProduct(productIdentifier);
    }

    [DllImport("__Internal")]
    private static extern void _RCPurchasePackage(string packageIdentifier, string offeringIdentifier);
    public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        _RCPurchasePackage(packageToPurchase.Identifier, packageToPurchase.OfferingIdentifier);
    }

    [DllImport("__Internal")]
    private static extern void _RCRestoreTransactions();
    public void RestoreTransactions()
    {
        _RCRestoreTransactions();
    }

    [DllImport("__Internal")]
    private static extern void _RCAddAttributionData(int network, string data, string networkUserId);
    public void AddAttributionData(int network, string data, string networkUserId)
    {
        _RCAddAttributionData(network, data, networkUserId);
    }

    [DllImport("__Internal")]
    private static extern void _RCCreateAlias(string newAppUserId);
    public void CreateAlias(string newAppUserId)
    {
        _RCCreateAlias(newAppUserId);
    }

    [DllImport("__Internal")]
    private static extern void _RCIdentify(string appUserId);
    public void Identify(string appUserId)
    {
        _RCIdentify(appUserId);
    }

    [DllImport("__Internal")]
    private static extern void _RCReset();
    public void Reset()
    {
        _RCReset();
    }

    [DllImport("__Internal")]
    private static extern void _RCSetFinishTransactions(bool finishTransactions);
    public void SetFinishTransactions(bool finishTransactions)
    {
        _RCSetFinishTransactions(finishTransactions);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAllowSharingStoreAccount(bool allow);
    public void SetAllowSharingStoreAccount(bool allow)
    {
        _RCSetAllowSharingStoreAccount(allow);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetDebugLogsEnabled(bool enabled);
    public void SetDebugLogsEnabled(bool enabled)
    {
        _RCSetDebugLogsEnabled(enabled);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetProxyUrl(string proxyUrl);
    public void SetProxyUrl(string proxyUrl)
    {
        _RCSetProxyUrl(proxyUrl);
    }

    [DllImport("__Internal")]
    private static extern string _RCGetAppUserID();
    public string GetAppUserId()
    {
        return _RCGetAppUserID();
    }

    [DllImport("__Internal")]
    private static extern void _RCGetPurchaserInfo();
    public void GetPurchaserInfo()
    {
        _RCGetPurchaserInfo();
    }

    [DllImport("__Internal")]
    private static extern void _RCGetOfferings();
    public void GetOfferings()
    {
        _RCGetOfferings();
    }

    [DllImport("__Internal")]
    private static extern void _RCSyncPurchases();
    public void SyncPurchases()
    {
        // NOOP
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAutomaticAppleSearchAdsAttributionCollection(bool enabled);
    public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
    {
        _RCSetAutomaticAppleSearchAdsAttributionCollection(enabled);
    }

    [DllImport("__Internal")]
    private static extern bool _RCIsAnonymous();
    public bool IsAnonymous()
    {
        return _RCIsAnonymous();
    }

    [DllImport("__Internal")]
    private static extern void _RCCheckTrialOrIntroductoryPriceEligibility(string productIdentifiersJson);
    public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers)
    {
        var request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };

        _RCCheckTrialOrIntroductoryPriceEligibility(JsonUtility.ToJson(request));
    }

    [DllImport("__Internal")]
    private static extern void _RCInvalidatePurchaserInfoCache();
    public void InvalidatePurchaserInfoCache() 
    {
        _RCInvalidatePurchaserInfoCache();
    }
    
    [DllImport("__Internal")]
    private static extern void _RCSetAttributes(string attributesJson);
    public void SetAttributes(string attributesJson)
    {
        _RCSetAttributes(attributesJson);
    }

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

}
#endif
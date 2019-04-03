using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnityEngine;

public class PurchasesWrapperiOS : IPurchasesWrapper
{
    [DllImport("__Internal")]
    private static extern void _RCSetupPurchases(string gameObject, string apiKey, string appUserId);
    public void Setup(string gameObject, string apiKey, string appUserId)
    {
        _RCSetupPurchases(gameObject, apiKey, appUserId);
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
    private static extern void _RCMakePurchase(string productIdentifier, string type);
    public void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null)
    {
        _RCMakePurchase(productIdentifier, type);
    }

    [DllImport("__Internal")]
    private static extern void _RCRestoreTransactions();
    public void RestoreTransactions()
    {
        _RCRestoreTransactions();
    }

    [DllImport("__Internal")]
    private static extern void _RCAddAttributionData(int network, string data);
    public void AddAttributionData(int network, string data)
    {
        _RCAddAttributionData(network, data);
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
    private static extern void _RCGetEntitlements();
    public void GetEntitlements()
    {
        _RCGetEntitlements();
    }

}

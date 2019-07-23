using System;

public interface IPurchasesWrapper
{
	void Setup(string gameObject, string apiKey, string appUserId, bool observerMode);
	void AddAttributionData(int network, string data, string networkUserId);
	void GetProducts(string[] productIdentifiers, string type = "subs");
	void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null);
	void RestoreTransactions();
	void CreateAlias(string newAppUserId);
	void Identify(string appUserId);
	void Reset();
	void SetFinishTransactions(bool finishTransactions);
    void SetAllowSharingStoreAccount(bool allow);
    void SetDebugLogsEnabled(bool enabled);
    string GetAppUserId();
    void GetPurchaserInfo();
    void GetEntitlements();
	void SyncPurchases();
	void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled);
}
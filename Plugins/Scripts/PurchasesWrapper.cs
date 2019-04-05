using System;

public interface IPurchasesWrapper
{
	void Setup(string gameObject, string apiKey, string appUserId);
	void AddAttributionData(int network, string data);
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
}
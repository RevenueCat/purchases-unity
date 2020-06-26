using System;
using System.Collections.Generic;

public interface IPurchasesWrapper
{
	void Setup(string gameObject, string apiKey, string appUserId, bool observerMode, string userDefaultsSuiteName);
	void AddAttributionData(int network, string data, string networkUserId);
	void GetProducts(string[] productIdentifiers, string type = "subs");
    void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy);
    void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null, Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy);
    void RestoreTransactions();
	void CreateAlias(string newAppUserId);
	void Identify(string appUserId);
	void Reset();
	void SetFinishTransactions(bool finishTransactions);
    void SetAllowSharingStoreAccount(bool allow);
    void SetDebugLogsEnabled(bool enabled);
    string GetAppUserId();
    void GetPurchaserInfo();
    void GetOfferings();
	void SyncPurchases();
	void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled);
    bool IsAnonymous();
    void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers);
    void InvalidatePurchaserInfoCache();
    void SetAttributes(string attributesJson);
    void SetEmail(string email);
    void SetPhoneNumber(string phoneNumber);
    void SetDisplayName(string displayName);
    void SetPushToken(string token);
}
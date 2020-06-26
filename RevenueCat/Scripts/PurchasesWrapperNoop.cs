using System.Collections.Generic;

public partial class Purchases
{
    private class PurchasesWrapperNoop : IPurchasesWrapper
    {
        public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode, string userDefaultsSuiteName)
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

        public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
        {
        }

        public void PurchasePackage(Package packageToPurchase, string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
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

        public void SetProxyUrl(string proxyUrl)
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

        public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers)
        {
        }

        public void InvalidatePurchaserInfoCache()
        {
        }

        public void SetAttributes(string attributesJson)
        {
        }

        public void SetEmail(string email)
        {
        }

        public void SetPhoneNumber(string phoneNumber)
        {
        }

        public void SetDisplayName(string displayName)
        {
        }

        public void SetPushToken(string token)
        {
        }
    }
}
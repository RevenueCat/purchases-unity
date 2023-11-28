using System.Collections.Generic;

public partial class Purchases
{
    private class PurchasesWrapperNoop : IPurchasesWrapper
    {
        public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode, bool usesStoreKit2IfAvailable,
            string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson, bool shouldShowInAppMessagesAutomatically)
        {
        }

        public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode, bool usesStoreKit2IfAvailable,
            string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson, bool shouldShowInAppMessagesAutomatically,
            EntitlementVerificationMode entitlementVerificationMode)
        {
        }

        public void GetProducts(string[] productIdentifiers, string type = "subs")
        {
        }

        public void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null)
        {
        }

        public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false, string presentedOfferingIdentifier = null,
            Purchases.PromotionalOffer discount = null)
        {
        }

        public void PurchasePackage(Package packageToPurchase, string oldSku = null,
            ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
            bool googleIsPersonalizedPrice = false, Purchases.PromotionalOffer discount = null)
        {
        }

        public void PurchaseSubscriptionOption(Purchases.SubscriptionOption subscriptionOption,
            Purchases.GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false)
        {
        }

        public void RestorePurchases()
        {
        }

        public void LogIn(string appUserId)
        {
        }

        public void LogOut()
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

        public void SetLogLevel(LogLevel level)
        {
        }

        public void SetLogHandler()
        {
        }

        public void SetProxyURL(string proxyURL)
        {
        }

        public void GetCustomerInfo()
        {
        }

        public void GetOfferings()
        {
        }

        public void SyncPurchases()
        {
        }

        public void SyncObserverModeAmazonPurchase(string productID, string receiptID, string amazonUserID,
            string isoCurrencyCode, double price)
        {
        }

        public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
        {
        }

        public void EnableAdServicesAttributionTokenCollection()
        {
        }

        public bool IsAnonymous()
        {
            return false;
        }

        public bool IsConfigured()
        {
            return false;
        }

        public void CheckTrialOrIntroductoryPriceEligibility(string[] productIdentifiers)
        {
        }

        public void InvalidateCustomerInfoCache()
        {
        }

        public void PresentCodeRedemptionSheet()
        {
        }

        public void SetSimulatesAskToBuyInSandbox(bool enabled)
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

        public void SetAdjustID(string adjustID)
        {
        }

        public void SetAppsflyerID(string appsflyerID)
        {
        }

        public void SetFBAnonymousID(string fbAnonymousID)
        {
        }

        public void SetMparticleID(string mparticleID)
        {
        }

        public void SetOnesignalID(string onesignalID)
        {
        }

        public void SetAirshipChannelID(string airshipChannelID)
        {
        }

        public void SetCleverTapID(string cleverTapID)
        {
        }

        public void SetMixpanelDistinctID(string mixpanelDistinctID)
        {
        }

        public void SetFirebaseAppInstanceID(string firebaseAppInstanceID)
        {
        }

        public void SetMediaSource(string mediaSource)
        {
        }

        public void SetCampaign(string campaign)
        {
        }

        public void SetAdGroup(string adGroup)
        {
        }

        public void SetAd(string ad)
        {
        }

        public void SetKeyword(string keyword)
        {
        }

        public void SetCreative(string creative)
        {
        }

        public void CollectDeviceIdentifiers()
        {
        }

        public void CanMakePayments(Purchases.BillingFeature[] features)
        {
        }

        public void GetPromotionalOffer(string productIdentifier, string discountIdentifier)
        {
        }

        public void ShowInAppMessages(Purchases.InAppMessageType[] messageTypes)
        {
        }
    }
}

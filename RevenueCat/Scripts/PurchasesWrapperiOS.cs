using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_IOS
public class PurchasesWrapperiOS : IPurchasesWrapper
{
    [DllImport("__Internal")]
    private static extern void _RCSetupPurchases(string gameObject, string apiKey, string appUserId, bool observerMode,
                                                 bool usesStoreKit2IfAvailable, string userDefaultsSuiteName,
                                                 string dangerousSettingsJson);

    public void Setup(string gameObject, string apiKey, string appUserId, bool observerMode, bool usesStoreKit2IfAvailable,
        string userDefaultsSuiteName, bool useAmazon, string dangerousSettingsJson)
    {
        _RCSetupPurchases(gameObject, apiKey, appUserId, observerMode, usesStoreKit2IfAvailable,
            userDefaultsSuiteName, dangerousSettingsJson);
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
    private static extern void _RCPurchaseProduct(string productIdentifier, string signedDiscountTimestamp);
    public void PurchaseProduct(string productIdentifier, string type = "subs", string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        Purchases.PromotionalOffer discount = null)
    {
        string discountTimestamp = null;
        if (discount != null)
        {
            discountTimestamp = discount.Timestamp.ToString();
        }
        _RCPurchaseProduct(productIdentifier, discountTimestamp);
    }

    [DllImport("__Internal")]
    private static extern void _RCPurchasePackage(string packageIdentifier, string offeringIdentifier, string signedDiscountTimestamp);
    public void PurchasePackage(Purchases.Package packageToPurchase, string oldSku = null,
        Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
        Purchases.PromotionalOffer discount = null)
    {
        string discountTimestamp = null;
        if (discount != null)
        {
            discountTimestamp = discount.Timestamp.ToString();
        }
        _RCPurchasePackage(packageToPurchase.Identifier, packageToPurchase.OfferingIdentifier, discountTimestamp);
    }

    [DllImport("__Internal")]
    private static extern void _RCRestorePurchases();
    public void RestorePurchases()
    {
        _RCRestorePurchases();
    }

    [DllImport("__Internal")]
    private static extern void _RCSyncPurchases();
    public void SyncPurchases()
    {
        _RCSyncPurchases();
    }

    public void SyncObserverModeAmazonPurchase(string productID, string receiptID, string amazonUserID,
        string isoCurrencyCode, double price)
    {
        // No-Op
    }

    [DllImport("__Internal")]
    private static extern void _RCLogIn(string appUserId);
    public void LogIn(string appUserId)
    {
        _RCLogIn(appUserId);
    }

    [DllImport("__Internal")]
    private static extern void _RCLogOut();
    public void LogOut()
    {
        _RCLogOut();
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
    private static extern void _RCSetProxyURLString(string proxyURL);
    public void SetProxyURL(string proxyURL)
    {
        _RCSetProxyURLString(proxyURL);
    }

    [DllImport("__Internal")]
    private static extern string _RCGetAppUserID();
    public string GetAppUserId()
    {
        return _RCGetAppUserID();
    }

    [DllImport("__Internal")]
    private static extern void _RCGetCustomerInfo();
    public void GetCustomerInfo()
    {
        _RCGetCustomerInfo();
    }

    [DllImport("__Internal")]
    private static extern void _RCGetOfferings();
    public void GetOfferings()
    {
        _RCGetOfferings();
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAutomaticAppleSearchAdsAttributionCollection(bool enabled);
    public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
    {
        _RCSetAutomaticAppleSearchAdsAttributionCollection(enabled);
    }

    [DllImport("__Internal")]
    private static extern void _RCEnableAdServicesAttributionTokenCollection();
    public void EnableAdServicesAttributionTokenCollection()
    {
        _RCEnableAdServicesAttributionTokenCollection();
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
    private static extern void _RCInvalidateCustomerInfoCache();
    public void InvalidateCustomerInfoCache()
    {
        _RCInvalidateCustomerInfoCache();
    }

    [DllImport("__Internal")]
    private static extern void _RCPresentCodeRedemptionSheet();
    public void PresentCodeRedemptionSheet()
    {
        _RCPresentCodeRedemptionSheet();
    }

    [DllImport("__Internal")]
    private static extern void _RCSetSimulatesAskToBuyInSandbox(bool enabled);
    public void SetSimulatesAskToBuyInSandbox(bool enabled)
    {
        _RCSetSimulatesAskToBuyInSandbox(enabled);
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

    [DllImport("__Internal")]
    private static extern void _RCSetAdjustID(string adjustID);
    public void SetAdjustID(string adjustID)
    {
        _RCSetAdjustID(adjustID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAppsflyerID(string appsflyerID);
    public void SetAppsflyerID(string appsflyerID)
    {
        _RCSetAppsflyerID(appsflyerID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetFBAnonymousID(string fbAnonymousID);
    public void SetFBAnonymousID(string fbAnonymousID)
    {
        _RCSetFBAnonymousID(fbAnonymousID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetMparticleID(string mparticleID);
    public void SetMparticleID(string mparticleID)
    {
        _RCSetMparticleID(mparticleID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetOnesignalID(string onesignalID);
    public void SetOnesignalID(string onesignalID)
    {
        _RCSetOnesignalID(onesignalID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAirshipChannelID(string airshipChannelID);
    public void SetAirshipChannelID(string airshipChannelID)
    {
        _RCSetAirshipChannelID(airshipChannelID);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetMediaSource(string mediaSource);
    public void SetMediaSource(string mediaSource)
    {
        _RCSetMediaSource(mediaSource);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetCampaign(string campaign);
    public void SetCampaign(string campaign)
    {
        _RCSetCampaign(campaign);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAdGroup(string adGroup);
    public void SetAdGroup(string adGroup)
    {
        _RCSetAdGroup(adGroup);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAd(string ad);
    public void SetAd(string ad)
    {
        _RCSetAd(ad);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetKeyword(string keyword);
    public void SetKeyword(string keyword)
    {
        _RCSetKeyword(keyword);
    }

    [DllImport("__Internal")]
    private static extern void _RCSetCreative(string creative);
    public void SetCreative(string creative)
    {
        _RCSetCreative(creative);
    }

    [DllImport("__Internal")]
    private static extern void _RCCollectDeviceIdentifiers();
    public void CollectDeviceIdentifiers()
    {
        _RCCollectDeviceIdentifiers();
    }

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private class CanMakePaymentsRequest
    {
        public int[] features;
    }

    [DllImport("__Internal")]
    private static extern void _RCCanMakePayments(string featuresJson);
    public void CanMakePayments(Purchases.BillingFeature[] features)
    {
        int[] featuresAsInts = new int[features.Length];
        for (int i = 0; i < features.Length; i++) {
            Purchases.BillingFeature feature = features[i];
            featuresAsInts[i] = (int)feature;
        }

        var request = new CanMakePaymentsRequest
        {
            features = featuresAsInts
        };

        _RCCanMakePayments(JsonUtility.ToJson(request));
    }

    [DllImport("__Internal")]
    private static extern void _RCGetPromotionalOffer(string productIdentifier, string discountIdentifier);
    public void GetPromotionalOffer(string productIdentifier, string discountIdentifier)
    {
        _RCGetPromotionalOffer(productIdentifier, discountIdentifier);
    }

}
#endif
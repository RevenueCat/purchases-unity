using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;
using RevenueCat.SimpleJSON;

#pragma warning disable CS0649

public partial class Purchases : MonoBehaviour
{
    public delegate void GetProductsFunc(List<StoreProduct> products, Error error);

    public delegate void MakePurchaseFunc(string productIdentifier, CustomerInfo customerInfo, bool userCancelled, Error error);

    public delegate void CustomerInfoFunc(CustomerInfo customerInfo, Error error);

    public delegate void LogInFunc(CustomerInfo customerInfo, bool created, Error error);

    public delegate void GetOfferingsFunc(Offerings offerings, Error error);

    public delegate void CheckTrialOrIntroductoryPriceEligibilityFunc(Dictionary<string, IntroEligibility> products);
    
    /// <summary>
    /// Callback function containing the result of CanMakePayments
    /// <param name="canMakePayments">A bool value indicating whether billing
    /// is supported for the current user (meaning IN-APP purchases are supported),
    /// and, if provided, whether a list of specified BillingFeatures are supported.
    /// This will be false if there is an error</param>
    /// <param name="error">An Error object or null if successful.</param>
    /// 
    /// </summary>
    public delegate void CanMakePaymentsFunc(bool canMakePayments, Error error);
    
    /// <summary>
    /// Callback function containing the result of GetPromotionalOffer
    /// <param name="promotionalOffer">A Purchases.PromotionalOffer. It will be Null if platform is Android or
    /// the iOS version is not compatible with promotional offers</param>
    /// <param name="error">An Error object or null if successful.</param>
    /// 
    /// </summary>
    public delegate void GetPromotionalOfferFunc(PromotionalOffer promotionalOffer, Error error);

    [Tooltip("Activate if you plan to call Purchases.Setup programmatically.")]
    public bool useRuntimeSetup;

    [Tooltip("RevenueCat API Key specifically for Apple platforms. Get from https://app.revenuecat.com/")]
    public string revenueCatAPIKeyApple;

    [Tooltip("RevenueCat API Key specifically for Google Play. Get from https://app.revenuecat.com/")]
    public string revenueCatAPIKeyGoogle;

    [Header("Alternative Stores")]
    [Tooltip("RevenueCat API Key specifically for Amazon Appstore. Get from https://app.revenuecat.com/")]
    public string revenueCatAPIKeyAmazon;

    [Tooltip("Enables Amazon Store support. Android only, on iOS it has no effect." +
             "If enabled, it will use the API key in RevenueCatAPIKeyAmazon.")]
    public bool useAmazon;

    [Header("Dangerous Settings")]
    [Tooltip("Disable or enable automatically detecting current subscriptions." +  
             "If this is disabled, RevenueCat won't check current purchases, and it will not sync any purchase automatically " +
             "when the app starts. Call syncPurchases whenever a new purchase is detected so the receipt is sent to " +
             "RevenueCat's backend. " +
             "In iOS, consumables disappear from the receipt after the transaction is finished, so make sure purchases " +
             "are synced before finishing any consumable transaction, otherwise RevenueCat won't register the purchase. " +
             "Auto syncing of purchases is enabled by default.")]
    public bool autoSyncPurchases = true;

    [Tooltip(
        "App user id. Pass in your own ID if your app has accounts. If blank, RevenueCat will generate a user ID for you.")]
    // ReSharper disable once InconsistentNaming
    public string AppUserID;

    [Tooltip("List of product identifiers.")]
    public string[] productIdentifiers;

    [Tooltip("A subclass of Purchases.UpdatedCustomerInfoListener component. Use your custom subclass to define how to handle updated customer information.")]
    public UpdatedCustomerInfoListener listener;

    [Tooltip("An optional boolean. Set this to TRUE if you have your own IAP implementation and want to use only RevenueCat's backend. Default is FALSE.")]
    public bool observerMode;

    [Tooltip("An optional string. iOS only. Set this to use a specific NSUserDefaults suite for RevenueCat. This might be handy if you are deleting all NSUserDefaults in your app and leaving RevenueCat in a bad state.")]
    public string userDefaultsSuiteName;

    [Header("Advanced")]
    [Tooltip("Set this property to your proxy URL before configuring Purchases *only* if you've received a proxy key value from your RevenueCat contact.")]
    public string proxyURL;

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
        if (!string.IsNullOrEmpty(proxyURL))
        {
            _wrapper.SetProxyURL(proxyURL);
        }

        if (useRuntimeSetup) return;

        Setup(string.IsNullOrEmpty(AppUserID) ? null : AppUserID);
        GetProducts(productIdentifiers, null);
    }

    private void Setup(string newUserId)
    {
        var apiKey = "";

        if (Application.platform == RuntimePlatform.IPhonePlayer
            || Application.platform == RuntimePlatform.OSXPlayer)
            apiKey = revenueCatAPIKeyApple;
        else if (Application.platform == RuntimePlatform.Android
            || IsAndroidEmulator())
            apiKey = useAmazon ? revenueCatAPIKeyAmazon : revenueCatAPIKeyGoogle;
        
        var dangerousSettings = new DangerousSettings(autoSyncPurchases);
        var builder = PurchasesConfiguration.Builder.Init(apiKey)
            .SetAppUserId(newUserId)
            .SetObserverMode(observerMode)
            .SetUserDefaultsSuiteName(userDefaultsSuiteName)
            .SetUseAmazon(useAmazon)
            .SetDangerousSettings(dangerousSettings);

        Setup(builder.Build());
    }

    private bool IsAndroidEmulator()
    {
        try
        {
            // From https://stackoverflow.com/questions/51880866/detect-if-game-running-in-android-emulator
            AndroidJavaClass osBuild = new AndroidJavaClass("android.os.Build");
            string fingerPrint = osBuild.GetStatic<string>("FINGERPRINT");
            return fingerPrint.Contains("generic");
        }
        catch
        {
            // Throws error when running on non-Android platforms
            return false;
        }
    }

    private GetProductsFunc ProductsCallback { get; set; }

    // Optionally call this if you want to fetch more products,
    // called automatically with pre-configured products
    // ReSharper disable once MemberCanBePrivate.Global
    public void GetProducts(string[] products, GetProductsFunc callback, string type = "subs")
    {
        ProductsCallback = callback;
        _wrapper.GetProducts(products, type);
    }

    private MakePurchaseFunc MakePurchaseCallback { get; set; }

    public void PurchaseProduct(string productIdentifier, MakePurchaseFunc callback,
        string type = "subs", string oldSku = null, ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchaseProduct(productIdentifier, type, oldSku, prorationMode);
    }
    
    public void PurchaseDiscountedProduct(string productIdentifier, PromotionalOffer discount, MakePurchaseFunc callback)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchaseProduct(productIdentifier, discount: discount);
    }

    public void PurchasePackage(Package package, MakePurchaseFunc callback, string oldSku = null, ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchasePackage(package, oldSku, prorationMode);
    }

    public void PurchaseDiscountedPackage(Package package, PromotionalOffer discount, MakePurchaseFunc callback)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchasePackage(package, discount: discount);
    }

    private CustomerInfoFunc RestorePurchasesCallback { get; set; }

    public void RestorePurchases(CustomerInfoFunc callback)
    {
        RestorePurchasesCallback = callback;
        _wrapper.RestorePurchases();
    }

    [Obsolete("Deprecated, use set<NetworkId> methods instead.")]
    public void AddAttributionData(string dataJson, AttributionNetwork network, string networkUserId = null)
    {
        _wrapper.AddAttributionData((int)network, dataJson, networkUserId);
    }

    private LogInFunc LogInCallback { get; set; }

    public void LogIn(string appUserId, LogInFunc callback)
    {
        LogInCallback = callback;
        _wrapper.LogIn(appUserId);
    }    
    
    private CustomerInfoFunc LogOutCallback { get; set; }

    public void LogOut(CustomerInfoFunc callback)
    {
        LogOutCallback = callback;
        _wrapper.LogOut();
    }
    
    // ReSharper disable once UnusedMember.Global
    public void SetFinishTransactions(bool finishTransactions)
    {
        _wrapper.SetFinishTransactions(finishTransactions);
    }

    // ReSharper disable once UnusedMember.Global
    [Obsolete("Deprecated, configure behavior through the RevenueCat Dashboard instead.")]
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
    public bool IsAnonymous()
    {
        return _wrapper.IsAnonymous();
    }

    // ReSharper disable once UnusedMember.Global
    public void SetDebugLogsEnabled(bool logsEnabled)
    {
        _wrapper.SetDebugLogsEnabled(logsEnabled);
    }
    
    private CustomerInfoFunc GetCustomerInfoCallback { get; set; }

    // ReSharper disable once UnusedMember.Global
    public void GetCustomerInfo(CustomerInfoFunc callback)
    {
        GetCustomerInfoCallback = callback;
        _wrapper.GetCustomerInfo();
    }

    private GetOfferingsFunc GetOfferingsCallback { get; set; }

    public void GetOfferings(GetOfferingsFunc callback)
    {
        GetOfferingsCallback = callback;
        _wrapper.GetOfferings();
    }

    public void SyncPurchases()
    {
        _wrapper.SyncPurchases();
    }

    // ReSharper disable once UnusedMember.Global
    public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
    {
        _wrapper.SetAutomaticAppleSearchAdsAttributionCollection(enabled);
    }
    private CheckTrialOrIntroductoryPriceEligibilityFunc CheckTrialOrIntroductoryPriceEligibilityCallback { get; set; }
    public void CheckTrialOrIntroductoryPriceEligibility(string[] products, CheckTrialOrIntroductoryPriceEligibilityFunc callback)
    {
        CheckTrialOrIntroductoryPriceEligibilityCallback = callback;
        _wrapper.CheckTrialOrIntroductoryPriceEligibility(products);
    }

    public void InvalidateCustomerInfoCache()
    {
        _wrapper.InvalidateCustomerInfoCache();
    }
    
    public void PresentCodeRedemptionSheet()
    {
        _wrapper.PresentCodeRedemptionSheet();
    }

    /**
      * iOS only.
      * Set this property to true *only* when testing the ask-to-buy / SCA purchases flow.
      * More information: http://errors.rev.cat/ask-to-buy
      */
    public void SetSimulatesAskToBuyInSandbox(bool enabled)
    {
        _wrapper.SetSimulatesAskToBuyInSandbox(enabled);
    }

    public void SetAttributes(Dictionary<string, string> attributes)
    {
        var jsonObject = new JSONObject();
        foreach (var keyValuePair in attributes)
        {
            if (keyValuePair.Value == null)
            {
                jsonObject[keyValuePair.Key] = JSONNull.CreateOrGet();
            }
            else
            {
                jsonObject[keyValuePair.Key] = keyValuePair.Value;
            }
        }
        _wrapper.SetAttributes(jsonObject.ToString());
    }

    public void SetEmail(string email)
    {
        _wrapper.SetEmail(email);
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        _wrapper.SetPhoneNumber(phoneNumber);
    }

    public void SetDisplayName(string displayName)
    {
        _wrapper.SetDisplayName(displayName);
    }

    public void SetPushToken(string token)
    {
        _wrapper.SetPushToken(token);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the Adjust Id for the user.
     * Required for the RevenueCat Adjust integration
     * </summary>
     * <param name="adjustID">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetAdjustID(string adjustID)
    {
        _wrapper.SetAdjustID(adjustID);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the Appsflyer Id for the user
     * Required for the RevenueCat Appsflyer integration
     * </summary>
     * <param name="appsflyerID">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetAppsflyerID(string appsflyerID)
    {
        _wrapper.SetAppsflyerID(appsflyerID);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the Facebook SDK Anonymous Id for the user
     * Required for the RevenueCat Facebook integration
     * </summary>
     * <param name="fbAnonymousID">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetFBAnonymousID(string fbAnonymousID)
    {
        _wrapper.SetFBAnonymousID(fbAnonymousID);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the mParticle Id for the user
     * Required for the RevenueCat mParticle integration
     * </summary>
     * <param name="mparticleID">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetMparticleID(string mparticleID)
    {
        _wrapper.SetMparticleID(mparticleID);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the OneSignal Player Id for the user
     * Required for the RevenueCat OneSignal integration
     * </summary>
     * <param name="onesignalID">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetOnesignalID(string onesignalID)
    {
        _wrapper.SetOnesignalID(onesignalID);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the Airship Channel Id for the user
     * Required for the RevenueCat Airship integration
     * </summary>
     * <param name="airshipChannelID">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetAirshipChannelID(string airshipChannelID)
    {
        _wrapper.SetAirshipChannelID(airshipChannelID);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the install media source for the user
     * </summary>
     * <param name="mediaSource">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetMediaSource(string mediaSource)
    {
        _wrapper.SetMediaSource(mediaSource);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the install campaign for the user
     * </summary>
     * <param name="campaign">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetCampaign(string campaign)
    {
        _wrapper.SetCampaign(campaign);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the install ad group for the user
     * </summary>
     * <param name="adGroup">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetAdGroup(string adGroup)
    {
        _wrapper.SetAdGroup(adGroup);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the install ad for the user
     * </summary>
     * <param name="ad">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetAd(string ad)
    {
        _wrapper.SetAd(ad);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the install keyword for the user
     * </summary>
     * <param name="keyword">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetKeyword(string keyword)
    {
        _wrapper.SetKeyword(keyword);
    }

    /**
     * <summary>
     * Subscriber attribute associated with the install creative for the user
     * </summary>
     * <param name="creative">Empty String or null will delete the subscriber attribute.</param>
     */
    public void SetCreative(string creative)
    {
        _wrapper.SetCreative(creative);
    }

    /**
     * <summary>
     * Automatically collect subscriber attributes associated with the device identifiers.
     * $idfa, $idfv, $ip on iOS
     * $gpsAdId, $androidId, $ip on Android
     * </summary>
     */
    public void CollectDeviceIdentifiers()
    {
        _wrapper.CollectDeviceIdentifiers();
    }
    
    private CanMakePaymentsFunc CanMakePaymentsCallback { get; set; }

     /// <summary>
     /// Check if billing is supported for the current user (meaning IN-APP purchases are supported)
     /// and whether a list of specified feature types are supported.
     ///
     /// Note: BillingFeatures are only relevant to Google Play Android users.
     /// For other stores and platforms, BillingFeatures won't be checked.
     /// </summary>
     /// <param name="features">An array of BillingFeatures to check for support.
     /// If empty, no features will be checked.</param>
     /// <param name="callback">A callback receiving a bool for canMakePayments and potentially an Error</param>
     public void CanMakePayments(BillingFeature[] features, CanMakePaymentsFunc callback) {
         CanMakePaymentsCallback = callback;
         _wrapper.CanMakePayments(features == null ? new BillingFeature[] { } : features);
     }
    
     /// <summary>
     /// Check if billing is supported for the current user (meaning IN-APP purchases are supported)
     /// </summary>
     /// <param name="callback">A callback receiving a bool for canMakePayments and potentially an Error</param>
    public void CanMakePayments(CanMakePaymentsFunc callback)
    {
        CanMakePayments(new BillingFeature[] { }, callback);
    }
     
     private GetPromotionalOfferFunc GetPromotionalOfferCallback { get; set; }

     /// <summary>
     /// iOS only. Use this function to retrieve the Purchases.PromotionalOffer for a given Purchases.Package.
     /// </summary>
     /// <param name="storeProduct">The Purchases.StoreProduct the user intends to purchase</param>
     /// <param name="discount">The Purchases.Discount to apply to the product.</param>
     /// <param name="callback">A callback receiving a Purchases.PromotionalOffer. Null is returned for Android and
     /// incompatible iOS versions.</param>
     public void GetPromotionalOffer(StoreProduct storeProduct, Discount discount, GetPromotionalOfferFunc callback)
     {
        GetPromotionalOfferCallback = callback;
         _wrapper.GetPromotionalOffer(storeProduct.Identifier, discount.Identifier);
     }
     
    // ReSharper disable once UnusedMember.Local
    private void _receiveProducts(string productsJson)
    {
        Debug.Log("_receiveProducts " + productsJson);

        if (ProductsCallback == null) return;
        
        var response = JSON.Parse(productsJson);

        if (ResponseHasError(response))
        {
            ProductsCallback(null, new Error(response["error"]));
        }
        else
        {
            var products = new List<StoreProduct>();
            foreach (JSONNode productResponse in response["products"])
            {
                var product = new StoreProduct(productResponse);
                products.Add(product);
            }

            ProductsCallback(products, null);
        }
        ProductsCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _getCustomerInfo(string customerInfoJson)
    {
        Debug.Log("_getCustomerInfo " + customerInfoJson);
        ReceiveCustomerInfoMethod(customerInfoJson, GetCustomerInfoCallback);
        GetCustomerInfoCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _makePurchase(string makePurchaseResponseJson)
    {
        Debug.Log("_makePurchase " + makePurchaseResponseJson);

        if (MakePurchaseCallback == null) return;

        var response = JSON.Parse(makePurchaseResponseJson);

        if (ResponseHasError(response))
        {
            MakePurchaseCallback(null, null, response["userCancelled"],
                new Error(response["error"]));
        }
        else
        {
            var info = new CustomerInfo(response["customerInfo"]);
            var productIdentifier = response["productIdentifier"];
            MakePurchaseCallback(productIdentifier, info, false, null);
        }

        MakePurchaseCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _receiveCustomerInfo(string customerInfoJson)
    {
        Debug.Log("_receiveCustomerInfo " + customerInfoJson);

        if (listener == null) return;

        var response = JSON.Parse(customerInfoJson);
        if (response["customerInfo"] == null) return;
        var info = new CustomerInfo(response["customerInfo"]);
        listener.CustomerInfoReceived(info);
    }

    // ReSharper disable once UnusedMember.Local
    private void _restorePurchases(string customerInfoJson)
    {
        Debug.Log("_restorePurchases " + customerInfoJson);
        ReceiveCustomerInfoMethod(customerInfoJson, RestorePurchasesCallback);
        RestorePurchasesCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _logIn(string logInResultJson)
    {
        Debug.Log("_logIn " + logInResultJson);
        ReceiveLogInResultMethod(logInResultJson, LogInCallback);
        LogInCallback = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _logOut(string customerInfoJson)
    {
        Debug.Log("_logOut " + customerInfoJson);
        ReceiveCustomerInfoMethod(customerInfoJson, LogOutCallback);
        LogOutCallback = null;
    }

    // ReSharper disable once UnusedMember.Local 
    private void _getOfferings(string offeringsJson)
    {
        Debug.Log("_getOfferings " + offeringsJson);
        if (GetOfferingsCallback == null) return;
        var response = JSON.Parse(offeringsJson);
        if (ResponseHasError(response))
        {
            GetOfferingsCallback(null, new Error(response["error"]));
        }
        else
        {
            var offeringsResponse = response["offerings"];
            GetOfferingsCallback(new Offerings(offeringsResponse), null);
        }
        GetOfferingsCallback = null;
    }
    
    private void _checkTrialOrIntroductoryPriceEligibility(string json)
    {
        Debug.Log("_checkTrialOrIntroductoryPriceEligibility " + json);

        if (CheckTrialOrIntroductoryPriceEligibilityCallback == null) return;

        var response = JSON.Parse(json);
        var dictionary = new Dictionary<string, IntroEligibility>();
        foreach (var keyValuePair in response)
        {
            dictionary[keyValuePair.Key] = new IntroEligibility(keyValuePair.Value);
        }
        
        CheckTrialOrIntroductoryPriceEligibilityCallback(dictionary);

        CheckTrialOrIntroductoryPriceEligibilityCallback = null;
    }

    private void _canMakePayments(string canMakePaymentsJson)
    {
        Debug.Log("_canMakePayments" + canMakePaymentsJson);

        if (CanMakePaymentsCallback == null) return;
        
        var response = JSON.Parse(canMakePaymentsJson);

        if (ResponseHasError(response))
        {
            CanMakePaymentsCallback(false, new Error(response["error"]));
        }
        else
        {
            var canMakePayments = response["canMakePayments"];
            CanMakePaymentsCallback(canMakePayments, null);
        }
        CanMakePaymentsCallback = null;
    }

    private void _getPromotionalOffer(string getPromotionalOfferJson)
    {
        Debug.Log("_getPromotionalOffer" + getPromotionalOfferJson);

        if (GetPromotionalOfferCallback == null) return;
        
        var response = JSON.Parse(getPromotionalOfferJson);

        if (ResponseHasError(response))
        {
            GetPromotionalOfferCallback(null, new Error(response["error"]));
        }
        else
        {
            var promotionalOffer = new PromotionalOffer(response);
            GetPromotionalOfferCallback(promotionalOffer, null);
        }
        GetPromotionalOfferCallback = null;
    }

    private static void ReceiveCustomerInfoMethod(string arguments, CustomerInfoFunc callback)
    {
        if (callback == null) return;

        var response = JSON.Parse(arguments);
        
        if (ResponseHasError(response))
        {
            callback(null, new Error(response["error"]));
        }
        else
        {
            var info = new CustomerInfo(response["customerInfo"]); 
            callback(info, null);
        }
    }

    private static void ReceiveLogInResultMethod(string arguments, LogInFunc callback)
    {
        if (callback == null) return;

        var response = JSON.Parse(arguments);
        
        if (ResponseHasError(response))
        {
            callback(null, false, new Error(response["error"]));
        }
        else
        {
            var info = new CustomerInfo(response["customerInfo"]); 
            var created = response["created"];
            callback(info, created, null);
        }
    }

    private static bool ResponseHasError(JSONNode response)
    {
        return response != null && response.HasKey("error") && response["error"] != null && !response["error"].IsNull;
    }

}

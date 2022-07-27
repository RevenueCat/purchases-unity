using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;
using RevenueCat.SimpleJSON;

#pragma warning disable CS0649

public partial class Purchases : MonoBehaviour
{
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

    [Tooltip("Activate if you plan to call Purchases.Configure or Purchases.Setup programmatically.")]
    public bool useRuntimeSetup;

    [Tooltip("RevenueCat API Key specifically for Apple platforms.\nGet from https://app.revenuecat.com/ \n" +
             "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
             "it through PurchasesConfiguration instead")]
    public string revenueCatAPIKeyApple;

    [Tooltip("RevenueCat API Key specifically for Google Play.\nGet from https://app.revenuecat.com/ \n" +
             "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
             "it through PurchasesConfiguration instead")]
    public string revenueCatAPIKeyGoogle;

    [Header("Alternative Stores")]
    [Tooltip("RevenueCat API Key specifically for Amazon Appstore.\nGet from https://app.revenuecat.com/ \n" +
             "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
             "it through PurchasesConfiguration instead")]
    public string revenueCatAPIKeyAmazon;

    [Tooltip("Enables Amazon Store support. Android only, on iOS it has no effect.\n" +
             "If enabled, it will use the API key in RevenueCatAPIKeyAmazon.\n" +
             "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
             "it through PurchasesConfiguration instead")]
    public bool useAmazon;

    [Header("Dangerous Settings")]
    [Tooltip("Disable or enable automatically detecting current subscriptions.\n" +
             "If this is disabled, RevenueCat won't check current purchases, and it will not sync any purchase automatically " +
             "when the app starts.\nCall syncPurchases whenever a new purchase is detected so the receipt is sent to " +
             "RevenueCat's backend.\n" +
             "In iOS, consumables disappear from the receipt after the transaction is finished, so make sure purchases " +
             "are synced before finishing any consumable transaction, otherwise RevenueCat won't register the purchase.\n" +
             "Auto syncing of purchases is enabled by default.\n" +
             "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
             "it through PurchasesConfiguration instead")]
    public bool autoSyncPurchases = true;

    [Tooltip("App user id. Pass in your own ID if your app has accounts.\n" +
             "If blank, RevenueCat will generate a user ID for you.\n" +
             "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
             "it through PurchasesConfiguration instead")]
    // ReSharper disable once InconsistentNaming
    public string appUserID;

    [Tooltip("List of product identifiers.")]
    public string[] productIdentifiers;

    [Tooltip("A subclass of Purchases.UpdatedCustomerInfoListener component.\n" +
             "Use your custom subclass to define how to handle updated customer information.")]
    public UpdatedCustomerInfoListener listener;

    [Tooltip("An optional boolean. Set this to true if you have your own IAP implementation " +
             "and want to use only RevenueCat's backend.\nDefault is false.\n" +
             "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
             "it through PurchasesConfiguration instead")]
    public bool observerMode;

    [Tooltip("An optional string. iOS only.\n" +
             "Set this to use a specific NSUserDefaults suite for RevenueCat. " +
             "This might be handy if you are deleting all NSUserDefaults in your app " +
             "and leaving RevenueCat in a bad state.\n" +
             "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
             "it through PurchasesConfiguration instead")]
    public string userDefaultsSuiteName;

    [Header("Advanced")]
    [Tooltip("Set this property to your proxy URL before configuring Purchases *only* if you've received " +
             "a proxy key value from your RevenueCat contact.\n" +
             "NOTE: This value will be ignored if \"Use Runtime Setup\" is true. For Runtime Setup, you can configure " +
             "it through PurchasesConfiguration instead")]
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

        Configure(string.IsNullOrEmpty(appUserID) ? null : appUserID);
        GetProducts(productIdentifiers, null);
    }

    private void Configure(string newUserId)
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

        Configure(builder.Build());
    }

    [Obsolete("Deprecated, use Configure instead.", false)]
    public void Setup(PurchasesConfiguration purchasesConfiguration)
    {
        Configure(purchasesConfiguration);
    }
    
    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Use this method to configure the SDK programmatically.
    /// To use this, you *must* check <c>useRuntimeSetup</c> in the Unity IDE UI. 
    /// The values used through this setup will override values set through the Unity IDE UI.
    /// <see cref="useRuntimeSetup"/>
    /// To configure the SDK programmatically:
    /// Create a configuration builder, set its properties, then call `Build` to obtain the configuration.
    /// Lastly, call Purchases.Configure and with the obtained PurchasesConfiguration object.
    /// </summary>
    ///
    /// <example>
    /// For example:
    /// <code>
    /// Purchases.PurchasesConfiguration.Builder builder = Purchases.PurchasesConfiguration.Builder.Init("api_key");
    /// Purchases.PurchasesConfiguration purchasesConfiguration =
    ///     builder
    ///         .SetAppUserId(appUserId)
    ///         .Build();
    /// purchases.Configure(purchasesConfiguration);
    /// </code>
    /// </example>
    /// 
    public void Configure(PurchasesConfiguration purchasesConfiguration)
    {
        var dangerousSettings = purchasesConfiguration.DangerousSettings.Serialize().ToString();
        _wrapper.Setup(gameObject.name, purchasesConfiguration.ApiKey, purchasesConfiguration.AppUserId,
            purchasesConfiguration.ObserverMode, purchasesConfiguration.UserDefaultsSuiteName,
            purchasesConfiguration.UseAmazon, dangerousSettings);
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
    
    /// <summary>
    /// Callback type for <see cref="Purchases.GetProducts"/>.
    /// Includes a list of products or an error.
    /// </summary>
    public delegate void GetProductsFunc(List<StoreProduct> products, Error error);

    private GetProductsFunc ProductsCallback { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    /// <summary>
    /// Fetches the <c>StoreProducts</c> for your IAPs for given productIdentifiers.
    /// This method is called automatically with products pre-configured through Unity IDE UI.
    /// You can optionally call this if you want to fetch more products.
    /// </summary>
    /// <seealso cref="StoreProduct"/>
    /// <param name="products">
    /// A set of product identifiers for in-app purchases setup via AppStoreConnect.\n
    /// This should be either hard coded in your application, from a file, or from a custom endpoint if \n
    /// you want to be able to deploy new IAPs without an app update.
    /// </param>
    /// <param name="callback">
    /// A <see cref="GetProductsFunc"/> callback that is called with the loaded products.\n
    /// If the fetch fails for any reason it will return an empty array and an error.
    /// </param>
    /// <remarks>
    /// completion may be called without StoreProducts that you are expecting.\n
    /// This is usually caused by iTunesConnect configuration errors.\n
    /// Ensure your IAPs have the “Ready to Submit” status in iTunesConnect.\n
    /// Also ensure that you have an active developer program subscription and you have signed the\n
    /// latest paid application agreements.\n
    /// If you’re having trouble, <see href="https://rev.cat/how-to-configure-products"/>
    /// </remarks>
    public void GetProducts(string[] products, GetProductsFunc callback, string type = "subs")
    {
        ProductsCallback = callback;
        _wrapper.GetProducts(products, type);
    }

    /// <summary>
    /// Callback type for methods that make purchases, like <see cref="Purchases.PurchaseProduct"/>,\n
    /// <see cref="Purchases.PurchaseDiscountedProduct"/>, <see cref="Purchases.PurchasePackage"/> and \n
    /// <see cref="Purchases.PurchaseDiscountedPackage"/>.
    /// <param name="productIdentifier"> The product identifier for which the purchase was attempted.</param>
    /// <param name="customerInfo"> The updated <see cref="CustomerInfo"/> object after the successful purchase.</param>
    /// <param name="userCancelled"> A boolean that indicates whether the purchase was cancelled by the user.</param>
    /// <param name="error"> An error, if one occurred. Null if the purchase was successful. </param>
    /// 
    /// </summary>
    public delegate void MakePurchaseFunc(string productIdentifier, CustomerInfo customerInfo, bool userCancelled,
        Error error);
    
    private MakePurchaseFunc MakePurchaseCallback { get; set; }

    ///
    /// <summary>
    /// Initiates a purchase of a <see cref="StoreProduct"/>.
    /// </summary>
    /// Use this function if you are not using the <see cref="Offerings"/> system to purchase a <see cref="StoreProduct"/>.
    /// If you are using the <see cref="Offerings"/> system, use <see cref="PurchasePackage"/> instead.
    ///
    /// <remarks>
    /// Call this method when a user has decided to purchase a product.
    /// Only call this in direct response to user input.
    /// </remarks>
    ///
    /// From here the SDK will handle the purchase with <c>StoreKit</c> and call the <c>PurchaseCompletedBlock</c>.
    ///
    /// <remarks>
    /// Note: You do not need to finish the transaction yourself in the completion callback, RevenueCat will
    /// handle this for you.
    /// </remarks>
    ///
    /// <param name="productIdentifier"> The identifier of the <see cref="StoreProduct"/> the user intends to purchase.</param>
    /// <param name="callback"> A <see cref="MakePurchaseCallback"/> completion block that is called when the purchase completes.</param>
    /// <param name="type"> Android only. The type of product to purchase. </param>
    /// <param name="oldSku"> Android only. Optional. The oldSku to upgrade from. </param>
    /// <param name="prorationMode"> Android only. Optional. The <see cref="ProrationMode"/> to use when upgrading the given oldSku. </param>
    ///
    public void PurchaseProduct(string productIdentifier, MakePurchaseFunc callback,
        string type = "subs", string oldSku = null,
        ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchaseProduct(productIdentifier, type, oldSku, prorationMode);
    }

    /// 
    /// <summary>
    /// iOS only. Initiates a purchase of a <see cref="StoreProduct"/> with a <see cref="PromotionalOffer"/>.
    /// You can get a <c>PromotionalOffer</c> by calling <see cref="GetPromotionalOffer"/>.
    /// </summary>
    /// Use this function if you are not using the <see cref="Offerings"/> system to purchase a <see cref="StoreProduct"/>.
    /// If you are using the <see cref="Offerings"/> system, use <see cref="PurchasePackage"/> instead.
    /// 
    /// <remarks>
    /// Call this method when a user has decided to purchase a product.
    /// Only call this in direct response to user input.
    /// </remarks>
    /// 
    /// From here the SDK will handle the purchase with <c>StoreKit</c> and call the <c>PurchaseCompletedBlock</c>.
    /// 
    /// <remarks>
    /// Note: You do not need to finish the transaction yourself in the completion callback, RevenueCat will
    /// handle this for you.
    /// </remarks>
    /// 
    /// <param name="productIdentifier"> The identifier of the <see cref="StoreProduct"/> the user intends to purchase.</param>
    /// <param name="discount"> A <see cref="PromotionalOffer"/> to apply to the purchase.</param>
    /// <param name="callback"> A <see cref="MakePurchaseCallback"/> completion block that is called when the purchase completes.</param>
    public void PurchaseDiscountedProduct(string productIdentifier, PromotionalOffer discount,
        MakePurchaseFunc callback)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchaseProduct(productIdentifier, discount: discount);
    }

    ///
    /// <summary>
    /// Initiates a purchase of a <see cref="Package"/>.
    /// </summary>
    ///
    /// <remarks>
    /// Call this method when a user has decided to purchase a product.
    /// Only call this in direct response to user input.
    /// </remarks>
    ///
    /// From here the SDK will handle the purchase with <c>StoreKit</c> and call the <c>PurchaseCompletedBlock</c>.
    ///
    /// <remarks>
    /// Note: You do not need to finish the transaction yourself in the completion callback, RevenueCat will
    /// handle this for you.
    /// </remarks>
    ///
    /// <param name="package"> The <see cref="Package"/> the user intends to purchase.</param>
    /// <param name="callback"> A <see cref="MakePurchaseCallback"/> completion block that is called when the purchase completes.</param>
    /// <param name="oldSku"> Android only. Optional. The oldSku to upgrade from. </param>
    /// <param name="prorationMode"> Android only. Optional. The <see cref="ProrationMode"/> to use when upgrading the given oldSku. </param>
    ///
    public void PurchasePackage(Package package, MakePurchaseFunc callback, string oldSku = null,
        ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchasePackage(package, oldSku, prorationMode);
    }

    ///
    /// <summary>
    /// iOS only. Initiates a purchase of a <see cref="Package"/>.
    /// You can get a <c>PromotionalOffer</c> by calling <see cref="GetPromotionalOffer"/>.
    /// </summary>
    ///
    /// <remarks>
    /// Call this method when a user has decided to purchase a product.
    /// Only call this in direct response to user input.
    /// </remarks>
    ///
    /// From here the SDK will handle the purchase with <c>StoreKit</c> and call the <c>PurchaseCompletedBlock</c>.
    ///
    /// <remarks>
    /// Note: You do not need to finish the transaction yourself in the completion callback, RevenueCat will
    /// handle this for you.
    /// </remarks>
    ///
    /// <param name="package"> The <see cref="Package"/> the user intends to purchase.</param>
    /// <param name="discount"> A <see cref="PromotionalOffer"/> to apply to the purchase.</param>
    /// <param name="callback"> A <see cref="MakePurchaseCallback"/> completion block that is called when the purchase completes.</param>
    ///
    public void PurchaseDiscountedPackage(Package package, PromotionalOffer discount, MakePurchaseFunc callback)
    {
        MakePurchaseCallback = callback;
        _wrapper.PurchasePackage(package, discount: discount);
    }

    /// <summary>
    /// Callback type for methods that return <see cref="CustomerInfo"/>.
    /// Includes a <see cref="CustomerInfo"/> or an error.
    /// </summary>
    /// <param name="customerInfo"> A <see cref="CustomerInfo"/> if the request was successful, null otherwise. </param>
    /// <param name="error"> An error if the request was not successful, null otherwise. </param>
    public delegate void CustomerInfoFunc(CustomerInfo customerInfo, Error error);
    private CustomerInfoFunc RestorePurchasesCallback { get; set; }

    /// <summary>
    /// 
    /// This method will post all purchases associated with the current Store account to RevenueCat and become
    /// associated with the current <c>appUserID</c>. If the receipt is being used by an existing user, the current
    /// <c>appUserID</c> will be aliased together with the <c>appUserID</c> of the existing user.
    ///  Going forward, either <c>appUserID</c> will be able to reference the same user.
    /// </summary>
    ///
    /// You shouldn't use this method if you have your own account system. In that case "restoration" is provided
    /// by your app passing the same <c>appUserID</c> used to purchase originally.
    ///
    /// - Note: This may force your users to enter their Store password so should only be performed on request of
    /// the user. Typically with a button in settings or near your purchase UI. Use
    /// <see cref="SyncPurchases"/> if you need to restore transactions programmatically.
    ///
    /// <param name="callback"> A <see cref="CustomerInfoFunc"/> which will contain a <see cref="CustomerInfo"/>
    /// if restoration was successful, or an error otherwise. </param>
    public void RestorePurchases(CustomerInfoFunc callback)
    {
        RestorePurchasesCallback = callback;
        _wrapper.RestorePurchases();
    }

    [Obsolete("Deprecated, use set<NetworkId> methods instead.", true)]
    public void AddAttributionData(string dataJson, AttributionNetwork network, string networkUserId = null) { }

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

    /// <summary>
    /// Android only. Noop in iOS.
    /// 
    /// This method will send a purchase to the RevenueCat backend. This function should only be called if you are
    /// in Amazon observer mode or performing a client side migration of your current users to RevenueCat.
    /// The receipt IDs are cached if successfully posted so they are not posted more than once.
    /// </summary>
    /// <param name="productID">Product ID associated to the purchase.</param>
    /// <param name="receiptID"> ReceiptId that represents the Amazon purchase.</param>
    /// <param name="amazonUserID">Amazon's userID.</param>
    /// <param name="isoCurrencyCode">Product's currency code in ISO 4217 format.</param>
    /// <param name="price">Product's price.</param>
    public void SyncObserverModeAmazonPurchase(string productID, string receiptID, string amazonUserID,
        string isoCurrencyCode, double price)
    {
        _wrapper.SyncObserverModeAmazonPurchase(productID, receiptID, amazonUserID, isoCurrencyCode, price);
    }

    // ReSharper disable once UnusedMember.Global
    public void SetAutomaticAppleSearchAdsAttributionCollection(bool enabled)
    {
        _wrapper.SetAutomaticAppleSearchAdsAttributionCollection(enabled);
    }

    private CheckTrialOrIntroductoryPriceEligibilityFunc CheckTrialOrIntroductoryPriceEligibilityCallback { get; set; }

    public void CheckTrialOrIntroductoryPriceEligibility(string[] products,
        CheckTrialOrIntroductoryPriceEligibilityFunc callback)
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
    public void CanMakePayments(BillingFeature[] features, CanMakePaymentsFunc callback)
    {
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
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using RevenueCat;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class Purchases : MonoBehaviour
//{
//    private IPurchasesWrapper _wrapper = new PurchasesWrapperNoop();

//    /// <summary>
//    /// Callback type for <see cref="Purchases.GetStorefront"/>.
//    /// Includes the info of the current store account.
//    /// </summary>
//    public delegate void GetStorefrontFunc(Storefront storefront);

//    private GetStorefrontFunc StorefrontCallback { get; set; }

//    /// <summary>
//    /// Fetches the <c>Storefront</c> for the customer's current store account.
//    /// If there is an error, the callback will be called with a null value.
//    /// </summary>
//    public void GetStorefront(GetStorefrontFunc callback)
//    {
//        StorefrontCallback = callback;
//        _wrapper.GetStorefrontAsync();
//    }

//    /// <summary>
//    /// Callback type for <see cref="Purchases.GetProducts"/>.
//    /// Includes a list of products or an error.
//    /// </summary>
//    public delegate void GetProductsFunc(List<StoreProduct> products, Error error);

//    private GetProductsFunc ProductsCallback { get; set; }

//    // ReSharper disable once MemberCanBePrivate.Global
//    /// <summary>
//    /// Fetches the <c>StoreProducts</c> for your IAPs for given productIdentifiers.
//    /// This method is called automatically with products pre-configured through Unity IDE UI.
//    /// You can optionally call this if you want to fetch more products.
//    /// </summary>
//    /// <seealso cref="StoreProduct"/>
//    /// <param name="products">
//    /// A set of product identifiers for in-app purchases setup via AppStoreConnect.\n
//    /// This should be either hard coded in your application, from a file, or from a custom endpoint if \n
//    /// you want to be able to deploy new IAPs without an app update.
//    /// </param>
//    /// <param name="callback">
//    /// A <see cref="GetProductsFunc"/> callback that is called with the loaded products.\n
//    /// If the fetch fails for any reason it will return an empty array and an error.
//    /// </param>
//    /// <param name="type"> Android only. The type of product to purchase. </param>
//    /// <remarks>
//    /// completion may be called without <see cref="StoreProduct"/>s that you are expecting.\n
//    /// This is usually caused by iTunesConnect configuration errors.\n
//    /// Ensure your IAPs have the “Ready to Submit” status in iTunesConnect.\n
//    /// Also ensure that you have an active developer program subscription and you have signed the\n
//    /// latest paid application agreements.\n
//    /// If you’re having trouble, <see href="https://rev.cat/how-to-configure-products"/>
//    /// </remarks>
//    public void GetProducts(string[] products, GetProductsFunc callback, string type = "subs")
//    {
//        ProductsCallback = callback;
//        _wrapper.GetProductsAsync(products, type);
//    }

//    /// <summary>
//    /// Callback type for methods that make purchases, like <see cref="Purchases.PurchaseProduct"/>,\n
//    /// <see cref="Purchases.PurchaseDiscountedProduct"/>, <see cref="Purchases.PurchasePackage"/>, \n
//    /// <see cref="Purchases.PurchaseDiscountedPackage"/>, <see cref="Purchases.PurchaseProductWithWinBackOffer"/>, \n
//    /// and <see cref="Purchases.PurchasePackageWithWinBackOffer"/>.
//    /// </summary>
//    ///
//    /// <param name="purchaseResult"> The <see cref="PurchaseResult"/> object for the purchase attempt that just happened.</param>
//    public delegate void MakePurchaseFunc(PurchaseResult purchaseResult);

//    private MakePurchaseFunc MakePurchaseCallback { get; set; }

//    ///
//    /// <summary>
//    /// Initiates a purchase of a <see cref="StoreProduct"/>.
//    /// </summary>
//    /// Use this function if you are not using the <see cref="Offerings"/> system to purchase a <see cref="StoreProduct"/>.
//    /// If you are using the <see cref="Offerings"/> system, use <see cref="PurchasePackage"/> instead.
//    ///
//    /// <remarks>
//    /// Call this method when a user has decided to purchase a product.
//    /// Only call this in direct response to user input.
//    /// </remarks>
//    ///
//    /// From here the SDK will handle the purchase with <c>StoreKit</c> and call the <c>PurchaseCompletedBlock</c>.
//    ///
//    /// <remarks>
//    /// Note: You do not need to finish the transaction yourself in the completion callback, RevenueCat will
//    /// handle this for you.
//    /// </remarks>
//    ///
//    /// <param name="productIdentifier"> The identifier of the <see cref="StoreProduct"/> the user intends to purchase.</param>
//    /// <param name="callback"> A <see cref="MakePurchaseCallback"/> completion block that is called when the purchase completes.</param>
//    /// <param name="type"> Android only. The type of product to purchase. </param>
//    /// <param name="oldSku"> Android only. Optional. The oldSku to upgrade from. </param>
//    /// <param name="prorationMode"> Android only. Optional. The <see cref="ProrationMode"/> to use when upgrading the given oldSku. </param>
//    /// <param name="googleIsPersonalizedPrice"> Android only. Optional. Indicates
//    /// personalized pricing on products available for purchase in the EU.
//    /// For compliance with EU regulations. User will see "This price has been
//    /// customized for you" in the purchase dialog when true.
//    /// See https://developer.android.com/google/play/billing/integrate#personalized-price
//    /// for more info. </param>
//    ///
//    public void PurchaseProduct(string productIdentifier, MakePurchaseFunc callback,
//        string type = "subs", string oldSku = null,
//        ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
//        bool googleIsPersonalizedPrice = false)
//    {
//        MakePurchaseCallback = callback;
//        _wrapper.PurchaseProductAsync(productIdentifier, type, oldSku, prorationMode, googleIsPersonalizedPrice);
//    }

//    ///
//    /// <summary>
//    /// iOS only. Initiates a purchase of a <see cref="StoreProduct"/> with a <see cref="PromotionalOffer"/>.
//    /// You can get a <c>PromotionalOffer</c> by calling <see cref="GetPromotionalOffer"/>.
//    /// </summary>
//    /// Use this function if you are not using the <see cref="Offerings"/> system to purchase a <see cref="StoreProduct"/>.
//    /// If you are using the <see cref="Offerings"/> system, use <see cref="PurchasePackage"/> instead.
//    ///
//    /// <remarks>
//    /// Call this method when a user has decided to purchase a product.
//    /// Only call this in direct response to user input.
//    /// </remarks>
//    ///
//    /// From here the SDK will handle the purchase with <c>StoreKit</c> and call the <c>PurchaseCompletedBlock</c>.
//    ///
//    /// <remarks>
//    /// Note: You do not need to finish the transaction yourself in the completion callback, RevenueCat will
//    /// handle this for you.
//    /// </remarks>
//    ///
//    /// <param name="productIdentifier"> The identifier of the <see cref="StoreProduct"/> the user intends to purchase.</param>
//    /// <param name="discount"> A <see cref="PromotionalOffer"/> to apply to the purchase.</param>
//    /// <param name="callback"> A <see cref="MakePurchaseCallback"/> completion block that is called when the purchase completes.</param>
//    public void PurchaseDiscountedProduct(string productIdentifier, PromotionalOffer discount, MakePurchaseFunc callback)
//    {
//        MakePurchaseCallback = callback;
//        _wrapper.PurchaseProductAsync(productIdentifier, discount: discount);
//    }

//    ///
//    /// <summary>
//    /// Initiates a purchase of a <see cref="Package"/>.
//    /// </summary>
//    ///
//    /// <remarks>
//    /// Call this method when a user has decided to purchase a product.
//    /// Only call this in direct response to user input.
//    /// </remarks>
//    ///
//    /// From here the SDK will handle the purchase with <c>StoreKit</c> and call the <c>PurchaseCompletedBlock</c>.
//    ///
//    /// <remarks>
//    /// Note: You do not need to finish the transaction yourself in the completion callback, RevenueCat will
//    /// handle this for you.
//    /// </remarks>
//    ///
//    /// <param name="package"> The <see cref="Package"/> the user intends to purchase.</param>
//    /// <param name="callback"> A <see cref="MakePurchaseCallback"/> completion block that is called when the purchase completes.</param>
//    /// <param name="oldSku"> Android only. Optional. The oldSku to upgrade from. </param>
//    /// <param name="prorationMode"> Android only. Optional. The <see cref="ProrationMode"/> to use when upgrading the given oldSku. </param>
//    /// <param name="googleIsPersonalizedPrice"> Android only. Optional. Indicates
//    /// personalized pricing on products available for purchase in the EU.
//    /// For compliance with EU regulations. User will see "This price has been
//    /// customized for you" in the purchase dialog when true.
//    /// See https://developer.android.com/google/play/billing/integrate#personalized-price
//    /// for more info. </param>
//    ///
//    public void PurchasePackage(Package package, MakePurchaseFunc callback, string oldSku = null,
//        ProrationMode prorationMode = ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy,
//        bool googleIsPersonalizedPrice = false)
//    {
//        MakePurchaseCallback = callback;
//        _wrapper.PurchasePackageAsync(package, oldSku, prorationMode, googleIsPersonalizedPrice);
//    }

//    ///
//    /// <summary>
//    /// iOS only. Initiates a purchase of a <see cref="Package"/>.
//    /// You can get a <c>PromotionalOffer</c> by calling <see cref="GetPromotionalOffer"/>.
//    /// </summary>
//    ///
//    /// <remarks>
//    /// Call this method when a user has decided to purchase a product.
//    /// Only call this in direct response to user input.
//    /// </remarks>
//    ///
//    /// From here the SDK will handle the purchase with <c>StoreKit</c> and call the <c>PurchaseCompletedBlock</c>.
//    ///
//    /// <remarks>
//    /// Note: You do not need to finish the transaction yourself in the completion callback, RevenueCat will
//    /// handle this for you.
//    /// </remarks>
//    ///
//    /// <param name="package"> The <see cref="Package"/> the user intends to purchase.</param>
//    /// <param name="discount"> A <see cref="PromotionalOffer"/> to apply to the purchase.</param>
//    /// <param name="callback"> A <see cref="MakePurchaseCallback"/> completion block that is called when the purchase completes.</param>
//    ///
//    public void PurchaseDiscountedPackage(Package package, PromotionalOffer discount, MakePurchaseFunc callback)
//    {
//        MakePurchaseCallback = callback;
//        _wrapper.PurchasePackageAsync(package, discount: discount);
//    }

//    public void PurchaseSubscriptionOption(SubscriptionOption subscriptionOption, MakePurchaseFunc callback,
//        GoogleProductChangeInfo googleProductChangeInfo = null, bool googleIsPersonalizedPrice = false)
//    {
//        MakePurchaseCallback = callback;
//        _wrapper.PurchaseSubscriptionOptionAsync(subscriptionOption, googleProductChangeInfo, googleIsPersonalizedPrice);
//    }

//    /// <summary>
//    /// Callback type for methods that return <see cref="CustomerInfo"/>.
//    /// Includes a <see cref="CustomerInfo"/> or an error.
//    /// </summary>
//    /// <param name="customerInfo"> A <see cref="CustomerInfo"/> if the request was successful, null otherwise. </param>
//    /// <param name="error"> An error if the request was not successful, null otherwise. </param>
//    public delegate void CustomerInfoFunc(CustomerInfo customerInfo, Error error);

//    private CustomerInfoFunc RestorePurchasesCallback { get; set; }

//    /// <summary>
//    /// This method will post all purchases associated with the current Store account to RevenueCat and become
//    /// associated with the current <c>appUserID</c>. If the receipt is being used by an existing user, the current
//    /// <c>appUserID</c> will be aliased together with the <c>appUserID</c> of the existing user.
//    ///  Going forward, either <c>appUserID</c> will be able to reference the same user.
//    /// <seealso href="https://docs.revenuecat.com/docs/restoring-purchases"/>
//    /// </summary>
//    /// <remarks>
//    /// You shouldn't use this method if you have your own account system. In that case "restoration" is provided. <br/>
//    /// Note: This may force your users to enter their Store password so should only be performed on request of
//    /// by your app passing the same <c>appUserID</c> used to purchase originally.
//    /// the user. Typically, with a button in settings or near your purchase UI. Use
//    /// <see cref="SyncPurchases"/> if you need to restore transactions programmatically.
//    /// </remarks>
//    /// <param name="callback"> A <see cref="CustomerInfoFunc"/> which will contain a <see cref="CustomerInfo"/>
//    /// if restoration was successful, or an error otherwise. </param>
//    public void RestorePurchases(CustomerInfoFunc callback)
//    {
//        RestorePurchasesCallback = callback;
//        _wrapper.RestorePurchases();
//    }

//    [Obsolete("Deprecated, use set<NetworkId> methods instead.", true)]
//    public void AddAttributionData(string dataJson, AttributionNetwork network, string networkUserId = null) { }

//    /// <summary>
//    /// Callback function for <see cref="Purchases.LogIn"/>.
//    /// </summary>
//    /// <param name="customerInfo"> The <see cref="CustomerInfo"/> of the user if the request was successful.
//    /// Null otherwise.</param>
//    /// <param name="created"> True if the user was registered in RevenueCat's database for the first time upon
//    /// making this call. False if a user with this <c>appUserID</c> already existed in RevenueCat's database for
//    /// this app.</param>
//    /// <param name="error"> If the request was unsuccessful, contains an error. Null otherwise. </param>
//    public delegate void LogInFunc(CustomerInfo customerInfo, bool created, Error error);

//    private LogInFunc LogInCallback { get; set; }

//    /// <summary>
//    /// This function will log in the current user with an <c>appUserID</c>.
//    /// </summary>
//    /// <remarks>
//    /// RevenueCat provides a source of truth for a subscriber's status across different platforms.
//    /// To do this, each subscriber has an App User ID that uniquely identifies them within your application.
//    /// User identity is one of the most important components of many mobile applications,
//    /// and it's extra important to make sure the subscription status RevenueCat is
//    /// tracking gets associated with the correct user.
//    /// The Purchases SDK allows you to specify your own user identifiers or use anonymous identifiers
//    /// generated by RevenueCat. Some apps will use a combination
//    /// of their own identifiers and RevenueCat anonymous Ids - that's okay!
//    /// </remarks>
//    /// <param name="appUserId"> The <c>appUserID</c> that should be linked to the current user. </param>
//    /// <param name="callback">
//    /// The callback block will be called with the latest <see cref="CustomerInfo"/> and a <c>bool</c> specifying
//    /// whether the user was created for the first time in the RevenueCat backend.
//    /// </param>
//    /// <seealso href="https://docs.revenuecat.com/docs/user-ids"/>
//    /// <seealso cref="LogOut"/>
//    /// <seealso cref="IsAnonymous"/>
//    /// <seealso cref="appUserID"/>
//    public void LogIn(string appUserId, LogInFunc callback)
//    {
//        LogInCallback = callback;
//        _wrapper.LogInAsync(appUserId);
//    }

//    private CustomerInfoFunc LogOutCallback { get; set; }


//    ///
//    /// <summary>
//    /// Logs out the <c>Purchases</c> client, clearing the saved <c>appUserID</c>.
//    /// </summary>
//    /// <remarks>
//    /// This will generate a random user id and save it in the cache.
//    /// If this method is called and the current user is anonymous, it will return an error.
//    /// </remarks>
//    ///
//    /// <param name="callback"> The <see cref="CustomerInfoFunc"/> callback will contain a CustomerInfo if the logOut
//    /// operation was successful, or an error otherwise.</param>
//    ///
//    /// <seealso href="https://docs.revenuecat.com/docs/user-ids"/>
//    /// <seealso cref="LogIn"/>
//    /// <seealso cref="IsAnonymous"/>
//    /// <seealso cref="appUserID"/>
//    ///
//    public void LogOut(CustomerInfoFunc callback)
//    {
//        LogOutCallback = callback;
//        _wrapper.LogOut();
//    }

//    // ReSharper disable once UnusedMember.Global
//    [Obsolete(@"Configure behavior through the RevenueCat dashboard instead. 
//    If you have configured the 'Legacy' restore behavior in the RevenueCat Dashboard
//    and are currently setting this to true, keep this setting active.")]
//    public void SetAllowSharingStoreAccount(bool allow)
//    {
//        _wrapper.SetAllowSharingStoreAccount(allow);
//    }

//    // ReSharper disable once UnusedMember.Global
//    /// <summary>
//    /// The <c>appUserID</c> used by <c>Purchases</c>.
//    /// If not passed on initialization this will be generated and cached by <c>Purchases</c>.
//    /// </summary>
//    /// <returns> The app user ID currently used by <c>Purchases</c>.</returns>
//    public string GetAppUserId()
//    {
//        return _wrapper.GetAppUserId();
//    }

//    // ReSharper disable once UnusedMember.Global
//    /// <summary>
//    /// Returns <c>true</c> if the <c>appUserID</c> has been generated by RevenueCat, <c>false</c> otherwise.
//    /// </summary>
//    public bool IsAnonymous()
//    {
//        return _wrapper.IsAnonymous();
//    }

//    // ReSharper disable once UnusedMember.Global
//    /// <summary>
//    /// Returns <c>true</c> if <c>configure</c> has been called and [Purchases.sharedInstance] is set.
//    /// </summary>
//    public bool IsConfigured()
//    {
//        return _wrapper.IsConfigured();
//    }

//    // ReSharper disable once UnusedMember.Global
//    /// <summary>
//    /// Enable debug logging. Useful for debugging issues with the lovely team @RevenueCat.
//    /// </summary>
//    ///
//    /// <param name="logsEnabled"> Whether debug logs should be enabled.</param>
//    [Obsolete("Deprecated, use logLevel instead.")]
//    public void SetDebugLogsEnabled(bool logsEnabled)
//    {
//        _wrapper.SetDebugLogsEnabled(logsEnabled);
//    }

//    // ReSharper disable once UnusedMember.Global
//    /// <summary>
//    /// Configure log level. Useful for debugging issues with the lovely team @RevenueCat.
//    /// </summary>
//    public void SetLogLevel(LogLevel level)
//    {
//        _wrapper.SetLogLevel(level);
//    }

//    private CustomerInfoFunc GetCustomerInfoCallback { get; set; }

//    // ReSharper disable once UnusedMember.Global
//    /// <summary>
//    /// Get latest available <see cref="CustomerInfo"/>.
//    /// </summary>
//    ///
//    /// <param name="callback"> A completion block called when customer info is available and not stale.
//    /// Called immediately if <see cref="CustomerInfo"/> is cached. Customer info can be nil if an error occurred.
//    /// </param>
//    ///
//    public void GetCustomerInfo(CustomerInfoFunc callback)
//    {
//        GetCustomerInfoCallback = callback;
//        _wrapper.GetCustomerInfoAsync();
//    }

//    /// <summary>
//    /// Callback for <see cref="Purchases.GetOfferings"/>.
//    /// </summary>
//    /// <param name="offerings"> The <see cref="Offerings"/> object if the request was successful, null otherwise.</param>
//    /// <param name="error"> The error if the request was unsuccessful, null otherwise.</param>
//    public delegate void GetOfferingsFunc(Offerings offerings, Error error);

//    private GetOfferingsFunc GetOfferingsCallback { get; set; }

//    /// <summary>
//    /// Callback for <see cref="Purchases.GetCurrentOfferingForPlacement"/>.
//    /// </summary>
//    /// <param name="offerings"> The nullable <see cref="Offering"/> object if the request was successful, null otherwise.</param>
//    /// <param name="error"> The error if the request was unsuccessful, null otherwise.</param>
//    public delegate void GetCurrentOfferingForPlacementFunc(Offering offerings, Error error);

//    private GetCurrentOfferingForPlacementFunc GetCurrentOfferingForPlacementCallback { get; set; }

//    /// <summary>
//    /// Callback for <see cref="Purchases.SyncAttributesAndOfferingsIfNeeded"/>.
//    /// </summary>
//    /// <param name="offerings"> The <see cref="Offerings"/> object if the request was successful, null otherwise.</param>
//    /// <param name="error"> The error if the request was unsuccessful, null otherwise.</param>
//    public delegate void SyncAttributesAndOfferingsIfNeededFunc(Offerings offerings, Error error);

//    private SyncAttributesAndOfferingsIfNeededFunc SyncAttributesAndOfferingsIfNeededCallback { get; set; }

//    ///
//    /// <summary>
//    /// Fetch the configured <see cref="Offerings"/> for this user.
//    /// </summary>
//    ///
//    /// <see cref="Offerings"/> allows you to configure your in-app products
//    /// via RevenueCat and greatly simplifies management.
//    ///
//    /// <see cref="Offerings"/> will be fetched and cached on instantiation so that, by the time they are needed,
//    /// your prices are loaded for your purchase flow. Time is money.
//    ///
//    /// <param name="callback"> A completion block called when offerings are available.
//    /// Called immediately if offerings are cached. <see cref="Offerings"/> will be null if an error occurred.
//    /// </param>
//    ///
//    /// <seealso href="https://docs.revenuecat.com/docs/displaying-products"/>
//    ///
//    public void GetOfferings(GetOfferingsFunc callback)
//    {
//        GetOfferingsCallback = callback;
//        _wrapper.GetOfferingsAsync();
//    }

//    public void GetCurrentOfferingForPlacement(string placementIdentifier, GetCurrentOfferingForPlacementFunc callback)
//    {
//        GetCurrentOfferingForPlacementCallback = callback;
//        _wrapper.GetCurrentOfferingForPlacementAsync(placementIdentifier);
//    }

//    public void SyncAttributesAndOfferingsIfNeeded(SyncAttributesAndOfferingsIfNeededFunc callback)
//    {
//        SyncAttributesAndOfferingsIfNeededCallback = callback;
//        _wrapper.SyncAttributesAndOfferingsIfNeededAsync();
//    }

//    /// <summary>
//    /// This method will post all purchases associated with the current App Store account to RevenueCat and
//    /// become associated with the current <c>appUserID</c>.
//    /// </summary>
//    ///
//    /// If the receipt is being used by an existing user, the current <c>appUserID</c> will be aliased together with
//    /// the <c>appUserID</c> of the existing user.
//    /// Going forward, either <c>appUserID</c> will be able to reference the same user.
//    ///
//    /// <remarks>
//    /// Warning: This function should only be called if you're not calling any purchase method.
//    /// </remarks>
//    ///
//    /// <remarks>
//    /// Note: This method will not trigger a login prompt from App Store. However, if the receipt currently
//    /// on the device does not contain subscriptions, but the user has made subscription purchases, this method
//    /// won't be able to restore them. Use <see cref="RestorePurchases"/> to cover those cases.
//    /// </remarks>
//    /// <seealso href="https://docs.revenuecat.com/docs/restoring-purchases"/>
//    ///
//    public void SyncPurchases()
//    {
//        _wrapper.SyncPurchases();
//    }

//    private CustomerInfoFunc SyncPurchasesCallback { get; set; }

//    /// <summary>
//    /// This method will post all purchases associated with the current App Store account to RevenueCat and
//    /// become associated with the current <c>appUserID</c>.
//    /// </summary>
//    ///
//    /// If the receipt is being used by an existing user, the current <c>appUserID</c> will be aliased together with
//    /// the <c>appUserID</c> of the existing user.
//    /// Going forward, either <c>appUserID</c> will be able to reference the same user.
//    ///
//    /// <remarks>
//    /// Warning: This function should only be called if you're not calling any purchase method.
//    /// </remarks>
//    ///
//    /// <remarks>
//    /// Note: This method will not trigger a login prompt from App Store. However, if the receipt currently
//    /// on the device does not contain subscriptions, but the user has made subscription purchases, this method
//    /// won't be able to restore them. Use <see cref="RestorePurchases"/> to cover those cases.
//    /// </remarks>
//    /// <seealso href="https://docs.revenuecat.com/docs/restoring-purchases"/>
//    ///
//    /// <param name="callback"> A <see cref="CustomerInfoFunc"/> which will contain a <see cref="CustomerInfo"/>
//    /// if sync was successful, or an error otherwise. </param>
//    public void SyncPurchases(CustomerInfoFunc callback)
//    {
//        SyncPurchasesCallback = callback;
//        _wrapper.SyncPurchases();
//    }

//    /// <summary>
//    /// Android only. Noop in iOS.
//    ///
//    /// This method will send a purchase to the RevenueCat backend. This function should only be called if you are
//    /// in Amazon observer mode or performing a client side migration of your current users to RevenueCat.
//    /// The receipt IDs are cached if successfully posted so they are not posted more than once.
//    /// </summary>
//    /// <param name="productID">Product ID associated to the purchase.</param>
//    /// <param name="receiptID"> ReceiptId that represents the Amazon purchase.</param>
//    /// <param name="amazonUserID">Amazon's userID.</param>
//    /// <param name="isoCurrencyCode">Product's currency code in ISO 4217 format.</param>
//    /// <param name="price">Product's price.</param>
//    [Obsolete("Deprecated, use SyncAmazonPurchase instead.")]
//    public void SyncObserverModeAmazonPurchase(string productID, string receiptID, string amazonUserID,
//        string isoCurrencyCode, double price)
//    {
//        _wrapper.SyncAmazonPurchase(productID, receiptID, amazonUserID, isoCurrencyCode, price);
//    }

//    /// <summary>
//    /// Android only. Noop in iOS.
//    ///
//    /// This method will send a purchase to the RevenueCat backend. This function should only be called if you are
//    /// in Amazon observer mode or performing a client side migration of your current users to RevenueCat.
//    /// The receipt IDs are cached if successfully posted so they are not posted more than once.
//    /// </summary>
//    /// <param name="productID">Product ID associated to the purchase.</param>
//    /// <param name="receiptID"> ReceiptId that represents the Amazon purchase.</param>
//    /// <param name="amazonUserID">Amazon's userID.</param>
//    /// <param name="isoCurrencyCode">Product's currency code in ISO 4217 format.</param>
//    /// <param name="price">Product's price.</param>
//    public void SyncAmazonPurchase(string productID, string receiptID, string amazonUserID,
//        string isoCurrencyCode, double price)
//    {
//        _wrapper.SyncAmazonPurchase(productID, receiptID, amazonUserID, isoCurrencyCode, price);
//    }

//    // ReSharper disable once UnusedMember.Global
//    /// <summary>
//    /// Enable automatic collection of Apple Search Ads attribution using AdServices. Defaults to `false`.
//    /// </summary>
//    public void EnableAdServicesAttributionTokenCollection()
//    {
//        _wrapper.EnableAdServicesAttributionTokenCollection();
//    }

//    /// <summary>
//    /// iOS only. Callback for the <see cref="Purchases.CheckTrialOrIntroductoryPriceEligibility"/> method.
//    /// </summary>
//    /// <param name="products"> A Dictionary mapping product identifiers to their eligibility status,
//    /// as <see cref="IntroEligibility"/> objects.</param>
//    public delegate void CheckTrialOrIntroductoryPriceEligibilityFunc(Dictionary<string, IntroEligibility> products);

//    private CheckTrialOrIntroductoryPriceEligibilityFunc CheckTrialOrIntroductoryPriceEligibilityCallback { get; set; }

//    ///
//    /// <summary>
//    /// iOS only. Computes whether or not a user is eligible for the introductory pricing period of a given product.
//    /// You should use this method to determine whether or not you show the user the normal product price or
//    /// the introductory price. This also applies to trials (trials are considered a type of introductory pricing).
//    /// <seealso href="https://docs.revenuecat.com/docs/ios-subscription-offers"/>.
//    /// </summary>
//    ///
//    /// <remarks>
//    /// Note: If you're looking to use Promotional Offers instead,
//    /// use <see cref="GetPromotionalOffer"/>.
//    /// </remarks>
//    ///
//    /// <remarks>
//    /// Note: Subscription groups are automatically collected for determining eligibility. If RevenueCat can't
//    /// definitively compute the eligibility, most likely because of missing group information, it will return
//    /// <see cref="IntroEligibilityStatus.IntroEligibilityStatusUnknown"/>.
//    /// The best course of action on unknown status is to display the non-intro
//    /// pricing, to not create a misleading situation. To avoid this, make sure you are testing with the latest
//    /// version of iOS so that the subscription group can be collected by the SDK.
//    /// </remarks>
//    ///
//    /// <param name="products"> The <see cref="StoreProduct"/>  for which you want to compute eligibility.</param>
//    /// <param name="callback"> The <see cref="CheckTrialOrIntroductoryPriceEligibilityFunc"/> callback. </param>
//    public void CheckTrialOrIntroductoryPriceEligibility(string[] products,
//        CheckTrialOrIntroductoryPriceEligibilityFunc callback)
//    {
//        CheckTrialOrIntroductoryPriceEligibilityCallback = callback;
//        _wrapper.CheckTrialOrIntroductoryPriceEligibilityAsync(products);
//    }

//    ///
//    /// <summary>
//    /// Invalidates the cache for customer information.
//    /// </summary>
//    ///
//    /// <remarks>Most apps will not need to use this method; invalidating the cache can leave your app in an invalid state.
//    /// Refer to https://docs.revenuecat.com/docs/purchaserinfo#section-get-user-information
//    /// for more information on using the cache properly.
//    /// </remarks>
//    ///
//    /// This is useful for cases where customer information might have been updated outside of the app, like if a
//    /// promotional subscription is granted through the RevenueCat dashboard.
//    ///
//    public void InvalidateCustomerInfoCache()
//    {
//        _wrapper.InvalidateCustomerInfoCache();
//    }

//    /// <summary>
//    /// iOS only. Displays a sheet that enables users to redeem subscription offer codes
//    /// that you generated in App Store Connect.
//    /// </summary>
//    public void PresentCodeRedemptionSheet()
//    {
//        _wrapper.PresentCodeRedemptionSheet();
//    }

//    /// <summary>
//    /// Callback for <see cref="Purchases.RecordPurchase"/>.
//    /// </summary>
//    /// <param name="transaction"> The <see cref="StoreTransaction"/> object if the request was successful, null otherwise.</param>
//    /// <param name="error"> The error if the request was unsuccessful, null otherwise.</param>
//    public delegate void RecordPurchaseFunc(StoreTransaction transaction, Error error);

//    private RecordPurchaseFunc RecordPurchaseCallback { get; set; }

//    /// <summary>
//    /// iOS only. Always returns an error on iOS < 15.
//    /// Use this method only if you already have your own IAP implementation using StoreKit 2 and want to use
//    /// RevenueCat's backend. If you are using StoreKit 1 for your implementation, you do not need this method.
//    /// You only need to use this method with *new* purchases. Subscription updates are observed automatically.
//    /// Important: This should only be used if you have set PurchasesAreCompletedBy to MyApp during SDK configuration.
//    /// Important: You need to finish the transaction yourself after calling this method.
//    /// </summary>
//    /// <param name="productID">Product ID that was just purchased.</param>
//    /// <param name="callback"> A completion block called when the purchase has been recorded, with either a success or an error.</param>
//    public void RecordPurchase(string productID, RecordPurchaseFunc callback)
//    {
//        RecordPurchaseCallback = callback;
//        _wrapper.RecordPurchase(productID);
//    }

//    /// <summary>
//    /// iOS only.
//    /// Set this property to true *only* when testing the ask-to-buy / SCA purchases flow.
//    /// <seealso href="http://errors.rev.cat/ask-to-buy"/>
//    /// </summary>
//    /// <param name="askToBuyEnabled"> Whether to start simulating ask-to-buy flow in sandbox. </param>
//    public void SetSimulatesAskToBuyInSandbox(bool askToBuyEnabled)
//    {
//        _wrapper.SetSimulatesAskToBuyInSandbox(askToBuyEnabled);
//    }

//    /// <summary>
//    /// Subscriber attributes are useful for storing additional, structured information on a user.
//    /// Since attributes are writable using a public key they should not be used for
//    /// managing secure or sensitive information such as subscription status, coins, etc.
//    /// </summary>
//    /// <remarks>Key names starting with "$" are reserved names used by RevenueCat. For a full list of key
//    /// restrictions refer [to our guide](https://docs.revenuecat.com/docs/subscriber-attributes)
//    /// </remarks>
//    /// <param name="attributes"> Map of attributes by key. Set the value as an empty string to delete an attribute. </param>
//    public void SetAttributes(Dictionary<string, string> attributes)
//    {
//        var jsonObject = new JObject();
//        foreach (var keyValuePair in attributes)
//        {
//            if (keyValuePair.Value == null)
//            {
//                jsonObject[keyValuePair.Key] = JToken.Parse("null");
//            }
//            else
//            {
//                jsonObject[keyValuePair.Key] = keyValuePair.Value;
//            }
//        }

//        _wrapper.SetAttributes(jsonObject.ToString());
//    }

//    ///
//    /// <summary>
//    /// Sets the subscriber attribute associated with the email address for the user.
//    /// </summary>
//    /// <seealso href="https://docs.revenuecat.com/docs/subscriber-attributes"/>
//    ///
//    /// <param name="email"> The email to set.
//    /// Passing empty String or null will delete the subscriber attribute.
//    /// </param>
//    ///
//    public void SetEmail(string email)
//    {
//        _wrapper.SetEmail(email);
//    }

//    /// <summary>
//    /// Sets the subscriber attribute associated with the phone number for the user.
//    /// </summary>
//    /// <seealso href="https://docs.revenuecat.com/docs/subscriber-attributes"/>
//    ///
//    /// <param name="phoneNumber"> The phone number to set.
//    /// Passing empty String or null will delete the subscriber attribute.
//    /// </param>
//    ///
//    public void SetPhoneNumber(string phoneNumber)
//    {
//        _wrapper.SetPhoneNumber(phoneNumber);
//    }

//    /// <summary>
//    /// Sets the subscriber attribute associated with the display name for the user.
//    /// </summary>
//    /// <seealso href="https://docs.revenuecat.com/docs/subscriber-attributes"/>
//    ///
//    /// <param name="displayName"> The display name to set.
//    /// Passing empty String or null will delete the subscriber attribute.
//    /// </param>
//    ///
//    public void SetDisplayName(string displayName)
//    {
//        _wrapper.SetDisplayName(displayName);
//    }

//    /// <summary>
//    /// Sets the subscriber attribute associated with the push token for the user.
//    /// </summary>
//    /// <seealso href="https://docs.revenuecat.com/docs/subscriber-attributes"/>
//    ///
//    /// <param name="token"> The push token to set.
//    /// Passing empty String or null will delete the subscriber attribute.
//    /// </param>
//    ///
//    public void SetPushToken(string token)
//    {
//        _wrapper.SetPushToken(token);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the Adjust Id for the user.
//     * Required for the RevenueCat Adjust integration
//     * </summary>
//     * <param name="adjustID">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetAdjustID(string adjustID)
//    {
//        _wrapper.SetAdjustID(adjustID);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the Appsflyer Id for the user
//     * Required for the RevenueCat Appsflyer integration
//     * </summary>
//     * <param name="appsflyerID">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetAppsflyerID(string appsflyerID)
//    {
//        _wrapper.SetAppsflyerID(appsflyerID);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the Facebook SDK Anonymous Id for the user
//     * Required for the RevenueCat Facebook integration
//     * </summary>
//     * <param name="fbAnonymousID">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetFBAnonymousID(string fbAnonymousID)
//    {
//        _wrapper.SetFBAnonymousID(fbAnonymousID);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the mParticle Id for the user
//     * Required for the RevenueCat mParticle integration
//     * </summary>
//     * <param name="mparticleID">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetMparticleID(string mparticleID)
//    {
//        _wrapper.SetMparticleID(mparticleID);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the OneSignal Player Id for the user
//     * Required for the RevenueCat OneSignal integration
//     * </summary>
//     * <param name="onesignalID">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetOnesignalID(string onesignalID)
//    {
//        _wrapper.SetOnesignalID(onesignalID);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the Airship Channel Id for the user
//     * Required for the RevenueCat Airship integration
//     * </summary>
//     * <param name="airshipChannelID">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetAirshipChannelID(string airshipChannelID)
//    {
//        _wrapper.SetAirshipChannelID(airshipChannelID);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the CleverTap ID for the user.
//     * Required for the RevenueCat CleverTap integration.
//     * </summary>
//     * <param name="cleverTapID">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetCleverTapID(string cleverTapID)
//    {
//        _wrapper.SetCleverTapID(cleverTapID);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the Mixpanel Distinct ID for the user.
//     * Optional for the RevenueCat Mixpanel integration.
//     * </summary>
//     * <param name="mixpanelDistinctID">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetMixpanelDistinctID(string mixpanelDistinctID)
//    {
//        _wrapper.SetMixpanelDistinctID(mixpanelDistinctID);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the Firebase App Instance ID for the user.
//     * Required for the RevenueCat Firebase integration.
//     * </summary>
//     * <param name="firebaseAppInstanceID">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetFirebaseAppInstanceID(string firebaseAppInstanceID)
//    {
//        _wrapper.SetFirebaseAppInstanceID(firebaseAppInstanceID);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the install media source for the user
//     * </summary>
//     * <param name="mediaSource">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetMediaSource(string mediaSource)
//    {
//        _wrapper.SetMediaSource(mediaSource);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the install campaign for the user
//     * </summary>
//     * <param name="campaign">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetCampaign(string campaign)
//    {
//        _wrapper.SetCampaign(campaign);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the install ad group for the user
//     * </summary>
//     * <param name="adGroup">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetAdGroup(string adGroup)
//    {
//        _wrapper.SetAdGroup(adGroup);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the install ad for the user
//     * </summary>
//     * <param name="ad">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetAd(string ad)
//    {
//        _wrapper.SetAd(ad);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the install keyword for the user
//     * </summary>
//     * <param name="keyword">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetKeyword(string keyword)
//    {
//        _wrapper.SetKeyword(keyword);
//    }

//    /**
//     * <summary>
//     * Sets the subscriber attribute associated with the install creative for the user
//     * </summary>
//     * <param name="creative">Empty String or null will delete the subscriber attribute.</param>
//     */
//    public void SetCreative(string creative)
//    {
//        _wrapper.SetCreative(creative);
//    }

//    /**
//     * <summary>
//     * Automatically collect subscriber attributes associated with the device identifiers.
//     * $idfa, $idfv, $ip on iOS
//     * $gpsAdId, $androidId, $ip on Android
//     * </summary>
//     */
//    public void CollectDeviceIdentifiers()
//    {
//        _wrapper.CollectDeviceIdentifiers();
//    }

//    /// <summary>
//    /// Callback function containing the result of CanMakePayments
//    /// </summary>
//    ///
//    /// <param name="canMakePayments">A bool value indicating whether billing
//    /// is supported for the current user (meaning IN-APP purchases are supported),
//    /// and, if provided, whether a list of specified BillingFeatures are supported.
//    /// This will be false if there is an error</param>
//    /// <param name="error">An Error object or null if successful.</param>
//    public delegate void CanMakePaymentsFunc(bool canMakePayments, Error error);

//    private CanMakePaymentsFunc CanMakePaymentsCallback { get; set; }

//    /// <summary>
//    /// Check if billing is supported for the current user (meaning IN-APP purchases are supported)
//    /// and whether a list of specified feature types are supported.
//    ///
//    /// Note: BillingFeatures are only relevant to Google Play Android users.
//    /// For other stores and platforms, BillingFeatures won't be checked.
//    /// </summary>
//    /// <param name="features">An array of BillingFeatures to check for support.
//    /// If empty, no features will be checked.</param>
//    /// <param name="callback">A callback receiving a bool for canMakePayments and potentially an Error</param>
//    public void CanMakePayments(BillingFeature[] features, CanMakePaymentsFunc callback)
//    {
//        CanMakePaymentsCallback = callback;
//        _wrapper.CanMakePaymentsAsync(features == null ? new BillingFeature[] { } : features);
//    }

//    /// <summary>
//    /// Check if billing is supported for the current user (meaning IN-APP purchases are supported)
//    /// </summary>
//    /// <param name="callback">A callback receiving a bool for canMakePayments and potentially an Error</param>
//    public void CanMakePayments(CanMakePaymentsFunc callback)
//    {
//        CanMakePayments(new BillingFeature[] { }, callback);
//    }

//    /// <summary>
//    /// Callback function containing the result of GetAmazonLWAConsentStatus
//    /// </summary>
//    ///
//    /// <param name="hasConsented">A bool value indicating whether user has given consent to
//    /// Login with Amazon.
//    /// </param>
//    /// <param name="error">An Error object or null if successful.</param>
//    public delegate void GetAmazonLWAConsentStatusFunc(bool hasConsented, Error error);

//    private GetAmazonLWAConsentStatusFunc GetAmazonLWAConsentStatusCallback { get; set; }

//    /// <summary>
//    /// Get the Login with Amazon consent status for the current user. Used to implement one-click account creation
//    /// using Quick Subscribe.
//    ///
//    /// For more information, check the documentation:
//    /// https://rev.cat/amazon-quicksubscribe
//    ///
//    /// Note: This method only works for the Amazon Appstore. There is no Google equivalent at this time.
//    /// Calling from a Google-configured app will always return False.
//    /// </summary>
//    /// <param name="callback">A callback receiving a bool for hasConsented and potentially an Error</param>
//    public void GetAmazonLWAConsentStatus(GetAmazonLWAConsentStatusFunc callback)
//    {
//        GetAmazonLWAConsentStatusCallback = callback;
//        _wrapper.GetAmazonLWAConsentStatusAsync();
//    }

//    /// <summary>
//    /// Callback function containing the result of GetPromotionalOffer
//    /// </summary>
//    ///
//    /// <param name="promotionalOffer">A Purchases.PromotionalOffer. It will be Null if platform is Android or
//    /// the iOS version is not compatible with promotional offers</param>
//    /// <param name="error">An Error object or null if successful.</param>
//    public delegate void GetPromotionalOfferFunc(PromotionalOffer promotionalOffer, Error error);

//    private GetPromotionalOfferFunc GetPromotionalOfferCallback { get; set; }

//    /// <summary>
//    /// iOS only. Use this function to retrieve the Purchases.PromotionalOffer for a given Purchases.Package.
//    /// </summary>
//    /// <param name="storeProduct">The Purchases.StoreProduct the user intends to purchase</param>
//    /// <param name="discount">The Purchases.Discount to apply to the product.</param>
//    /// <param name="callback">A callback receiving a Purchases.PromotionalOffer. Null is returned for Android and
//    /// incompatible iOS versions.</param>
//    public void GetPromotionalOffer(StoreProduct storeProduct, Discount discount, GetPromotionalOfferFunc callback)
//    {
//        GetPromotionalOfferCallback = callback;
//        _wrapper.GetPromotionalOffer(storeProduct.Identifier, discount.Identifier);
//    }

//    /// Displays the specified store in-app message types to the user if there are any available to be shown.
//    /// - Important: This should only be used if you disabled these messages from showing automatically
//    /// during SDK configuration setting ``shouldShowInAppMessagesAutomatically`` to ``false``.
//    ///
//    /// @param [messageTypes] The types of messages to show.
//    public void ShowInAppMessages(InAppMessageType[] messageTypes = null)
//    {
//        _wrapper.ShowInAppMessages(messageTypes);
//    }

//    public delegate void ParseAsWebPurchaseRedemptionFunc(WebPurchaseRedemption webPurchaseRedemption);

//    private ParseAsWebPurchaseRedemptionFunc ParseAsWebPurchaseRedemptionCallback { get; set; }

//    public void ParseAsWebPurchaseRedemption(string urlString, ParseAsWebPurchaseRedemptionFunc callback)
//    {
//        ParseAsWebPurchaseRedemptionCallback = callback;
//        _wrapper.ParseAsWebPurchaseRedemptionAsync(urlString);
//    }

//    public delegate void RedeemWebPurchaseFunc(WebPurchaseRedemptionResult result);

//    private RedeemWebPurchaseFunc RedeemWebPurchaseCallback { get; set; }

//    public void RedeemWebPurchase(WebPurchaseRedemption webPurchaseRedemption, RedeemWebPurchaseFunc callback)
//    {
//        RedeemWebPurchaseCallback = callback;
//        _wrapper.RedeemWebPurchaseAsync(webPurchaseRedemption);
//    }

//    public delegate void GetVirtualCurrenciesFunc(VirtualCurrencies virtualCurrencies, Error error);

//    private GetVirtualCurrenciesFunc GetVirtualCurrenciesCallback { get; set; }

//    /// <summary>
//    /// Fetches the virtual currencies for the current user.
//    /// </summary>
//    public void GetVirtualCurrencies(GetVirtualCurrenciesFunc callback)
//    {
//        GetVirtualCurrenciesCallback = callback;
//        _wrapper.GetVirtualCurrenciesAsync();
//    }

//    /// <summary>
//    /// The currently cached VirtualCurrencies if one is available.
//    /// </summary>
//    /// <remarks>
//    /// This value will remain null until virtual currencies have been fetched at 
//    /// least once with GetVirtualCurrencies or an equivalent function.
//    /// </remarks>
//    public VirtualCurrencies GetCachedVirtualCurrencies()
//    {
//        return _wrapper.GetCachedVirtualCurrencies();
//    }

//    /// <summary>
//    /// Invalidates the cache for virtual currencies.
//    /// </summary>
//    /// 
//    /// <remarks>
//    /// This is useful for cases where a virtual currency's balance might have been updated
//    /// outside of the app, like if you decreased a user's balance from the user spending a virtual currency,
//    /// or if you increased the balance from your backend using the server APIs.
//    /// </remarks>
//    public void InvalidateVirtualCurrenciesCache()
//    {
//        _wrapper.InvalidateVirtualCurrenciesCache();
//    }

//    public delegate void GetEligibleWinBackOffersForProductFunc(WinBackOffer[] winBackOffers, Error error);

//    private GetEligibleWinBackOffersForProductFunc GetEligibleWinBackOffersForProductCallback { get; set; }

//    /// <summary>
//    /// Gets eligible win-back offers for a given store product. Only available on iOS 18.0+ with StoreKit 2.
//    /// Returns an error if the platform is not iOS 18.0+ or if StoreKit 2 is not used.
//    /// </summary>
//    /// <param name="storeProduct">The Purchases.StoreProduct to get win-back offers for</param>
//    /// <param name="callback">A callback receiving an array of Purchases.WinBackOffer objects or an error if unsuccessful</param>
//    public void GetEligibleWinBackOffersForProduct(StoreProduct storeProduct, GetEligibleWinBackOffersForProductFunc callback)
//    {
//        GetEligibleWinBackOffersForProductCallback = callback;
//        _wrapper.GetEligibleWinBackOffersForProduct(storeProduct);
//    }

//    public delegate void GetEligibleWinBackOffersForPackageFunc(WinBackOffer[] winBackOffers, Error error);

//    private GetEligibleWinBackOffersForPackageFunc GetEligibleWinBackOffersForPackageCallback { get; set; }

//    /// <summary>
//    /// Gets eligible win-back offers for a given package. Only available on iOS 18.0+ with StoreKit 2. 
//    /// Returns an error if the platform is not iOS 18.0+ or if StoreKit 2 is not used.
//    /// </summary>
//    /// <param name="package">The Purchases.Package to get win-back offers for</param>
//    /// <param name="callback">A callback receiving an array of Purchases.WinBackOffer objects or an error if unsuccessful</param>
//    public void GetEligibleWinBackOffersForPackage(Package package, GetEligibleWinBackOffersForPackageFunc callback)
//    {
//        GetEligibleWinBackOffersForPackageCallback = callback;
//        _wrapper.GetEligibleWinBackOffersForPackage(package);
//    }

//    /// <summary>
//    /// Purchase a product with a win-back offer. Only available on iOS 18.0+ with StoreKit 2.
//    /// Returns an error if the platform is not iOS 18.0+ or if StoreKit 2 is not used.
//    /// </summary>
//    /// <param name="storeProduct">The Purchases.StoreProduct to purchase</param>
//    /// <param name="winBackOffer">The Purchases.WinBackOffer to use</param>
//    /// <param name="callback">A callback receiving the product identifier, customer info, user cancellation, and an error if the purchase fails</param>
//    public void PurchaseProductWithWinBackOffer(StoreProduct storeProduct, WinBackOffer winBackOffer, MakePurchaseFunc callback)
//    {
//        MakePurchaseCallback = callback;
//        _wrapper.PurchaseProductWithWinBackOffer(storeProduct, winBackOffer);
//    }

//    /// <summary>
//    /// Purchase a package with a win-back offer. Only available on iOS 18.0+ with StoreKit 2.
//    /// Returns an error if the platform is not iOS 18.0+ or if StoreKit 2 is not used.
//    /// </summary>
//    /// <param name="package">The Purchases.Package to purchase</param>
//    /// <param name="winBackOffer">The Purchases.WinBackOffer to use</param>
//    /// <param name="callback">A callback receiving the product identifier, customer info, user cancellation, and an error if the purchase fails</param>
//    public void PurchasePackageWithWinBackOffer(Package package, WinBackOffer winBackOffer, MakePurchaseFunc callback)
//    {
//        MakePurchaseCallback = callback;
//        _wrapper.PurchasePackageWithWinBackOffer(package, winBackOffer);
//    }

//    private void _receiveStorefront(string storefrontJson)
//    {
//        Debug.Log("_receiveStorefront " + storefrontJson);

//        if (StorefrontCallback == null)
//        {
//            return;
//        }

//        var callback = StorefrontCallback;
//        StorefrontCallback = null;

//        if (storefrontJson is null or "{}")
//        {
//            callback(null);
//        }
//        else
//        {
//            var response = JObject.Parse(storefrontJson);
//            var countryCode = response["countryCode"];

//            if (countryCode == null)
//            {
//                Debug.LogError("StorefrontCallback received null countryCode");
//                callback(null);
//            }
//            else
//            {
//                var storefront = countryCode.ToObject<Storefront>();
//                callback(storefront);
//            }
//        }
//    }

//    // ReSharper disable once UnusedMember.Local
//    private void _receiveProducts(string productsJson)
//    {
//        Debug.Log("_receiveProducts " + productsJson);

//        if (ProductsCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(productsJson);
//        var callback = ProductsCallback;
//        ProductsCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, error);
//        }
//        else
//        {
//            var products = new List<StoreProduct>();

//            foreach (var productResponse in response["products"]!)
//            {
//                var product = productResponse?.ToObject<StoreProduct>();

//                if (product != null)
//                {
//                    products.Add(product);
//                }
//            }

//            callback(products, null);
//        }
//    }

//    // ReSharper disable once UnusedMember.Local
//    private void _getCustomerInfo(string customerInfoJson)
//    {
//        Debug.Log("_getCustomerInfo " + customerInfoJson);
//        var callback = GetCustomerInfoCallback;
//        GetCustomerInfoCallback = null;
//        ReceiveCustomerInfoMethod(customerInfoJson, callback);
//    }

//    // ReSharper disable once UnusedMember.Local
//    private void _makePurchase(string makePurchaseResponseJson)
//    {
//        Debug.Log("_makePurchase " + makePurchaseResponseJson);

//        if (MakePurchaseCallback == null)
//        {
//            return;
//        }

//        var callback = MakePurchaseCallback;
//        MakePurchaseCallback = null;

//        var purchaseResult = JsonConvert.DeserializeObject<PurchaseResult>(makePurchaseResponseJson);
//        callback(purchaseResult);
//    }

//    // ReSharper disable once UnusedMember.Local
//    private void _receiveCustomerInfo(string customerInfoJson)
//    {
//        Debug.Log("_receiveCustomerInfo " + customerInfoJson);

//        //if (listener == null)
//        //{
//        //    return;
//        //}

//        var response = JObject.Parse(customerInfoJson);
//        var info = response["customerInfo"]?.ToObject<CustomerInfo>();

//        //if (info != null)
//        //{
//        //    listener.CustomerInfoReceived(info);
//        //}
//    }


//    // ReSharper disable once UnusedMember.Local
//    private void _restorePurchases(string customerInfoJson)
//    {
//        Debug.Log("_restorePurchases " + customerInfoJson);
//        var callback = RestorePurchasesCallback;
//        RestorePurchasesCallback = null;
//        ReceiveCustomerInfoMethod(customerInfoJson, callback);
//    }

//    private void _syncPurchases(string customerInfoJson)
//    {
//        Debug.Log("_syncPurchases " + customerInfoJson);
//        var callback = SyncPurchasesCallback;
//        SyncPurchasesCallback = null;
//        ReceiveCustomerInfoMethod(customerInfoJson, callback);
//    }

//    // ReSharper disable once UnusedMember.Local
//    private void _logIn(string logInResultJson)
//    {
//        Debug.Log("_logIn " + logInResultJson);
//        var callback = LogInCallback;
//        LogInCallback = null;
//        ReceiveLogInResultMethod(logInResultJson, callback);
//    }

//    // ReSharper disable once UnusedMember.Local
//    private void _logOut(string customerInfoJson)
//    {
//        Debug.Log("_logOut " + customerInfoJson);
//        var callback = LogOutCallback;
//        LogOutCallback = null;
//        ReceiveCustomerInfoMethod(customerInfoJson, callback);
//    }

//    // ReSharper disable once UnusedMember.Local
//    private void _getOfferings(string offeringsJson)
//    {
//        Debug.Log("_getOfferings " + offeringsJson);

//        if (GetOfferingsCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(offeringsJson);
//        var callback = GetOfferingsCallback;
//        GetOfferingsCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, error);
//        }
//        else
//        {
//            var offerings = response["offerings"]?.ToObject<Offerings>();
//            callback(offerings, null);
//        }
//    }

//    // ReSharper disable once UnusedMember.Local
//    private void _getCurrentOfferingForPlacement(string offeringJson)
//    {
//        if (GetCurrentOfferingForPlacementCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(offeringJson);
//        var callback = GetCurrentOfferingForPlacementCallback;
//        GetCurrentOfferingForPlacementCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, error);
//        }
//        else
//        {
//            var offering = response["offering"]?.ToObject<Offering>();
//            callback(offering, null);
//        }
//    }

//    // ReSharper disable once UnusedMember.Local
//    private void _syncAttributesAndOfferingsIfNeeded(string offeringsJson)
//    {
//        if (SyncAttributesAndOfferingsIfNeededCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(offeringsJson);
//        var callback = SyncAttributesAndOfferingsIfNeededCallback;
//        SyncAttributesAndOfferingsIfNeededCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, error);
//        }
//        else
//        {
//            var offerings = response["offerings"]?.ToObject<Offerings>();
//            callback(offerings, null);
//        }
//    }

//    private void _checkTrialOrIntroductoryPriceEligibility(string json)
//    {
//        Debug.Log("_checkTrialOrIntroductoryPriceEligibility " + json);

//        if (CheckTrialOrIntroductoryPriceEligibilityCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(json);
//        var dictionary = new Dictionary<string, IntroEligibility>();

//        foreach (var (key, value) in response)
//        {
//            dictionary[key] = value?.ToObject<IntroEligibility>();
//        }

//        var callback = CheckTrialOrIntroductoryPriceEligibilityCallback;
//        CheckTrialOrIntroductoryPriceEligibilityCallback = null;
//        callback(dictionary);

//    }

//    private void _recordPurchase(string json)
//    {
//        Debug.Log("_recordPurchase " + json);

//        if (RecordPurchaseCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(json);
//        var callback = RecordPurchaseCallback;
//        RecordPurchaseCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, error);
//        }
//        else
//        {
//            var transaction = response["transaction"]?.ToObject<StoreTransaction>();
//            callback(transaction, null);
//        }
//    }

//    private void _canMakePayments(string canMakePaymentsJson)
//    {
//        Debug.Log("_canMakePayments" + canMakePaymentsJson);

//        if (CanMakePaymentsCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(canMakePaymentsJson);
//        var callback = CanMakePaymentsCallback;
//        CanMakePaymentsCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(false, error);
//        }
//        else
//        {
//            var canMakePayments = response["canMakePayments"]?.Value<bool>() ?? false;
//            callback(canMakePayments, null);
//        }
//    }

//    private void _getAmazonLWAConsentStatus(string getAmazonLWAConsentStatusJson)
//    {
//        Debug.Log("_getAmazonLWAConsentStatus" + getAmazonLWAConsentStatusJson);

//        if (GetAmazonLWAConsentStatusCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(getAmazonLWAConsentStatusJson);
//        var callback = GetAmazonLWAConsentStatusCallback;
//        GetAmazonLWAConsentStatusCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(false, error);
//        }
//        else
//        {
//            var hasConsented = response["amazonLWAConsentStatus"]?.Value<bool>() ?? false;
//            callback(hasConsented, null);
//        }

//    }

//    private void _getPromotionalOffer(string getPromotionalOfferJson)
//    {
//        Debug.Log("_getPromotionalOffer" + getPromotionalOfferJson);

//        if (GetPromotionalOfferCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(getPromotionalOfferJson);
//        var callback = GetPromotionalOfferCallback;
//        GetPromotionalOfferCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, error);
//        }
//        else
//        {
//            var promotionalOffer = response.ToObject<PromotionalOffer>();
//            callback(promotionalOffer, null);
//        }
//    }

//    private void _parseAsWebPurchaseRedemption(string parseAsWebPurchaseRedemptionJSON)
//    {
//        Debug.Log("_parseAsWebPurchaseRedemption " + parseAsWebPurchaseRedemptionJSON);

//        if (ParseAsWebPurchaseRedemptionCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(parseAsWebPurchaseRedemptionJSON);
//        var callback = ParseAsWebPurchaseRedemptionCallback;
//        ParseAsWebPurchaseRedemptionCallback = null;

//        if (TryGetResponseError(response, out _))
//        {
//            callback(null);
//        }
//        else
//        {
//            var webPurchaseRedemption = response["redemptionLink"]?.ToObject<WebPurchaseRedemption>();
//            callback(webPurchaseRedemption);
//        }
//    }

//    private void _redeemWebPurchase(string redeemWebPurchaseJSON)
//    {
//        Debug.Log("_redeemWebPurchase " + redeemWebPurchaseJSON);

//        if (RedeemWebPurchaseCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(redeemWebPurchaseJSON);
//        var callback = RedeemWebPurchaseCallback;
//        RedeemWebPurchaseCallback = null;

//        if (TryGetResponseError(response, out _))
//        {
//            callback(null);
//        }
//        else
//        {
//            var result = WebPurchaseRedemptionResult.FromJson(response);
//            callback(result);
//        }
//    }

//    private void _getVirtualCurrencies(string getVirtualCurrenciesJson)
//    {
//        Debug.Log("_getVirtualCurrencies " + getVirtualCurrenciesJson);

//        if (GetVirtualCurrenciesCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(getVirtualCurrenciesJson);
//        var callback = GetVirtualCurrenciesCallback;
//        GetVirtualCurrenciesCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, error);
//        }
//        else
//        {
//            var virtualCurrencies = response?.ToObject<VirtualCurrencies>();
//            callback(virtualCurrencies, null);
//        }
//    }

//    private void _getEligibleWinBackOffersForProduct(string eligibleWinBackOffersJson)
//    {
//        Debug.Log("_getEligibleWinBackOffersForProduct " + eligibleWinBackOffersJson);

//        if (GetEligibleWinBackOffersForProductCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(eligibleWinBackOffersJson);
//        var callback = GetEligibleWinBackOffersForProductCallback;
//        GetEligibleWinBackOffersForProductCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, response["error"]?.ToObject<Error>());
//        }
//        else
//        {
//            var winBackOffers = new List<WinBackOffer>();

//            foreach (var offerResponse in response["eligibleWinBackOffers"]!)
//            {
//                var offer = offerResponse?.ToObject<WinBackOffer>();
//                winBackOffers.Add(offer);
//            }

//            callback(winBackOffers.ToArray(), null);
//        }
//    }

//    private void _getEligibleWinBackOffersForPackage(string eligibleWinBackOffersJson)
//    {
//        Debug.Log("_getEligibleWinBackOffersForPackage " + eligibleWinBackOffersJson);

//        if (GetEligibleWinBackOffersForPackageCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(eligibleWinBackOffersJson);
//        var callback = GetEligibleWinBackOffersForPackageCallback;
//        GetEligibleWinBackOffersForPackageCallback = null;

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, error);
//        }
//        else
//        {
//            var winBackOffers = new List<WinBackOffer>();

//            foreach (var offerResponse in response["eligibleWinBackOffers"]!)
//            {
//                var offer = offerResponse?.ToObject<WinBackOffer>();
//                winBackOffers.Add(offer);
//            }

//            callback(winBackOffers.ToArray(), null);
//        }

//    }

//    private void _purchaseProductWithWinBackOffer(string purchaseProductWithWinBackOfferJson)
//    {
//        Debug.Log("_purchaseProductWithWinBackOffer " + purchaseProductWithWinBackOfferJson);

//        if (MakePurchaseCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(purchaseProductWithWinBackOfferJson);
//        var callback = MakePurchaseCallback;
//        MakePurchaseCallback = null;

//        callback(response?.ToObject<PurchaseResult>());
//    }

//    private void _purchasePackageWithWinBackOffer(string purchasePackageWithWinBackOfferJson)
//    {
//        Debug.Log("_purchasePackageWithWinBackOffer " + purchasePackageWithWinBackOfferJson);

//        if (MakePurchaseCallback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(purchasePackageWithWinBackOfferJson);
//        var callback = MakePurchaseCallback;
//        MakePurchaseCallback = null;

//        callback(response.ToObject<PurchaseResult>());
//    }

//    private static void ReceiveCustomerInfoMethod(string arguments, CustomerInfoFunc callback)
//    {
//        if (callback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(arguments);

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, error);
//        }
//        else
//        {
//            var info = response["customerInfo"]?.ToObject<CustomerInfo>();
//            callback(info, null);
//        }
//    }

//    private static void ReceiveLogInResultMethod(string arguments, LogInFunc callback)
//    {
//        if (callback == null)
//        {
//            return;
//        }

//        var response = JObject.Parse(arguments);

//        if (TryGetResponseError(response, out var error))
//        {
//            callback(null, false, error);
//        }
//        else
//        {
//            var info = response["customerInfo"]!.ToObject<CustomerInfo>();
//            var created = response["created"]!.ToObject<bool>();
//            callback(info, created, null);
//        }
//    }

//    private static bool TryGetResponseError(JObject response, out Error error)
//    {
//        error = null;

//        if (response.TryGetValue("error", out var e))
//        {
//            error = e.ToObject<Error>();
//        }

//        return error != null;
//    }
//}

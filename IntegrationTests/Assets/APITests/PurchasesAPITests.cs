using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomListener : Purchases.UpdatedCustomerInfoListener
{
    public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        throw new System.NotImplementedException();
    }
}

public class PurchasesAPITests : MonoBehaviour
{
    private void Start()
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.revenueCatAPIKeyApple = "def";
        purchases.revenueCatAPIKeyGoogle = "ghi";
        purchases.revenueCatAPIKeyAmazon = "ghi";
        purchases.useAmazon = true;
        purchases.appUserID = "abc";
        purchases.productIdentifiers = new[]
        {
            "a", "b", "c"
        };

        purchases.listener = new CustomListener();
        purchases.userDefaultsSuiteName = "suitename";
        purchases.proxyURL = "https://proxy-url.revenuecat.com";

        Purchases.CustomerInfo receivedCustomerInfo;
        Purchases.Error receivedError;
        Purchases.StoreTransaction receivedTransaction;

        Purchases.Package receivedPackage;
        Purchases.Offerings receivedOfferings;
        Purchases.Offering receivedOffering;
        String receivedProductIdentifier;
        bool receivedUserCancelled;
        Purchases.LogLevel receivedLogLevel;
        String receivedMessage;

        List<Purchases.StoreProduct> receivedProducts = new List<Purchases.StoreProduct>();

        List<Purchases.StoreProduct> storeProducts = new List<Purchases.StoreProduct>();
        purchases.GetProducts(new string[]
        {
            "a", "b"
        }, (products, error) => { receivedProducts = products; });

        purchases.GetProducts(new string[] { "a", "b" }, (products, error) => { receivedProducts = products; }, "type");

        purchases.GetOfferings((offerings, error) =>
        {
            receivedOfferings = offerings;
            receivedError = error;
            receivedOffering = receivedOfferings.All.First().Value;

            receivedPackage = receivedOffering.AvailablePackages.First();
            purchases.PurchasePackage(receivedPackage, (productIdentifier, customerInfo, userCancelled, error2) =>
            {
                receivedProductIdentifier = productIdentifier;
                receivedCustomerInfo = customerInfo;
                receivedUserCancelled = userCancelled;
                receivedError = error2;
            });

            purchases.PurchasePackage(receivedPackage, (productIdentifier, customerInfo, userCancelled, error2) =>
            {
                receivedProductIdentifier = productIdentifier;
                receivedCustomerInfo = customerInfo;
                receivedUserCancelled = userCancelled;
                receivedError = error2;
            }, "oldSku", Purchases.ProrationMode.ImmediateWithoutProration);

            Purchases.StoreProduct storeProduct = storeProducts.First();
            Purchases.SubscriptionOption subscriptionOption = storeProduct.DefaultOption;
            purchases.PurchaseSubscriptionOption(subscriptionOption, (productIdentifier, customerInfo, userCancelled, error) =>
            {
                receivedProductIdentifier = productIdentifier;
                receivedCustomerInfo = customerInfo;
                receivedUserCancelled = userCancelled;
                receivedError = error;
            }, null, false);

            Purchases.PromotionalOffer receivedPromoOffer;
            purchases.GetPromotionalOffer(storeProduct, storeProduct.Discounts.First(), (offer, error2) =>
            {
                receivedPromoOffer = offer;
                receivedError = error2;

                purchases.PurchaseDiscountedPackage(receivedPackage, receivedPromoOffer,
                    (productIdentifier, purchaserInfo, userCancelled, error3) =>
                    {
                        receivedProductIdentifier = productIdentifier;
                        receivedCustomerInfo = purchaserInfo;
                        receivedUserCancelled = userCancelled;
                        receivedError = error3;
                    });

                purchases.PurchaseDiscountedProduct("product_id", receivedPromoOffer,
                    (productIdentifier, purchaserInfo, userCancelled, error3) =>
                    {
                        receivedProductIdentifier = productIdentifier;
                        receivedCustomerInfo = purchaserInfo;
                        receivedUserCancelled = userCancelled;
                        receivedError = error3;
                    });
            });
        });

        purchases.PurchaseProduct("product_id", (productIdentifier, customerInfo, userCancelled, error2) =>
        {
            receivedProductIdentifier = productIdentifier;
            receivedCustomerInfo = customerInfo;
            receivedUserCancelled = userCancelled;
            receivedError = error2;
        });

        purchases.PurchaseProduct("product_id", (productIdentifier, purchaserInfo, userCancelled, error2) =>
        {
            receivedProductIdentifier = productIdentifier;
            receivedCustomerInfo = purchaserInfo;
            receivedUserCancelled = userCancelled;
            receivedError = error2;
        }, "type", "oldSku", Purchases.ProrationMode.ImmediateWithoutProration);

        purchases.RestorePurchases((customerInfo, error) =>
        {
            receivedCustomerInfo = customerInfo;
            receivedError = error;
        });

        bool receivedCreated = false;
        purchases.LogIn("appuUerId", (info, created, error) =>
        {
            receivedCustomerInfo = info;
            receivedCreated = created;
            receivedError = error;
        });

        purchases.LogOut((info, error) =>
        {
            receivedCustomerInfo = info;
            receivedError = error;
        });

        #pragma warning disable CS0618 // Type or member is obsolete
        purchases.SetAllowSharingStoreAccount(false);
        #pragma warning restore CS0618 // Type or member is obsolete
        string appUserId = purchases.GetAppUserId();
        bool isAnonymous = purchases.IsAnonymous();
        bool isConfigured = purchases.IsConfigured();
        #pragma warning disable CS0618 // Type or member is obsolete
        purchases.SetDebugLogsEnabled(true);
        #pragma warning restore CS0618 // Type or member is obsolete
        purchases.SetLogLevel(Purchases.LogLevel.Debug);
        purchases.SetLogHandler((level, message) =>
        {
            receivedLogLevel = level;
            receivedMessage = message;
        });
        purchases.GetCustomerInfo((info, error) =>
        {
            receivedCustomerInfo = info;
            receivedError = error;
        });

        purchases.SyncPurchases();
        purchases.SyncPurchases((customerInfo, error) =>
        {
            receivedCustomerInfo = customerInfo;
            receivedError = error;
        });
        purchases.EnableAdServicesAttributionTokenCollection();
        Dictionary<string, Purchases.IntroEligibility> receivedEligibilities;
        purchases.CheckTrialOrIntroductoryPriceEligibility(new string[] { "a", "b" },
            eligibilities => { receivedEligibilities = eligibilities; });
        purchases.InvalidateCustomerInfoCache();
        purchases.PresentCodeRedemptionSheet();
        purchases.SetSimulatesAskToBuyInSandbox(true);

        purchases.SetAttributes(new Dictionary<string, string>());
        purchases.SetEmail("asdf");
        purchases.SetPhoneNumber("asdga");
        purchases.SetDisplayName("asdgas");
        purchases.SetPushToken("asdgas");
        purchases.SetAdjustID("asdgas");
        purchases.SetAppsflyerID("asdgas");
        purchases.SetFBAnonymousID("asdgas");
        purchases.SetMparticleID("asdgas");
        purchases.SetOnesignalID("asdgas");
        purchases.SetAirshipChannelID("asdgas");
        purchases.SetCleverTapID("asdgas");
        purchases.SetMixpanelDistinctID("asdgas");
        purchases.SetFirebaseAppInstanceID("asdgas");
        purchases.SetMediaSource("asdgas");
        purchases.SetCampaign("asdgas");
        purchases.SetAdGroup("asdgas");
        purchases.SetAd("asdgas");
        purchases.SetKeyword("asdgas");
        purchases.SetCreative("asdgas");
        purchases.CollectDeviceIdentifiers();

        bool receivedCanMakePayments = false;

        purchases.CanMakePayments((canMakePayments, error) =>
        {
            receivedCanMakePayments = canMakePayments;
            receivedError = error;
        });

        purchases.CanMakePayments(new Purchases.BillingFeature[] { Purchases.BillingFeature.Subscriptions },
            (canMakePayments, error) =>
            {
                receivedCanMakePayments = canMakePayments;
                receivedError = error;
            });

        Purchases.PurchasesConfiguration.Builder builder = Purchases.PurchasesConfiguration.Builder.Init("api_key");
        Purchases.PurchasesConfiguration purchasesConfiguration =
            builder.SetUserDefaultsSuiteName("user_default")
            .SetDangerousSettings(new Purchases.DangerousSettings(false))
            .SetPurchasesAreCompletedBy(Purchases.PurchasesAreCompletedBy.MyApp, Purchases.StoreKitVersion.StoreKit2)
            .SetUseAmazon(false)
            .SetAppUserId(appUserId)
            .SetStoreKitVersion(Purchases.StoreKitVersion.StoreKit2)
            .SetShouldShowInAppMessagesAutomatically(false)
            .SetEntitlementVerificationMode(Purchases.EntitlementVerificationMode.Informational)
            .SetPendingTransactionsForPrepaidPlansEnabled(true)
            .Build();
        purchases.Configure(purchasesConfiguration);
        purchases.RecordPurchase("product_id", (transaction, error) => 
        {
            receivedTransaction = transaction;
            receivedError = error;
        });

        purchases.SyncObserverModeAmazonPurchase("product_id", "receipt_id", "amazon_user_id", "iso_currency_code", 1.99);
        purchases.SyncAmazonPurchase("product_id", "receipt_id", "amazon_user_id", "iso_currency_code", 1.99);

        purchases.ShowInAppMessages(new Purchases.InAppMessageType[] { Purchases.InAppMessageType.BillingIssue,
            Purchases.InAppMessageType.PriceIncreaseConsent, Purchases.InAppMessageType.Generic, Purchases.InAppMessageType.WinBackOffer });
        purchases.ShowInAppMessages();
    }
}

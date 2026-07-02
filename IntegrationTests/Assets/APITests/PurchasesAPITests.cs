using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RevenueCat;
using RevenueCat.SimpleJSON;

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

        Purchases.PurchaseResult receivedPurchaseResult;

        Purchases.Package receivedPackage;
        Purchases.Offerings receivedOfferings;
        Purchases.Offering receivedOffering;
        String receivedProductIdentifier;
        bool receivedUserCancelled;
        Purchases.LogLevel receivedLogLevel;
        String receivedMessage;
        Purchases.Storefront? receivedStorefront;
        Purchases.VirtualCurrencies? receivedVirtualCurrencies;
        Purchases.Error? receivedOptionalError;

        purchases.GetStorefront((storefront) =>
        {
            receivedStorefront = storefront;
        });

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
            purchases.PurchasePackage(receivedPackage, (purchaseResult) =>
            {
                receivedPurchaseResult = purchaseResult;
            });

            purchases.PurchasePackage(receivedPackage, (purchaseResult) =>
            {
                receivedPurchaseResult = purchaseResult;
            }, "oldSku", Purchases.ProrationMode.ImmediateWithoutProration);

            Purchases.StoreProduct storeProduct = storeProducts.First();
            Purchases.SubscriptionOption subscriptionOption = storeProduct.DefaultOption;
            purchases.PurchaseSubscriptionOption(subscriptionOption, (purchaseResult) =>
            {
                receivedPurchaseResult = purchaseResult;
            }, null, false);

            Purchases.PromotionalOffer receivedPromoOffer;
            purchases.GetPromotionalOffer(storeProduct, storeProduct.Discounts.First(), (offer, error2) =>
            {
                receivedPromoOffer = offer;
                receivedError = error2;

                purchases.PurchaseDiscountedPackage(receivedPackage, receivedPromoOffer,
                    (purchaseResult) =>
                    {
                        receivedPurchaseResult = purchaseResult;
                    });

                purchases.PurchaseDiscountedProduct("product_id", receivedPromoOffer,
                    (purchaseResult) =>
                    {
                        receivedPurchaseResult = purchaseResult;
                    });
            });
        });

        purchases.PurchaseProduct("product_id", (purchaseResult) =>
        {
            receivedPurchaseResult = purchaseResult;
        });

        purchases.PurchaseProduct("product_id", (purchaseResult) =>
        {
            receivedPurchaseResult = purchaseResult;
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
        purchases.OverridePreferredUILocale("de_DE");
        purchases.OverridePreferredUILocale(null);
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
        purchases.SetOnesignalUserID("asdgas");
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
        purchases.SetAppsFlyerConversionData(new Dictionary<string, object>());
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
            .SetPreferredUILocaleOverride("de_DE")
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

        purchases.TrackCustomPaywallImpression();
        purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams("my_custom_paywall"));
#pragma warning disable CS0618
        purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams(paywallId: null, offeringId: null));
        purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams(paywallId: "", offeringId: ""));
        purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams(offeringId: "offering_id"));
        purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams("my_custom_paywall", "offering_id"));
#pragma warning restore CS0618
        Purchases.Offering offering = CreateOfferingForCustomPaywallImpression();
        Purchases.CustomPaywallImpressionParams paramsWithOffering =
            new Purchases.CustomPaywallImpressionParams(offering);
        Purchases.Offering paramsOffering = paramsWithOffering.Offering;
        purchases.TrackCustomPaywallImpression(paramsWithOffering);
        purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams("my_custom_paywall", paramsOffering));

        // Ad tracking API tests
        purchases.AdTracker.TrackAdDisplayed(new AdDisplayedData(AdTracker.MediatorName.AdMob, AdTracker.Format.Banner, "ad_unit", "imp_001"));
        purchases.AdTracker.TrackAdDisplayed(new AdDisplayedData(AdTracker.MediatorName.AdMob, AdTracker.Format.Rewarded, "ad_unit", "imp_002", networkName: "network", placement: "main_menu"));
        purchases.AdTracker.TrackAdOpened(new AdOpenedData(AdTracker.MediatorName.AppLovin, AdTracker.Format.Interstitial, "ad_unit", "imp_003"));
        purchases.AdTracker.TrackAdLoaded(new AdLoadedData(new AdTracker.MediatorName("custom"), AdTracker.Format.AppOpen, "ad_unit", "imp_004"));
        purchases.AdTracker.TrackAdRevenue(new AdRevenueData(AdTracker.MediatorName.AdMob, AdTracker.Format.Banner, "ad_unit", "imp_005", 1000000L, "USD", AdTracker.Precision.Estimated));
        purchases.AdTracker.TrackAdRevenue(new AdRevenueData(AdTracker.MediatorName.AdMob, AdTracker.Format.Rewarded, "ad_unit", "imp_006", 500000L, "EUR", AdTracker.Precision.PublisherDefined, networkName: "network", placement: "end_level"));
        purchases.AdTracker.TrackAdFailedToLoad(new AdFailedToLoadData(AdTracker.MediatorName.AdMob, AdTracker.Format.Banner, "ad_unit"));
        purchases.AdTracker.TrackAdFailedToLoad(new AdFailedToLoadData(AdTracker.MediatorName.AdMob, AdTracker.Format.Banner, "ad_unit", placement: "home", mediatorErrorCode: 2));

        // Win-back offer API tests
        // Purchasing win-back offers with a product
        purchases.GetProducts(new[] { "product_id" }, (products, error) =>
        {
            purchases.GetEligibleWinBackOffersForProduct(products[0], (winBackOffers, error2) =>
            {
                purchases.PurchaseProductWithWinBackOffer(products[0], winBackOffers[0], 
                    (purchaseResult) =>
                {
                    receivedPurchaseResult = purchaseResult;
                });
            });
        });

        // Purchasing win-back offers with a package
        purchases.GetOfferings((offerings, error) =>
        {
            Purchases.Package package = offerings.Current.AvailablePackages.First();
            purchases.GetEligibleWinBackOffersForPackage(package, (winBackOffers, error2) =>
            {
                purchases.PurchasePackageWithWinBackOffer(package, winBackOffers[0], 
                    (purchaseResult) =>
                {
                    receivedPurchaseResult = purchaseResult;
                });
            });
        });

        // Virtual currencies API tests
        purchases.GetVirtualCurrencies((virtualCurrencies, error) =>
        {
            receivedVirtualCurrencies = virtualCurrencies;
            receivedOptionalError = error;
        });

        receivedVirtualCurrencies = purchases.GetCachedVirtualCurrencies();
        purchases.InvalidateVirtualCurrenciesCache();
    }

    private static Purchases.Offering CreateOfferingForCustomPaywallImpression()
    {
        JSONNode offeringJson = JSON.Parse(
            @"{
                ""identifier"": ""custom_offering"",
                ""serverDescription"": ""Custom paywall offering"",
                ""metadata"": {},
                ""availablePackages"": [
                    {
                        ""identifier"": ""$rc_monthly"",
                        ""packageType"": ""MONTHLY"",
                        ""product"": {
                            ""identifier"": ""product_id"",
                            ""title"": ""Monthly"",
                            ""description"": ""Monthly subscription"",
                            ""price"": 1.99,
                            ""priceString"": ""$1.99"",
                            ""currencyCode"": ""USD"",
                            ""productCategory"": ""SUBSCRIPTION""
                        },
                        ""presentedOfferingContext"": {
                            ""offeringIdentifier"": ""custom_offering"",
                            ""placementIdentifier"": ""onboarding"",
                            ""targetingContext"": {
                                ""revision"": 7,
                                ""ruleId"": ""rule_1""
                            }
                        }
                    }
                ]
            }");

        return new Purchases.Offering(offeringJson);
    }
}

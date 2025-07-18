﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PurchasesListener : Purchases.UpdatedCustomerInfoListener
{
    public RectTransform parentPanel;
    public GameObject buttonPrefab;
    public Text infoLabel;

    private bool simulatesAskToBuyInSandbox;

    private int minYOffsetForButtons = 40; // values lower than these don't work great with devices
    // with safe areas on iOS

    private int minXOffsetForButtons = 20;

    private int xPaddingForButtons = 10;
    private int yPaddingForButtons = 5;

    private int maxButtonsPerRow = 2;
    private int currentButtons = 0;

    private Purchases.ProrationMode prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy;
    private string currentProductId = "";

    // Use this for initialization
    private void Start()
    {
        CreateButton("Get Customer Info", GetCustomerInfo);
        CreateButton("Get Offerings", GetOfferings);
        CreateButton("Get Current Offering For Onboarding Placement", GetCurrentOfferingForPlacement);
        CreateButton("Sync Attributes and Offerings", SyncAttributesAndOfferingsIfNeeded);
        CreateButton("Sync Purchases", SyncPurchases);
        CreateButton("Restore Purchases", RestorePurchases);
        CreateButton("Can Make Payments", CanMakePayments);
        CreateButton("Set Subs Attributes", SetSubscriberAttributes);
        CreateButton("Log in as \"test\"", LogInAsTest);
        CreateButton("Log in as random id", LogInAsRandomId);
        CreateButton("Log out", LogOut);
        CreateButton("Check Intro Eligibility", CheckIntroEligibility);
        CreateButton("Get Promo Offer", GetPromotionalOffers);
        CreateButton("Buy package w/discount", BuyFirstPackageWithDiscount);
        CreateButton("Buy product w/discount", BuyFirstProductWithDiscount);
        CreateButton("Code redemption sheet", PresentCodeRedemptionSheet);
        CreateButton("Invalidate customer info cache", InvalidateCustomerInfoCache);
        CreateButton("Get all products", GetAllProducts);
        CreateButton("Toggle simulatesAskToBuyInSandbox", ToggleSimulatesAskToBuyInSandbox);
        CreateButton("Is Anonymous", IsAnonymous);
        CreateButton("Is Configured", IsConfigured);
        CreateButton("Get AppUserId", GetAppUserId);
        CreateButton("Show In-App Messages", ShowInAppMessages);
        CreateButton("Get Amazon LWAConsentStatus", GetAmazonLWAConsentStatus);
        CreateProrationModeButtons();
        CreatePurchasePackageButtons();
        CreatePurchasePackageForPlacementButtons();
        CreateButton("Purchase Product For WinBack Testing", PurchaseProductForWinBackTesting);
        CreateButton("Fetch & Redeem WinBack for Product", FetchAndRedeemWinBackForProduct);
        CreateButton("Purchase Package For WinBack Testing", PurchasePackageForWinBackTesting);
        CreateButton("Fetch & Redeem WinBack for Package", FetchAndRedeemWinBackForPackage);
        CreateButton("Get Storefront", GetStorefront);

        var purchases = GetComponent<Purchases>();
        purchases.SetLogLevel(Purchases.LogLevel.Verbose);
        purchases.EnableAdServicesAttributionTokenCollection();
    }

    private void CreateProrationModeButtons()
    {
        foreach (Purchases.ProrationMode mode in Enum.GetValues(typeof(Purchases.ProrationMode)))
        {
            CreateButton("Proration mode: " +  mode, () =>
            {
                infoLabel.text = "ProrationMode set to " + mode;
                prorationMode = mode;
            });
        }
    }

    private void GetStorefront()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetStorefront((storefront) =>
        {
            if (storefront == null)
            {
                infoLabel.text = "Storefront is null";
            }
            else
            {
                infoLabel.text = "Storefront: " + storefront.CountryCode;
            }
        });
    }

    private void CreatePurchasePackageButtons()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                Debug.Log("offerings received " + offerings.ToString());

                foreach (var package in offerings.Current.AvailablePackages)
                {
                    Debug.Log("Package " + package);
                    if (package == null) continue;
                    var label = package.PackageType + " " + package.StoreProduct.PriceString;
                    CreateButton("Buy as Package: " + label, () => PurchasePackageButtonClicked(package));

                    CreateButton("Buy as Product: " + label, () => PurchaseProductButtonClicked(package.StoreProduct));

                    var options = package.StoreProduct.SubscriptionOptions;
                    if (options is not null) {
                        foreach (var subscriptionOption in options) {
                            List<string> parts = new List<string>();
                            var label2 = package.PackageType;

                            var phases = subscriptionOption.PricingPhases;
                            if (phases is not null) {
                                foreach (var pricingPhase in phases) {
                                    var period = pricingPhase.BillingPeriod;
                                    var price = pricingPhase.Price;
                                    if (period is not null && price is not null) {
                                        parts.Add(price.Formatted + " for " + period.ISO8601);
                                    }
                                }
                            } else {
                                parts.Add("ITS SO NULL");
                            }
                            var info = String.Join(" -> ", parts.ToArray());
                            CreateButton(info,  () => PurchaseSubscriptionOptionButtonClicked(subscriptionOption));
                        }
                    }
                }
            }
        });
    }

    private void CreatePurchasePackageForPlacementButtons()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetCurrentOfferingForPlacement("pizza", (offering, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                Debug.Log("offering for placement received " + offering.ToString());

                foreach (var package in offering.AvailablePackages)
                {
                    Debug.Log("Placement Package " + package);
                    if (package == null) continue;
                    var label = package.PackageType + " " + package.StoreProduct.PriceString;
                    CreateButton("Buy as Placement Package: " + label, () => PurchasePackageButtonClicked(package));

                    var options = package.StoreProduct.SubscriptionOptions;
                    if (options is not null) {
                        foreach (var subscriptionOption in options) {
                            List<string> parts = new List<string>();
                            var label2 = package.PackageType;

                            var phases = subscriptionOption.PricingPhases;
                            if (phases is not null) {
                                foreach (var pricingPhase in phases) {
                                    var period = pricingPhase.BillingPeriod;
                                    var price = pricingPhase.Price;
                                    if (period is not null && price is not null) {
                                        parts.Add(price.Formatted + " for " + period.ISO8601);
                                    }
                                }
                            } else {
                                parts.Add("ITS SO NULL");
                            }
                            var info = "PCMNT: " + String.Join(" -> ", parts.ToArray());
                            CreateButton(info,  () => PurchaseSubscriptionOptionButtonClicked(subscriptionOption));
                        }
                    }
                }
            }
        });
    }

    private void CreateButton(string label, UnityAction action)
    {
        var button = Instantiate(buttonPrefab, parentPanel, false);
        var buttonTransform = (RectTransform)button.transform;

        var rect = buttonTransform.rect;
        var height = rect.height;
        var width = rect.width;

        var yPos = -1 // unity counts from the bottom left, so negative values give you buttons that are
                   // lower in the screen
                   * (currentButtons / maxButtonsPerRow // how many buttons are on top of this one
                      * (height +
                         yPaddingForButtons) // distance from start of the first button to the start of the second
                      + minYOffsetForButtons // min distance to the top of the container
                      + height / 2); // y position starts from the center
        var xPos = (currentButtons % maxButtonsPerRow) // 0 for first column, 1 for second column
                   * (width + xPaddingForButtons) // distance from start of the first button to the start of the second
                   + minXOffsetForButtons + (width / 2); // x position starts from the center

        // anchors position calculation to make it easier to reason about
        var newButtonTransform = (RectTransform)button.transform;
        newButtonTransform.anchorMin = new Vector2(0, 1);
        newButtonTransform.anchorMax = new Vector2(0, 1);

        newButtonTransform.anchoredPosition = new Vector2(xPos, yPos);

        var tempButton = button.GetComponent<Button>();

        var textComponent = tempButton.GetComponentsInChildren<Text>()[0];
        textComponent.text = label;

        tempButton.onClick.AddListener(action);
        currentButtons++;
    }

    [Serializable]
    private class AdjustData
    {
        // ReSharper disable NotAccessedField.Local
        public string adid;
        public string network;
        public string adgroup;
        public string campaign;
        public string creative;
        public string clickLabel;
        public string trackerName;
        public string trackerToken;
    }


    private void PurchaseProductButtonClicked(Purchases.StoreProduct storeProduct)
    {
        var purchases = GetComponent<Purchases>();
        purchases.PurchaseProduct(storeProduct.Identifier, (purchaseResult) =>
        {
            if (!purchaseResult.UserCancelled)
            {
                if (purchaseResult.Error != null)
                {
                    LogError(purchaseResult.Error);
                }
                else
                {
                    DisplayCustomerInfo(purchaseResult.CustomerInfo);
                    Debug.Log("StoreTransaction: " + purchaseResult.StoreTransaction);
                }
            }
            else
            {
                Debug.Log("Subtester: User cancelled, don't show an error");
            }
        }, "subs", null, prorationMode, false);
    }


    private void PurchasePackageButtonClicked(Purchases.Package package)
    {
        var purchases = GetComponent<Purchases>();
        purchases.PurchasePackage(package, (purchaseResult) =>
        {
            if (!purchaseResult.UserCancelled)
            {
                if (purchaseResult.Error != null)
                {
                    LogError(purchaseResult.Error);
                }
                else
                {
                    DisplayCustomerInfo(purchaseResult.CustomerInfo);
                    Debug.Log("StoreTransaction: " + purchaseResult.StoreTransaction);
                }
            }
            else
            {
                Debug.Log("Subtester: User cancelled, don't show an error");
            }
        }, currentProductId, prorationMode);
    }

    private void PurchaseSubscriptionOptionButtonClicked(Purchases.SubscriptionOption subscriptionOption)
    {
        Purchases.GoogleProductChangeInfo googleProductChangeInfo = null;
        if (prorationMode != Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy && currentProductId != "") {
            googleProductChangeInfo = new Purchases.GoogleProductChangeInfo(currentProductId, prorationMode);
        }
        var purchases = GetComponent<Purchases>();
        purchases.PurchaseSubscriptionOption(subscriptionOption, (purchaseResult) =>
        {
            if (!purchaseResult.UserCancelled)
            {
                if (purchaseResult.Error != null)
                {
                    LogError(purchaseResult.Error);
                }
                else
                {
                    DisplayCustomerInfo(purchaseResult.CustomerInfo);
                    Debug.Log("StoreTransaction: " + purchaseResult.StoreTransaction);
                }
            }
            else
            {
                Debug.Log("Subtester: User cancelled, don't show an error");
            }
        }, googleProductChangeInfo, false);
    }

    void GetCustomerInfo()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetCustomerInfo((customerInfo, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                DisplayCustomerInfo(customerInfo);
                currentProductId = customerInfo.ActiveSubscriptions.First().Split(":").First();
            }
        });
    }

    void GetOfferings()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = offerings.ToString();
            }
        });
    }

    void GetCurrentOfferingForPlacement()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetCurrentOfferingForPlacement("onboarding", (offering, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = offering.ToString();
            }
        });
    }

    void SyncAttributesAndOfferingsIfNeeded()
    {
        var purchases = GetComponent<Purchases>();
        purchases.SyncAttributesAndOfferingsIfNeeded((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = offerings.ToString();
            }
        });
    }

    void RestorePurchases()
    {
        var purchases = GetComponent<Purchases>();
        purchases.RestorePurchases((customerInfo, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                DisplayCustomerInfo(customerInfo);
            }
        });
    }

    void SyncPurchases()
    {
        var purchases = GetComponent<Purchases>();
        purchases.SyncPurchases((customerInfo, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                DisplayCustomerInfo(customerInfo);
            }
        });
    }

    void CanMakePayments()
    {
        var purchases = GetComponent<Purchases>();
        purchases.CanMakePayments((canMakePayments, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = $"Can make payments: {canMakePayments}";
            }
        });
    }

    void SetSubscriberAttributes()
    {
        var purchases = GetComponent<Purchases>();
        Int32 unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        var timestampString = $"{unixTimestamp}";
        purchases.SetAttributes(new Dictionary<string, string>
        {
            { "timestamp", timestampString },
        });
        purchases.SetEmail($"email_{timestampString}@revenuecat.com");
        purchases.SetPhoneNumber($"{timestampString}");
        purchases.SetDisplayName($"displayName_{timestampString}");
        purchases.SetPushToken($"pushtoken_{timestampString}");
        purchases.SetAdjustID($"adjustId_{timestampString}");
        purchases.SetAppsflyerID($"appsflyerId_{timestampString}");
        purchases.SetFBAnonymousID($"fbAnonymousId_{timestampString}");
        purchases.SetMparticleID($"mparticleId_{timestampString}");
        purchases.SetOnesignalID($"onesignalId_{timestampString}");
        purchases.SetAirshipChannelID($"airshipChannelId_{timestampString}");
        purchases.SetCleverTapID($"cleverTapID_{timestampString}");
        purchases.SetMixpanelDistinctID($"mixpanelDistinctID_{timestampString}");
        purchases.SetFirebaseAppInstanceID($"firebaseAppInstanceID_{timestampString}");
        purchases.SetMediaSource($"mediaSource_{timestampString}");
        purchases.SetCampaign($"campaign_{timestampString}");
        purchases.SetAdGroup($"adgroup_{timestampString}");
        purchases.SetAd($"ad_{timestampString}");
        purchases.SetKeyword($"keyword_{timestampString}");
        purchases.SetCreative($"creative_{timestampString}");
        purchases.CollectDeviceIdentifiers();
        infoLabel.text = "Subscriber attributes set!";
    }

    void LogInAsTest()
    {
        LogIn("test");
    }

    void LogInAsRandomId()
    {
        Guid appUserID = Guid.NewGuid();
        LogIn(appUserID.ToString());
    }

    void LogIn(string appUserID)
    {
        var purchases = GetComponent<Purchases>();
        purchases.LogIn(appUserID, (customerInfo, created, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = $"created: {created}\n customerInfo:\n{customerInfo.ToString()}";
            }
        });
    }

    void LogOut()
    {
        var purchases = GetComponent<Purchases>();
        purchases.LogOut((customerInfo, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = customerInfo.ToString();
            }
        });
    }

    void CheckIntroEligibility()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                var productIds = GetPackages(offerings)
                    .Select(package => package.StoreProduct.Identifier) // map to product ids
                    .ToArray();
                purchases.GetProducts(productIds, (storeProducts, innerError) =>
                {
                    if (innerError != null)
                    {
                        LogError(innerError);
                    }
                    else
                    {
                        purchases.CheckTrialOrIntroductoryPriceEligibility(productIds, eligibilitiesByProductId =>
                        {
                            var items = eligibilitiesByProductId.Select(kvp =>
                                string.Format($"{kvp.Key} : Id={kvp.Value.ToString()}"));
                            infoLabel.text = $"{{ \n {string.Join(Environment.NewLine, items)}\n }} \n ";
                        });
                    }
                });
            }
        });
    }

    void GetPromotionalOffers()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                var products = GetPackages(offerings)
                    .Select(package => package.StoreProduct) // map to product ids
                    .Where(storeProduct => storeProduct.Discounts != null && storeProduct.Discounts.Any()) // map to product ids
                    .ToArray();

                if (products.Length == 0)
                {
                    infoLabel.text = "No promotional offers available for any products";
                    return;
                }

                infoLabel.text = "";
                foreach (Purchases.StoreProduct storeProduct in products)
                {
                    purchases.GetPromotionalOffer(storeProduct, storeProduct.Discounts.First(),
                        (promoOffer, innerError) =>
                        {
                            lock (this)
                            {
                                if (innerError != null)
                                {
                                    LogError(innerError);
                                }
                                else
                                {
                                    infoLabel.text += $"{promoOffer}\n-----\n";
                                }
                            }
                        });
                }
            }
        });
    }

    void BuyFirstProductWithDiscount()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                var products = GetPackages(offerings)
                    .Select(package => package.StoreProduct) // map to product ids
                    .Where(product => product.Discounts != null && product.Discounts.Any()) // map to product ids
                    .ToArray();

                if (products.Length == 0)
                {
                    infoLabel.text = "No promotional offers available for any products";
                    return;
                }

                infoLabel.text = "";
                var storeProduct = products.First();
                purchases.GetPromotionalOffer(storeProduct, storeProduct.Discounts.First(),
                    (promoOffer, promoOfferError) =>
                    {
                        lock (this)
                        {
                            if (promoOfferError != null)
                            {
                                LogError(promoOfferError);
                            }
                            else
                            {
                                purchases.PurchaseDiscountedProduct(storeProduct.Identifier, promoOffer,
                                    (purchaseResult) =>
                                    {
                                        if (purchaseResult.Error != null)
                                        {
                                            if (purchaseResult.UserCancelled)
                                            {
                                                infoLabel.text = "purchase cancelled!";
                                            }
                                            else
                                            {
                                                LogError(purchaseResult.Error);
                                            }
                                        }
                                        else
                                        {
                                            infoLabel.text +=
                                                $"Purchase of {purchaseResult.ProductIdentifier} successful!\ncustomerInfo:\n{purchaseResult.CustomerInfo}";
                                        }
                                    });
                            }
                        }
                    });
            }
        });
    }

    void BuyFirstPackageWithDiscount()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                var packages = GetPackages(offerings)
                    .Where(pkg => pkg.StoreProduct.Discounts != null && pkg.StoreProduct.Discounts.Any()) // map to product ids
                    .ToArray();

                if (packages.Length == 0)
                {
                    infoLabel.text = "No promotional offers available for any products";
                    return;
                }

                infoLabel.text = "";
                var package = packages.First();
                purchases.GetPromotionalOffer(package.StoreProduct, package.StoreProduct.Discounts.First(),
                    (promoOffer, promoOfferError) =>
                    {
                        lock (this)
                        {
                            if (promoOfferError != null)
                            {
                                LogError(promoOfferError);
                            }
                            else
                            {
                                purchases.PurchaseDiscountedProduct(package.StoreProduct.Identifier, promoOffer,
                                    (purchaseResult) =>
                                    {
                                        if (purchaseResult.Error != null)
                                        {
                                            if (purchaseResult.UserCancelled)
                                            {
                                                infoLabel.text = "purchase cancelled!";
                                            }
                                            else
                                            {
                                                LogError(purchaseResult.Error);
                                            }
                                        }
                                        else
                                        {
                                            infoLabel.text +=
                                                $"Purchase of {purchaseResult.ProductIdentifier} successful!\ncustomerInfo:\n{purchaseResult.CustomerInfo}";
                                        }
                                    });
                            }
                        }
                    });
            }
        });
    }

    void PresentCodeRedemptionSheet()
    {
        infoLabel.text = "Presenting code redemption sheet";
        var purchases = GetComponent<Purchases>();
        purchases.PresentCodeRedemptionSheet();
    }

    void InvalidateCustomerInfoCache()
    {
        var purchases = GetComponent<Purchases>();
        purchases.InvalidateCustomerInfoCache();
        infoLabel.text = "customer info cache invalidated!";
    }

    void GetAllProducts()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                var productIds = GetPackages(offerings)
                    .Select(package => package.StoreProduct.Identifier)
                    .ToArray();

                // note: we're getting all offerings, then packages, then the products, then the product ids
                // and then we're fetching the products from those ids.
                // you'd never do this in practice, but it serves as a way to test the relevant methods.
                purchases.GetProducts(productIds, (products, innerError) =>
                {
                    if (innerError != null)
                    {
                        LogError(innerError);
                    }
                    else
                    {
                        var items = products.Select(arg => $"{arg.ToString()}");
                        infoLabel.text = $"{{ \n {string.Join(Environment.NewLine, items)}\n }} \n";
                    }
                });
            }
        });
    }

    void ToggleSimulatesAskToBuyInSandbox()
    {
        simulatesAskToBuyInSandbox = !simulatesAskToBuyInSandbox;
        var purchases = GetComponent<Purchases>();
        purchases.SetSimulatesAskToBuyInSandbox(simulatesAskToBuyInSandbox);
        infoLabel.text = $"simulatesAskToBuyInSandbox set to {simulatesAskToBuyInSandbox}";
    }

    void IsAnonymous()
    {
        var purchases = GetComponent<Purchases>();
        infoLabel.text = $"is anonymous: {purchases.IsAnonymous()}";
    }

    void IsConfigured()
    {
        var purchases = GetComponent<Purchases>();
        infoLabel.text = $"is configured: {purchases.IsConfigured()}";
    }

    void GetAppUserId()
    {
        var purchases = GetComponent<Purchases>();
        infoLabel.text = $"appUserId {purchases.GetAppUserId()}";
    }

    void ShowInAppMessages()
    {
        var purchases = GetComponent<Purchases>();
        purchases.ShowInAppMessages(new Purchases.InAppMessageType[] { Purchases.InAppMessageType.BillingIssue,
        Purchases.InAppMessageType.PriceIncreaseConsent, Purchases.InAppMessageType.Generic, Purchases.InAppMessageType.WinBackOffer });
    }

    void PurchaseProductForWinBackTesting()
    {
        LogWinBackOfferTestingInstructions();
        var purchases = GetComponent<Purchases>();
        purchases.GetProducts(new[] { "com.revenuecat.monthly_4.99.1_week_intro" }, (products, error) =>
        {
            if (error != null)
            {
                LogError(error);
                return;
            }
            else if (products != null)
            {
                var product = products[0];
                PurchaseProductButtonClicked(product);
            }
            else
            {
                infoLabel.text = "No product found";
            }
        });
    }

    void FetchAndRedeemWinBackForProduct()
    {
        LogWinBackOfferTestingInstructions();
        var purchases = GetComponent<Purchases>();
        purchases.GetProducts(new[] { "com.revenuecat.monthly_4.99.1_week_intro" }, (products, error) =>
        {
            if (error != null)
            {
                LogError(error);
                return;
            }
            else if (products != null)
            {
                var product = products[0];
                infoLabel.text = $"Got product: {product}";
                purchases.GetEligibleWinBackOffersForProduct(product, (winBackOffers, error) =>
                {
                    if (error != null)
                    {
                        LogError(error);
                        return;
                    }
                    else
                    {
                        if (winBackOffers != null && winBackOffers.Length > 0)
                        {
                            Debug.Log("Win-back offers: " + winBackOffers);
                            // Print the eligible win-back offers
                            string offerText = "Eligible win-back offers:\n";
                            foreach (var offer in winBackOffers)
                            {
                                offerText += $"- {offer.Identifier}\n";
                            }
                            infoLabel.text = offerText;

                            purchases.PurchaseProductWithWinBackOffer(product, winBackOffers[0], (purchaseResult) =>
                            {
                                if (purchaseResult.Error != null)
                                {
                                    LogError(purchaseResult.Error);
                                    Debug.Log($"productIdentifier: {purchaseResult.ProductIdentifier}, customerInfo: {purchaseResult.CustomerInfo}, userCancelled: {purchaseResult.UserCancelled}, purchaseError: {purchaseResult.Error}");
                                    return;
                                }
                                else
                                {
                                    infoLabel.text = $"Purchase of {purchaseResult.ProductIdentifier} successful!\ncustomerInfo:\n{purchaseResult.CustomerInfo}";
                                    Debug.Log($"productIdentifier: {purchaseResult.ProductIdentifier}, customerInfo: {purchaseResult.CustomerInfo}, userCancelled: {purchaseResult.UserCancelled}, purchaseError: {purchaseResult.Error}");
                                }
                            });
                        }
                        else
                        {
                            infoLabel.text = "No eligible win-back offers found";
                        }
                    }
                });
            }
            else
            {
                infoLabel.text = "No product found";
            }
        });
    }

    void PurchasePackageForWinBackTesting()
    {
        LogWinBackOfferTestingInstructions();
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
                return;
            }
            else
            {
                var package = offerings.Current.AvailablePackages.First();
                PurchasePackageButtonClicked(package);
            }
        });
    }

    void FetchAndRedeemWinBackForPackage()
    {
        LogWinBackOfferTestingInstructions();
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                var package = offerings.Current.AvailablePackages.First();
                infoLabel.text = $"Got package: {package}";
                purchases.GetEligibleWinBackOffersForPackage(package, (winBackOffers, error) =>
                {
                    if (error != null)
                    {
                        LogError(error);
                        return;
                    }
                    else
                    {
                        if (winBackOffers != null && winBackOffers.Length > 0)
                        {
                            // Print the eligible win-back offers
                            string offerText = "Eligible win-back offers:\n";
                            foreach (var offer in winBackOffers)
                            {
                                offerText += $"- {offer.Identifier}\n";
                            }
                            infoLabel.text = offerText;

                            purchases.PurchasePackageWithWinBackOffer(package, winBackOffers[0],
                                (purchaseResult) =>
                                {
                                    if (purchaseResult.Error != null)
                                    {
                                        LogError(purchaseResult.Error);
                                        Debug.Log($"productIdentifier: {purchaseResult.ProductIdentifier}, customerInfo: {purchaseResult.CustomerInfo}, userCancelled: {purchaseResult.UserCancelled}, purchaseError: {purchaseResult.Error}");
                                        return;
                                    }
                                    else
                                    {
                                        infoLabel.text = $"Purchase of {purchaseResult.ProductIdentifier} successful!\ncustomerInfo:\n{purchaseResult.CustomerInfo}";
                                        Debug.Log($"productIdentifier: {purchaseResult.ProductIdentifier}, customerInfo: {purchaseResult.CustomerInfo}, userCancelled: {purchaseResult.UserCancelled}, purchaseError: {purchaseResult.Error}");
                                    }
                                });
                        }
                        else
                        {
                            infoLabel.text = "No eligible win-back offers found";
                        }
                    }
                });
            }
        });
    }

    void LogWinBackOfferTestingInstructions()
    {
        Debug.Log("To test win-back offers, add Subtester/SKConfig.storekit to your Xcode project, ensure that it is selected in the scheme, and run the app on an iOS 18.0+ device/simulator with StoreKit 2 enabled.");
    }

    void GetAmazonLWAConsentStatus()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetAmazonLWAConsentStatus((status, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = "AmazonLWAConsentStatus: " + status.ToString();
            }
        });
    }

    public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        Debug.Log(string.Format("customer info received {0}", customerInfo.ActiveSubscriptions));

        DisplayCustomerInfo(customerInfo);
    }

    private void LogError(Purchases.Error error)
    {
        Debug.Log("Subtester: " + error.ToString());
        infoLabel.text = error.ToString();
    }

    private void DisplayCustomerInfo(Purchases.CustomerInfo customerInfo)
    {
        infoLabel.text = customerInfo.ToString();
    }

    List<Purchases.Package> GetPackages(Purchases.Offerings offerings)
    {
        return // get all products from the offerings
            offerings.All // unpack as dictionary
                .Values // to list of values
                .Select(offering => offering.AvailablePackages) // map to packages
                .SelectMany(x => x) // transform the list of lists of packages into a list of packages
                .ToList();
    }
}

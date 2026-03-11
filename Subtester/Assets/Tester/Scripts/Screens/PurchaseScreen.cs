using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RevenueCat.Tester.Screens
{
    public class PurchaseScreen : ScreenBase
    {
        private VisualElement _packagesContainer;
        private VisualElement _offeringPickerContainer;
        private Label _selectedOfferingLabel;
        private Purchases.ProrationMode _prorationMode = Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy;
        private string _currentProductId = "";
        private Label _prorationLabel;
        private Dictionary<string, Purchases.Offering> _allOfferings;
        private string _selectedOfferingId;
        private string _currentOfferingId;

        public PurchaseScreen(Purchases purchases, LogConsole console)
            : base(purchases, console) { }

        protected override void Build()
        {
            AddSectionHeader("Load Products");

            AddButton("Fetch All Offerings", FetchAllOfferings);

            _offeringPickerContainer = AddDynamicContainer();
            var hint = new Label("Tap \"Fetch All Offerings\" to see available offerings");
            hint.AddToClassList("info-label");
            _offeringPickerContainer.Add(hint);

            var placementField = AddTextField("Placement", "placement ID (optional)");
            AddSecondaryButton("Load from Placement", () =>
            {
                var id = placementField.value;
                if (string.IsNullOrWhiteSpace(id))
                {
                    LogError("Enter a placement ID");
                    return;
                }
                LoadFromPlacement(id);
            });

            AddSectionHeader("Packages");

            _selectedOfferingLabel = AddInfoLabel("No offering selected");

            _packagesContainer = AddDynamicContainer();
            var noProducts = new Label("Pick an offering above to populate purchase options");
            noProducts.AddToClassList("info-label");
            _packagesContainer.Add(noProducts);

            AddSectionHeader("Proration Mode (Android)");

            _prorationLabel = AddInfoLabel($"Current: {_prorationMode}");

            var row = new VisualElement();
            row.AddToClassList("button-row");
            foreach (Purchases.ProrationMode mode in Enum.GetValues(typeof(Purchases.ProrationMode)))
            {
                var m = mode;
                var btn = new Button(() =>
                {
                    _prorationMode = m;
                    _prorationLabel.text = $"Current: {m}";
                    Log($"Proration mode set to {m}");
                }) { text = mode.ToString() };
                btn.AddToClassList("secondary-button");
                btn.AddToClassList("small-button");
                row.Add(btn);
            }
            Content.Add(row);

            AddSectionHeader("Discounts");

            AddButtonRow(
                ("Buy Product w/ Discount", BuyFirstProductWithDiscount),
                ("Buy Package w/ Discount", BuyFirstPackageWithDiscount)
            );

            AddSectionHeader("Win-Back Offers");
            AddInfoLabel("Requires iOS 18.0+ with StoreKit config");

            AddButton("Purchase Product for WinBack Testing", () =>
            {
                Log("Purchasing product for win-back testing...");
                Purchases.GetProducts(new[] { "com.revenuecat.monthly_4.99.1_week_intro" }, (products, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    if (products == null || products.Count == 0) { LogError("No product found"); return; }
                    PurchaseProduct(products[0]);
                });
            });

            AddButton("Fetch & Redeem WinBack for Product", () =>
            {
                Log("Fetching win-back offers for product...");
                Purchases.GetProducts(new[] { "com.revenuecat.monthly_4.99.1_week_intro" }, (products, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    if (products == null || products.Count == 0) { LogError("No product found"); return; }

                    var product = products[0];
                    Purchases.GetEligibleWinBackOffersForProduct(product, (offers, wbError) =>
                    {
                        if (wbError != null) { LogError(wbError); return; }
                        if (offers == null || offers.Length == 0)
                        {
                            Log("No eligible win-back offers found");
                            return;
                        }
                        LogSuccess($"Found {offers.Length} win-back offer(s)");
                        foreach (var o in offers) Log($"  - {o.Identifier}");

                        Purchases.PurchaseProductWithWinBackOffer(product, offers[0], HandlePurchaseResult);
                    });
                });
            });

            AddButton("Fetch & Redeem WinBack for Package", () =>
            {
                Log("Fetching win-back offers for package...");
                Purchases.GetOfferings((offerings, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    var package = offerings.Current?.AvailablePackages?.FirstOrDefault();
                    if (package == null) { LogError("No packages available"); return; }

                    Purchases.GetEligibleWinBackOffersForPackage(package, (offers, wbError) =>
                    {
                        if (wbError != null) { LogError(wbError); return; }
                        if (offers == null || offers.Length == 0)
                        {
                            Log("No eligible win-back offers found");
                            return;
                        }
                        LogSuccess($"Found {offers.Length} win-back offer(s)");
                        foreach (var o in offers) Log($"  - {o.Identifier}");

                        Purchases.PurchasePackageWithWinBackOffer(package, offers[0], HandlePurchaseResult);
                    });
                });
            });
        }

        private void FetchAllOfferings()
        {
            Log("Fetching all offerings...");
            Purchases.GetOfferings((offerings, error) =>
            {
                if (error != null) { LogError(error); return; }

                _allOfferings = offerings.All;
                _currentOfferingId = offerings.Current?.Identifier;

                LogSuccess($"Found {_allOfferings.Count} offering(s)" +
                           (_currentOfferingId != null ? $", current: \"{_currentOfferingId}\"" : ""));

                if (_currentOfferingId != null && _allOfferings.ContainsKey(_currentOfferingId))
                {
                    SelectOffering(_allOfferings[_currentOfferingId]);
                }
                else
                {
                    PopulateOfferingPicker();
                }
            });
        }

        private void PopulateOfferingPicker()
        {
            _offeringPickerContainer.Clear();

            foreach (var kvp in _allOfferings)
            {
                var offering = kvp.Value;
                var isCurrent = offering.Identifier == _currentOfferingId;
                var isSelected = offering.Identifier == _selectedOfferingId;
                var suffix = isCurrent ? " (current)" : "";

                var label = $"{offering.Identifier}{suffix} — {offering.AvailablePackages.Count} pkg(s)";

                var o = offering;
                var btn = new Button(() => SelectOffering(o)) { text = label };

                btn.AddToClassList(isSelected ? "offering-selected" : "secondary-button");

                btn.AddToClassList("small-button");
                _offeringPickerContainer.Add(btn);
            }
        }

        private void SelectOffering(Purchases.Offering offering)
        {
            _selectedOfferingId = offering.Identifier;
            _selectedOfferingLabel.text = $"Showing: {offering.Identifier} — {offering.AvailablePackages.Count} package(s)";
            LogSuccess($"Selected offering: {offering.Identifier}");

            PopulateOfferingPicker();
            PopulatePackages(offering.AvailablePackages);
        }

        private void LoadFromPlacement(string placementId)
        {
            Log($"Loading offering for placement \"{placementId}\"...");
            Purchases.GetCurrentOfferingForPlacement(placementId, (offering, error) =>
            {
                if (error != null) { LogError(error); return; }
                if (offering == null) { Log("No offering for this placement"); return; }

                LogSuccess($"Loaded offering: {offering.Identifier}");
                PopulatePackages(offering.AvailablePackages);
            });
        }

        private void PopulatePackages(List<Purchases.Package> packages)
        {
            _packagesContainer.Clear();

            foreach (var package in packages)
            {
                if (package == null) continue;

                var group = new VisualElement();
                group.AddToClassList("package-group");

                var label = new Label($"{package.PackageType} — {package.StoreProduct.PriceString}");
                label.AddToClassList("dynamic-item-label");
                group.Add(label);

                var row = new VisualElement();
                row.AddToClassList("button-row");

                var pkg = package;
                var buyPkgBtn = new Button(() => PurchasePackage(pkg)) { text = "Buy Package" };
                buyPkgBtn.AddToClassList("action-button");
                buyPkgBtn.AddToClassList("small-button");
                row.Add(buyPkgBtn);

                var buyProdBtn = new Button(() => PurchaseProduct(pkg.StoreProduct)) { text = "Buy Product" };
                buyProdBtn.AddToClassList("secondary-button");
                buyProdBtn.AddToClassList("small-button");
                row.Add(buyProdBtn);

                group.Add(row);

                var options = package.StoreProduct.SubscriptionOptions;
                if (options != null)
                {
                    foreach (var option in options)
                    {
                        var parts = new List<string>();
                        if (option.PricingPhases != null)
                        {
                            foreach (var phase in option.PricingPhases)
                            {
                                var period = phase.BillingPeriod;
                                var price = phase.Price;
                                if (period != null && price != null)
                                    parts.Add($"{price.Formatted} / {period.ISO8601}");
                            }
                        }

                        var info = parts.Count > 0 ? string.Join(" → ", parts) : "No pricing info";
                        var opt = option;
                        var optBtn = new Button(() => PurchaseSubscriptionOption(opt))
                        {
                            text = info
                        };
                        optBtn.AddToClassList("secondary-button");
                        optBtn.AddToClassList("small-button");
                        group.Add(optBtn);
                    }
                }

                _packagesContainer.Add(group);
            }
        }

        private void PurchasePackage(Purchases.Package package)
        {
            Log($"Purchasing package: {package.PackageType}...");
            Purchases.PurchasePackage(package, HandlePurchaseResult, _currentProductId, _prorationMode);
        }

        private void PurchaseProduct(Purchases.StoreProduct product)
        {
            Log($"Purchasing product: {product.Identifier}...");
            Purchases.PurchaseProduct(product.Identifier, HandlePurchaseResult,
                "subs", null, _prorationMode, false);
        }

        private void PurchaseSubscriptionOption(Purchases.SubscriptionOption option)
        {
            Log("Purchasing subscription option...");
            Purchases.GoogleProductChangeInfo changeInfo = null;
            if (_prorationMode != Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy
                && !string.IsNullOrEmpty(_currentProductId))
            {
                changeInfo = new Purchases.GoogleProductChangeInfo(_currentProductId, _prorationMode);
            }
            Purchases.PurchaseSubscriptionOption(option, HandlePurchaseResult, changeInfo, false);
        }

        private void HandlePurchaseResult(Purchases.PurchaseResult result)
        {
            if (result.UserCancelled)
            {
                Log("Purchase cancelled by user");
                return;
            }
            if (result.Error != null)
            {
                LogError(result.Error);
                return;
            }
            LogSuccess($"Purchased: {result.ProductIdentifier}");
            _currentProductId = result.ProductIdentifier?.Split(':').FirstOrDefault() ?? "";
            Log($"Transaction: {result.StoreTransaction}");
            Log(result.CustomerInfo.ToString());
        }

        private void BuyFirstProductWithDiscount()
        {
            Log("Looking for products with discounts...");
            Purchases.GetOfferings((offerings, error) =>
            {
                if (error != null) { LogError(error); return; }

                Purchases.StoreProduct discountProduct = null;
                foreach (var offering in offerings.All.Values)
                {
                    foreach (var package in offering.AvailablePackages)
                    {
                        if (package.StoreProduct.Discounts != null && package.StoreProduct.Discounts.Any())
                        {
                            discountProduct = package.StoreProduct;
                            break;
                        }
                    }
                    if (discountProduct != null) break;
                }

                if (discountProduct == null)
                {
                    Log("No products with discounts found");
                    return;
                }

                Log($"Getting promo offer for {discountProduct.Identifier}...");
                Purchases.GetPromotionalOffer(discountProduct, discountProduct.Discounts.First(),
                    (promoOffer, promoError) =>
                    {
                        if (promoError != null) { LogError(promoError); return; }
                        Purchases.PurchaseDiscountedProduct(discountProduct.Identifier, promoOffer, HandlePurchaseResult);
                    });
            });
        }

        private void BuyFirstPackageWithDiscount()
        {
            Log("Looking for packages with discounts...");
            Purchases.GetOfferings((offerings, error) =>
            {
                if (error != null) { LogError(error); return; }

                Purchases.Package discountPackage = null;
                foreach (var offering in offerings.All.Values)
                {
                    foreach (var package in offering.AvailablePackages)
                    {
                        if (package.StoreProduct.Discounts != null && package.StoreProduct.Discounts.Any())
                        {
                            discountPackage = package;
                            break;
                        }
                    }
                    if (discountPackage != null) break;
                }

                if (discountPackage == null)
                {
                    Log("No packages with discounts found");
                    return;
                }

                var product = discountPackage.StoreProduct;
                Log($"Getting promo offer for {product.Identifier}...");
                Purchases.GetPromotionalOffer(product, product.Discounts.First(),
                    (promoOffer, promoError) =>
                    {
                        if (promoError != null) { LogError(promoError); return; }
                        Purchases.PurchaseDiscountedPackage(discountPackage, promoOffer, HandlePurchaseResult);
                    });
            });
        }
    }
}

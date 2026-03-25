namespace RevenueCat.Tester.Screens
{
    public class OfferingsScreen : ScreenBase
    {
        public OfferingsScreen(Purchases purchases, LogConsole console)
            : base(purchases, console) { }

        protected override void Build()
        {
            AddSectionHeader("Offerings");

            AddButton("Get Offerings", () =>
            {
                Log("Fetching offerings...");
                Purchases.GetOfferings((offerings, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    LogSuccess("Offerings received");
                    Log(offerings.ToString());
                });
            });

            AddSecondaryButton("Sync Attributes & Offerings", () =>
            {
                Log("Syncing attributes and offerings...");
                Purchases.SyncAttributesAndOfferingsIfNeeded((offerings, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    LogSuccess("Synced attributes and offerings");
                    Log(offerings.ToString());
                });
            });

            AddSectionHeader("Placements");

            var placementField = AddTextField("Placement ID", "e.g. onboarding", "onboarding");

            AddButton("Get Offering for Placement", () =>
            {
                var placementId = placementField.value;
                if (string.IsNullOrWhiteSpace(placementId))
                {
                    LogError("Enter a placement ID first");
                    return;
                }

                Log($"Fetching offering for placement \"{placementId}\"...");
                Purchases.GetCurrentOfferingForPlacement(placementId, (offering, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    if (offering == null)
                    {
                        Log($"No offering found for placement \"{placementId}\"");
                        return;
                    }
                    LogSuccess($"Offering for \"{placementId}\": {offering.Identifier}");
                    Log(offering.ToString());
                });
            });

            AddSectionHeader("Products");

            AddButton("Get All Products", () =>
            {
                Log("Fetching all products from offerings...");
                Purchases.GetOfferings((offerings, error) =>
                {
                    if (error != null) { LogError(error); return; }

                    var productIds = new System.Collections.Generic.List<string>();
                    foreach (var offering in offerings.All.Values)
                    {
                        foreach (var package in offering.AvailablePackages)
                        {
                            productIds.Add(package.StoreProduct.Identifier);
                        }
                    }

                    Purchases.GetProducts(productIds.ToArray(), (products, innerError) =>
                    {
                        if (innerError != null) { LogError(innerError); return; }
                        LogSuccess($"Got {products.Count} products");
                        foreach (var product in products)
                        {
                            Log(product.ToString());
                        }
                    });
                });
            });

            AddButton("Check Intro Eligibility", () =>
            {
                Log("Checking intro eligibility...");
                Purchases.GetOfferings((offerings, error) =>
                {
                    if (error != null) { LogError(error); return; }

                    var productIds = new System.Collections.Generic.List<string>();
                    foreach (var offering in offerings.All.Values)
                    {
                        foreach (var package in offering.AvailablePackages)
                        {
                            productIds.Add(package.StoreProduct.Identifier);
                        }
                    }

                    Purchases.CheckTrialOrIntroductoryPriceEligibility(productIds.ToArray(), eligibilities =>
                    {
                        LogSuccess("Intro eligibility results:");
                        foreach (var kvp in eligibilities)
                        {
                            Log($"  {kvp.Key}: {kvp.Value}");
                        }
                    });
                });
            });

            AddButton("Get Promotional Offers", () =>
            {
                Log("Fetching promotional offers...");
                Purchases.GetOfferings((offerings, error) =>
                {
                    if (error != null) { LogError(error); return; }

                    bool found = false;
                    foreach (var offering in offerings.All.Values)
                    {
                        foreach (var package in offering.AvailablePackages)
                        {
                            var product = package.StoreProduct;
                            if (product.Discounts == null || product.Discounts.Length == 0) continue;

                            found = true;
                            foreach (var discount in product.Discounts)
                            {
                                Purchases.GetPromotionalOffer(product, discount, (promoOffer, innerError) =>
                                {
                                    if (innerError != null) { LogError(innerError); return; }
                                    LogSuccess($"Promo offer: {promoOffer}");
                                });
                            }
                        }
                    }

                    if (!found)
                    {
                        Log("No products with promotional offers found");
                    }
                });
            });
        }
    }
}

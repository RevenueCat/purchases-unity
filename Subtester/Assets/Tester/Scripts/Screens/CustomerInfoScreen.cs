namespace RevenueCat.Tester.Screens
{
    public class CustomerInfoScreen : ScreenBase
    {
        public CustomerInfoScreen(Purchases purchases, LogConsole console)
            : base(purchases, console) { }

        protected override void Build()
        {
            AddSectionHeader("Customer Info");

            AddButton("Get Customer Info", () =>
            {
                Log("Fetching customer info...");
                Purchases.GetCustomerInfo((info, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    LogSuccess("Customer info received");
                    Log(info.ToString());

                    if (info.ActiveSubscriptions.Count > 0)
                    {
                        Log($"Active subscriptions: {string.Join(", ", info.ActiveSubscriptions)}");
                    }

                    foreach (var kvp in info.Entitlements.All)
                    {
                        var e = kvp.Value;
                        Log($"Entitlement \"{kvp.Key}\": active={e.IsActive}, " +
                            $"product={e.ProductIdentifier}, expires={e.ExpirationDate}");
                    }
                });
            });

            AddSecondaryButton("Invalidate Customer Info Cache", () =>
            {
                Purchases.InvalidateCustomerInfoCache();
                LogSuccess("Customer info cache invalidated");
            });

            AddSectionHeader("Virtual Currencies");

            AddButton("Get Virtual Currencies", () =>
            {
                Log("Fetching virtual currencies...");
                Purchases.GetVirtualCurrencies((currencies, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    LogSuccess($"Virtual currencies: {currencies}");
                });
            });

            AddSecondaryButton("Get Cached Virtual Currencies", () =>
            {
                var cached = Purchases.GetCachedVirtualCurrencies();
                Log(cached != null ? $"Cached virtual currencies: {cached}" : "Cached virtual currencies: null");
            });

            AddSecondaryButton("Invalidate Virtual Currencies Cache", () =>
            {
                Purchases.InvalidateVirtualCurrenciesCache();
                LogSuccess("Virtual currencies cache invalidated");
            });
        }
    }
}

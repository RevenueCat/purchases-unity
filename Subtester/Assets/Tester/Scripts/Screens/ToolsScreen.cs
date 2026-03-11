namespace RevenueCat.Tester.Screens
{
    public class ToolsScreen : ScreenBase
    {
        private bool _simulatesAskToBuy;

        public ToolsScreen(Purchases purchases, LogConsole console)
            : base(purchases, console) { }

        protected override void Build()
        {
            AddSectionHeader("Purchases");

            AddButton("Restore Purchases", () =>
            {
                Log("Restoring purchases...");
                Purchases.RestorePurchases((info, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    LogSuccess("Purchases restored");
                    Log(info.ToString());
                });
            });

            AddButton("Sync Purchases", () =>
            {
                Log("Syncing purchases...");
                Purchases.SyncPurchases((info, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    LogSuccess("Purchases synced");
                    Log(info.ToString());
                });
            });

            AddButton("Can Make Payments", () =>
            {
                Purchases.CanMakePayments((canMake, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    Log($"Can make payments: {canMake}");
                });
            });

            AddSectionHeader("Configuration");

            AddSecondaryButton("Toggle simulatesAskToBuyInSandbox", () =>
            {
                _simulatesAskToBuy = !_simulatesAskToBuy;
                Purchases.SetSimulatesAskToBuyInSandbox(_simulatesAskToBuy);
                Log($"simulatesAskToBuyInSandbox = {_simulatesAskToBuy}");
            });

            AddSecondaryButton("Present Code Redemption Sheet", () =>
            {
                Log("Presenting code redemption sheet...");
                Purchases.PresentCodeRedemptionSheet();
            });

            AddSecondaryButton("Show In-App Messages", () =>
            {
                Log("Showing in-app messages...");
                Purchases.ShowInAppMessages(new[]
                {
                    Purchases.InAppMessageType.BillingIssue,
                    Purchases.InAppMessageType.PriceIncreaseConsent,
                    Purchases.InAppMessageType.Generic,
                    Purchases.InAppMessageType.WinBackOffer
                });
            });

            AddSectionHeader("Info");

            AddSecondaryButton("Get Storefront", () =>
            {
                Purchases.GetStorefront((storefront) =>
                {
                    Log(storefront != null
                        ? $"Storefront: {storefront.CountryCode}"
                        : "Storefront: null");
                });
            });

            AddSecondaryButton("Get Amazon LWA Consent Status", () =>
            {
                Purchases.GetAmazonLWAConsentStatus((status, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    Log($"Amazon LWA Consent Status: {status}");
                });
            });

            AddSectionHeader("Log Level");

            AddButtonRow(
                ("Debug", () => SetLogLevel(Purchases.LogLevel.Debug)),
                ("Verbose", () => SetLogLevel(Purchases.LogLevel.Verbose))
            );

            AddSecondaryButtonRow(
                ("Info", () => SetLogLevel(Purchases.LogLevel.Info)),
                ("Warn", () => SetLogLevel(Purchases.LogLevel.Warn)),
                ("Error", () => SetLogLevel(Purchases.LogLevel.Error))
            );

            AddSectionHeader("Web Purchase Redemption");

            var urlField = AddTextField("URL", "paste deep link URL here");

            AddButton("Parse & Redeem URL", () =>
            {
                var url = urlField.value;
                if (string.IsNullOrWhiteSpace(url))
                {
                    LogError("Enter a URL first");
                    return;
                }
                RedeemUrl(url);
            });

            AddInfoLabel("Deep links are also handled automatically via DeepLinkListener");

            DeepLinkListener.OnDeepLinkReceived += (url) =>
            {
                Log($"[DeepLink] Received: {url}");
                RedeemUrl(url);
            };

            AddSectionHeader("Ad Services");

            AddSecondaryButton("Enable AdServices Attribution Token Collection", () =>
            {
                Purchases.EnableAdServicesAttributionTokenCollection();
                LogSuccess("AdServices attribution token collection enabled");
            });
        }

        private void RedeemUrl(string url)
        {
            Log($"Parsing URL: {url}");
            Purchases.ParseAsWebPurchaseRedemption(url, (webPurchaseRedemption) =>
            {
                if (webPurchaseRedemption == null)
                {
                    Log("URL is not a web purchase redemption link");
                    return;
                }

                Log($"Starting redemption: {webPurchaseRedemption}");
                Purchases.RedeemWebPurchase(webPurchaseRedemption, (result) =>
                {
                    switch (result)
                    {
                        case Purchases.WebPurchaseRedemptionResult.Success success:
                            LogSuccess($"Redemption successful: {success.CustomerInfo}");
                            break;
                        case Purchases.WebPurchaseRedemptionResult.RedemptionError error:
                            LogError($"Redemption failed: {error.Error}");
                            break;
                        case Purchases.WebPurchaseRedemptionResult.InvalidToken:
                            LogError("Redemption failed: Invalid token");
                            break;
                        case Purchases.WebPurchaseRedemptionResult.PurchaseBelongsToOtherUser:
                            LogError("Redemption failed: Purchase belongs to other user");
                            break;
                        case Purchases.WebPurchaseRedemptionResult.Expired expired:
                            Log($"Redemption expired. New email sent to {expired.ObfuscatedEmail}");
                            break;
                    }
                });
            });
        }

        private void SetLogLevel(Purchases.LogLevel level)
        {
            Purchases.SetLogLevel(level);
            Log($"Log level set to {level}");
        }
    }
}

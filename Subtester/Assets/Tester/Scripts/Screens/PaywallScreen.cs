using System.Linq;
using System.Threading.Tasks;
using RevenueCatUI;
using UnityEngine;

namespace RevenueCat.Tester.Screens
{
    public class PaywallScreen : ScreenBase
    {
        public PaywallScreen(Purchases purchases, LogConsole console)
            : base(purchases, console) { }

        protected override void Build()
        {
            AddSectionHeader("Paywall");

            AddButton("Present Paywall", async () =>
            {
                Log("Presenting paywall...");
                var result = await PaywallsPresenter.Present();
                LogPaywallResult("Paywall", result);
            });

            AddButton("Present Paywall (No Close Button)", async () =>
            {
                Log("Presenting paywall without close button...");
                var options = new PaywallOptions(displayCloseButton: false);
                var result = await PaywallsPresenter.Present(options);
                LogPaywallResult("Paywall (no close)", result);
            });

            AddButton("Present Paywall Full Screen", async () =>
            {
                Log("Presenting paywall full screen...");
                var options = new PaywallOptions(
                    presentationConfiguration: PaywallPresentationConfiguration.FullScreen
                );
                var result = await PaywallsPresenter.Present(options);
                LogPaywallResult("Paywall (full screen)", result);
            });

            AddButton("Present Paywall for Random Offering", async () =>
            {
                Log("Fetching random offering...");
                var offering = await GetRandomOfferingAsync();
                if (offering == null) return;

                Log($"Presenting paywall for offering: {offering.Identifier}");
                var options = new PaywallOptions(offering, displayCloseButton: true);
                var result = await PaywallsPresenter.Present(options);
                LogPaywallResult($"Paywall ({offering.Identifier})", result);
            });

            AddSectionHeader("Paywall If Needed");

            var entitlementField = AddTextField("Entitlement ID", "e.g. pro, premium");

            AddButton("Present If Needed", async () =>
            {
                var entitlementId = entitlementField.value;
                if (string.IsNullOrWhiteSpace(entitlementId))
                {
                    LogError("Enter an entitlement ID");
                    return;
                }

                Log($"Checking entitlement \"{entitlementId}\"...");
                var offering = await GetRandomOfferingAsync();
                var options = offering != null
                    ? new PaywallOptions(offering, displayCloseButton: true)
                    : new PaywallOptions(displayCloseButton: true);

                var result = await PaywallsPresenter.PresentIfNeeded(entitlementId, options);
                var extra = result.Result == PaywallResultType.NotPresented
                    ? " (user already has entitlement)"
                    : "";
                LogPaywallResult($"PaywallIfNeeded({entitlementId})", result, extra);
            });

            AddSectionHeader("Purchase Logic (MyApp mode)");

            var isMyApp = Purchases.purchasesAreCompletedBy == Purchases.PurchasesAreCompletedBy.MyApp;
            if (!isMyApp)
            {
                AddInfoLabel("Disabled — purchasesAreCompletedBy is not set to MyApp");
            }

            var purchaseLogicBtn = AddButton("Present Paywall w/ Purchase Logic", async () =>
            {
                Log("Presenting paywall with custom purchase logic...");
                var purchaseLogic = new PurchaseLogic(
                    performPurchase: async (purchaseParams) =>
                    {
                        var pkg = purchaseParams.PackageToPurchase;
                        Log($"PurchaseLogic: purchasing {pkg.Identifier}...");

                        var tcs = new TaskCompletionSource<PurchaseLogicResult>();
                        Purchases.PurchasePackage(pkg, (purchaseResult) =>
                        {
                            if (purchaseResult.UserCancelled)
                            {
                                Log("PurchaseLogic: user cancelled");
                                tcs.SetResult(PurchaseLogicResult.Cancellation);
                            }
                            else if (purchaseResult.Error != null)
                            {
                                LogError($"PurchaseLogic error: {purchaseResult.Error}");
                                tcs.SetResult(PurchaseLogicResult.Error);
                            }
                            else
                            {
                                LogSuccess("PurchaseLogic: purchase succeeded");
                                tcs.SetResult(PurchaseLogicResult.Success);
                            }
                        });
                        return await tcs.Task;
                    },
                    performRestore: async () =>
                    {
                        Log("PurchaseLogic: restoring...");
                        var tcs = new TaskCompletionSource<PurchaseLogicResult>();
                        Purchases.RestorePurchases((info, error) =>
                        {
                            if (error != null)
                            {
                                LogError($"PurchaseLogic restore error: {error}");
                                tcs.SetResult(PurchaseLogicResult.Error);
                            }
                            else
                            {
                                LogSuccess("PurchaseLogic: restore succeeded");
                                tcs.SetResult(PurchaseLogicResult.Success);
                            }
                        });
                        return await tcs.Task;
                    }
                );

                var options = new PaywallOptions(displayCloseButton: true, purchaseLogic: purchaseLogic);
                var result = await PaywallsPresenter.Present(options);
                LogPaywallResult("Paywall w/ PurchaseLogic", result);
            });

            if (!isMyApp) purchaseLogicBtn.SetEnabled(false);

            AddSectionHeader("Customer Center");

            AddButton("Present Customer Center", async () =>
            {
                Log("Presenting Customer Center...");
                var callbacks = new CustomerCenterCallbacks
                {
                    OnFeedbackSurveyCompleted = (args) =>
                        Log($"CC: Survey completed — option: {args.FeedbackSurveyOptionId}"),
                    OnShowingManageSubscriptions = () =>
                        Log("CC: Showing manage subscriptions"),
                    OnRestoreStarted = () =>
                        Log("CC: Restore started"),
                    OnRestoreCompleted = (args) =>
                        LogSuccess($"CC: Restore completed — {args.CustomerInfo}"),
                    OnRestoreFailed = (args) =>
                        LogError($"CC: Restore failed — {args.Error}"),
                    OnRefundRequestStarted = (args) =>
                        Log($"CC: Refund request started — product: {args.ProductIdentifier}"),
                    OnRefundRequestCompleted = (args) =>
                        Log($"CC: Refund request completed — product: {args.ProductIdentifier}, status: {args.RefundRequestStatus}"),
                    OnManagementOptionSelected = (args) =>
                    {
                        var url = args.Url != null ? $", URL: {args.Url}" : "";
                        Log($"CC: Management option — {args.Option}{url}");
                    },
                    OnCustomActionSelected = (args) =>
                    {
                        var purchase = args.PurchaseIdentifier != null ? $", purchase: {args.PurchaseIdentifier}" : "";
                        Log($"CC: Custom action — {args.ActionId}{purchase}");
                    }
                };

                await CustomerCenterPresenter.Present(callbacks);
                Log("Customer Center dismissed");
            });
        }

        private async Task<Purchases.Offering> GetRandomOfferingAsync()
        {
            var tcs = new TaskCompletionSource<Purchases.Offerings>();
            Purchases.GetOfferings((offerings, error) =>
            {
                if (error != null)
                    tcs.SetException(new System.Exception(error.ToString()));
                else
                    tcs.SetResult(offerings);
            });

            Purchases.Offerings result;
            try { result = await tcs.Task; }
            catch (System.Exception e)
            {
                LogError($"Failed to get offerings: {e.Message}");
                return null;
            }

            if (result?.All?.Count > 0)
            {
                var all = result.All.Values.ToList();
                return all[Random.Range(0, all.Count)];
            }

            return result?.Current;
        }

        private void LogPaywallResult(string label, PaywallResult result, string extra = "")
        {
            var status = result.Result switch
            {
                PaywallResultType.Purchased => "PURCHASED",
                PaywallResultType.Restored => "RESTORED",
                PaywallResultType.Cancelled => "CANCELLED",
                PaywallResultType.NotPresented => "NOT PRESENTED",
                PaywallResultType.Error => "ERROR",
                _ => $"UNKNOWN ({result.Result})"
            };

            var message = $"{label}: {status}{extra}";

            if (result.Result == PaywallResultType.Purchased || result.Result == PaywallResultType.Restored)
                LogSuccess(message);
            else if (result.Result == PaywallResultType.Error)
                LogError(message);
            else
                Log(message);
        }
    }
}

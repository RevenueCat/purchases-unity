using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RevenueCatUI;
using UnityEngine;

namespace RevenueCat.Tester.Screens
{
    public class PaywallScreen : ScreenBase
    {
        private UnityEngine.UIElements.TextField _offeringIdField;

        public PaywallScreen(Purchases purchases, LogConsole console, string defaultOfferingId = null, RevenueCatUI.PaywallsBehaviour.CustomVariableEntry[] defaultCustomVariables = null)
            : base(purchases, console)
        {
            if (!string.IsNullOrEmpty(defaultOfferingId) && _offeringIdField != null)
                _offeringIdField.value = defaultOfferingId;

            if (defaultCustomVariables != null)
            {
                for (int i = 0; i < defaultCustomVariables.Length && i < _customVarFields.Count; i++)
                {
                    _customVarFields[i].key.value = defaultCustomVariables[i].key ?? "";
                    _customVarFields[i].value.value = defaultCustomVariables[i].value ?? "";
                }
            }
        }

        private readonly List<(UnityEngine.UIElements.TextField key, UnityEngine.UIElements.TextField value)> _customVarFields = new();

        private Dictionary<string, CustomVariableValue> GetCustomVariables()
        {
            var vars = new Dictionary<string, CustomVariableValue>();
            foreach (var (keyField, valueField) in _customVarFields)
            {
                var key = keyField.value?.Trim();
                if (!string.IsNullOrEmpty(key))
                    vars[key] = CustomVariableValue.String(valueField.value ?? "");
            }
            return vars.Count > 0 ? vars : null;
        }

        private PaywallOptions BuildOptions(bool displayCloseButton = true, Purchases.Offering offering = null, PaywallPresentationConfiguration presentationConfiguration = null, PurchaseLogic purchaseLogic = null)
        {
            var customVars = GetCustomVariables();
            if (offering != null)
                return new PaywallOptions(offering, displayCloseButton: displayCloseButton, customVariables: customVars, presentationConfiguration: presentationConfiguration, purchaseLogic: purchaseLogic);
            return new PaywallOptions(displayCloseButton: displayCloseButton, customVariables: customVars, presentationConfiguration: presentationConfiguration, purchaseLogic: purchaseLogic);
        }

        private async Task<Purchases.Offering> GetOfferingByIdAsync(string offeringId)
        {
            if (string.IsNullOrWhiteSpace(offeringId)) return null;

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

            if (result?.All != null && result.All.TryGetValue(offeringId, out var offering))
                return offering;

            LogError($"Offering \"{offeringId}\" not found");
            return null;
        }

        protected override void Build()
        {
            AddSectionHeader("Custom Variables");
            AddInfoLabel("Variables use {{ custom.key }} syntax in V2 paywalls");

            for (int i = 0; i < 3; i++)
            {
                var row = new UnityEngine.UIElements.VisualElement();
                row.AddToClassList("button-row");
                var keyField = new UnityEngine.UIElements.TextField("Key") { value = "" };
                keyField.AddToClassList("input-field");
                var valueField = new UnityEngine.UIElements.TextField("Value") { value = "" };
                valueField.AddToClassList("input-field");
                row.Add(keyField);
                row.Add(valueField);
                Content.Add(row);
                _customVarFields.Add((keyField, valueField));
            }

            AddSectionHeader("Paywall");

            _offeringIdField = AddTextField("Offering ID", "Leave empty for current offering");

            AddButton("Present Paywall", async () =>
            {
                var offering = await GetOfferingByIdAsync(_offeringIdField.value);
                Log("Presenting paywall...");
                var result = await PaywallsPresenter.Present(BuildOptions(offering: offering));
                LogPaywallResult("Paywall", result);
            });

            AddButton("Present Paywall (No Close Button)", async () =>
            {
                var offering = await GetOfferingByIdAsync(_offeringIdField.value);
                Log("Presenting paywall without close button...");
                var result = await PaywallsPresenter.Present(BuildOptions(displayCloseButton: false, offering: offering));
                LogPaywallResult("Paywall (no close)", result);
            });

            AddButton("Present Paywall Full Screen", async () =>
            {
                var offering = await GetOfferingByIdAsync(_offeringIdField.value);
                Log("Presenting paywall full screen...");
                var result = await PaywallsPresenter.Present(BuildOptions(offering: offering, presentationConfiguration: PaywallPresentationConfiguration.FullScreen));
                LogPaywallResult("Paywall (full screen)", result);
            });

            AddButton("Present Paywall for Random Offering", async () =>
            {
                Log("Fetching random offering...");
                var offering = await GetRandomOfferingAsync();
                if (offering == null) return;

                Log($"Presenting paywall for offering: {offering.Identifier}");
                var result = await PaywallsPresenter.Present(BuildOptions(offering: offering));
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
                var options = BuildOptions(offering: offering);

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

                var options = BuildOptions(purchaseLogic: purchaseLogic);
                var result = await PaywallsPresenter.Present(options);
                LogPaywallResult("Paywall w/ PurchaseLogic", result);
            });

            if (!isMyApp) purchaseLogicBtn.SetEnabled(false);

            AddSectionHeader("Custom Paywall Events");

            var paywallIdField = AddTextField("Paywall ID", "Optional — leave empty for no ID");
            var offeringIdField = AddTextField("Offering ID", "Optional — leave empty for no offering ID");

            AddButton("Track Custom Paywall Impression", () =>
            {
                var paywallId = paywallIdField.value?.Trim();
                var offeringId = offeringIdField.value?.Trim();
                if (string.IsNullOrEmpty(paywallId)) paywallId = null;
                if (string.IsNullOrEmpty(offeringId)) offeringId = null;

                if (paywallId == null && offeringId == null)
                {
                    Log("Tracking custom paywall impression (no params)...");
                    Purchases.TrackCustomPaywallImpression();
                }
                else
                {
                    Log($"Tracking custom paywall impression (paywallId: \"{paywallId}\", offeringId: \"{offeringId}\")...");
                    Purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams(paywallId, offeringId));
                }
                LogSuccess("Custom paywall impression tracked");
            });

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

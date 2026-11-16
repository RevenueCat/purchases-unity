#pragma warning disable CS0618 // Checkpoints are experimental on purpose.
// Both players need a native accessibility overlay for Maestro to see the uGUI.
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
#define ACCESSIBILITY_OVERLAY
#endif

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RevenueCatUI;

[DefaultExecutionOrder(100)]
public class MaestroTestApp : Purchases.UpdatedCustomerInfoListener
{
    private const string API_KEY = "MAESTRO_TESTS_REVENUECAT_API_KEY";

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetLaunchTestFlow();

    [DllImport("__Internal")]
    private static extern string GetLaunchTestApiKey();
#endif

    private static readonly Dictionary<string, System.Action<MaestroTestApp>> TestFlowScreenMap =
        new Dictionary<string, System.Action<MaestroTestApp>>
        {
            { "purchase_through_paywall", app => app.ShowPurchaseScreen() },
            { "paywall_exit_offer_dismisses", app => app.ShowExitOfferScreen() },
            // Presents its paywall to every user, with the monthly package preselected.
            { "checkpoint_purchase", app => app.ShowCheckpointScreen("donottouch_sdk_everyone") },
            // A well-formed identifier that is not configured in the dashboard.
            { "checkpoint_unknown_identifier", app => app.ShowCheckpointScreen("maestro_unknown_checkpoint") },
            { "checkpoint_custom_presenter_purchase", app => app.ShowCustomCheckpointScreen() },
            { "checkpoint_custom_presenter_back", app => app.ShowCustomCheckpointScreen() },
            { "checkpoint_custom_presenter_unmount", app => app.ShowCustomCheckpointScreen() },
            // An invalid identifier fails the checkpoint without relying on a network outage.
            { "checkpoint_custom_error_dialog", app => app.ShowCheckpointScreen("!", customErrorDialog: true) }
        };

    public GameObject testCasesScreen;
    public GameObject purchaseScreen;
    public Text entitlementsLabel;
    public Text paywallResultLabel;
    public Text errorLabel;

    public GameObject checkpointScreen;
    public Text checkpointIdentifierLabel;
    public Text checkpointStatusLabel;
    public Text[] checkpointLineLabels;
    public Text checkpointErrorLabel;

    public GameObject customCheckpointScreen;

    private const string CheckpointEntitlementId = "pro";
    private const string CheckpointProductId = "pro_monthly_subscription";

    private Purchases purchases;
    private string checkpointIdentifier;
    private int checkpointInvocations;
    private System.Action detachCheckpoint;
    private bool customErrorDialog;
    private bool useExitOfferTestOffering;

    // The custom presenter screen. The checkpoint owner stands in for a screen that registers the
    // checkpoint, while this screen owns the paywall UI, so unmounting the owner detaches a real
    // registration without dismissing the paywall. The counters expose premature callbacks and
    // callbacks from a detached owner instead of hiding them.
    private sealed class CheckpointOwner
    {
        internal int Id;
        internal System.Action Detach;
    }

    private CheckpointOwner checkpointOwner;
    private int nextOwnerId;
    private PaywallPresentationParams presentation;
    private System.Action<PaywallPresentationResult> completePresentation;
    private bool awaitingCompletion;
    private int presentationCount;
    private int callbackCount;
    private int earlyCallbacks;
    private int staleCallbacks;
    private int completionCount;
    private string lastCompletion;
    private string customEntitlements;
    private string purchaseStatus;
    private string customError;

#if ACCESSIBILITY_OVERLAY
    private bool overlayReady;
#if UNITY_ANDROID
    private AndroidJavaClass nativeOverlay;
#else
    [DllImport("__Internal")]
    private static extern void NativeAccessibilityOverlayInit();

    [DllImport("__Internal")]
    private static extern void NativeAccessibilityOverlaySetElement(string id, string text,
        int left, int top, int right, int bottom);

    [DllImport("__Internal")]
    private static extern void NativeAccessibilityOverlayClear();
#endif
#endif

    void Start()
    {
        Debug.Log("MaestroTestApp: Start() called");
        purchases = GetComponent<Purchases>();

        // Local runs pass a Test Store key as a launch argument so it never has to be written to source.
        var testApiKey = GetLaunchArgument("rc_test_api_key");
        var apiKey = string.IsNullOrEmpty(testApiKey) ? API_KEY : testApiKey;
        var config = Purchases.PurchasesConfiguration.Builder.Init(apiKey).Build();
        purchases.Configure(config);
        purchases.SetLogLevel(Purchases.LogLevel.Verbose);
        purchases.listener = this;
        Debug.Log("MaestroTestApp: Purchases configured");

        if (errorLabel != null)
        {
            errorLabel.gameObject.SetActive(false);
        }

#if ACCESSIBILITY_OVERLAY
        InitOverlay();
#endif

        WireButtons();

        string testFlow = GetTestFlow();
        if (testFlow != null && TestFlowScreenMap.TryGetValue(testFlow, out var navigateAction))
        {
            Debug.Log($"MaestroTestApp: launching straight into '{testFlow}'");
            navigateAction(this);
        }
        else
        {
            ShowTestCases();
        }
    }

    private string GetTestFlow() => GetLaunchArgument("e2e_test_flow");

    private string GetLaunchArgument(string name)
    {
#if UNITY_IOS && !UNITY_EDITOR
        return name == "rc_test_api_key" ? GetLaunchTestApiKey() : GetLaunchTestFlow();
#elif UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = activity.Call<AndroidJavaObject>("getIntent");
            return intent.Call<string>("getStringExtra", name);
        }
        catch (System.Exception)
        {
            return null;
        }
#else
        return null;
#endif
    }

    // The buttons are wired here rather than in the scene because SceneSetup builds
    // Main.unity from an editor script, and only persistent listeners survive
    // serialization. Anything added there with onClick.AddListener is dropped when the
    // scene is saved, which ships an app whose buttons do nothing.
    private void WireButtons()
    {
        WireButton(testCasesScreen, "PurchaseButton", ShowPurchaseScreen);
        WireButton(testCasesScreen, "CheckpointButton", () => ShowCheckpointScreen("donottouch_sdk_everyone"));
        WireButton(testCasesScreen, "CheckpointUnknownButton", () => ShowCheckpointScreen("maestro_unknown_checkpoint"));
        WireButton(checkpointScreen, "HitCheckpointButton", HitCheckpoint);
        WireButton(testCasesScreen, "CustomCheckpointPresenterButton", ShowCustomCheckpointScreen);
        WireButton(testCasesScreen, "CheckpointCustomErrorDialogButton", () => ShowCheckpointScreen("!", customErrorDialog: true));
        WireButton(checkpointScreen, "BackButton", ShowTestCases);
        WireButton(customCheckpointScreen, "HitCustomCheckpointButton", HitCustomCheckpoint);
        WireButton(customCheckpointScreen, "RemountCheckpointOwnerButton", RemountCheckpointOwner);
        WireButton(customCheckpointScreen, "BuyButton", BuyMonthlyPackage);
        WireButton(customCheckpointScreen, "ContinueButton", () => FinishPresentation(PaywallPresentationResult.Continued));
        WireButton(customCheckpointScreen, "CloseButton", () => FinishPresentation(PaywallPresentationResult.Closed));
        WireButton(customCheckpointScreen, "BackFromPaywallButton", () => FinishPresentation(PaywallPresentationResult.NavigatedBack));
        WireButton(customCheckpointScreen, "UnmountCheckpointOwnerButton", UnmountCheckpointOwner);
        WireButton(customCheckpointScreen, "BackButton", ShowTestCases);
        WireButton(purchaseScreen, "PaywallButton", PresentPaywall);
        WireButton(purchaseScreen, "BackButton", ShowTestCases);
    }

    private void WireButton(GameObject screen, string buttonName, UnityEngine.Events.UnityAction action)
    {
        var button = screen == null ? null : screen.transform.Find(buttonName)?.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"MaestroTestApp: could not find button '{buttonName}'");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        Debug.Log($"MaestroTestApp: wired button '{buttonName}'");
    }

    public void ShowTestCases()
    {
        detachCheckpoint?.Invoke();
        detachCheckpoint = null;
        checkpointOwner?.Detach?.Invoke();
        checkpointOwner = null;
        testCasesScreen.SetActive(true);
        purchaseScreen.SetActive(false);
        checkpointScreen.SetActive(false);
        customCheckpointScreen.SetActive(false);
#if ACCESSIBILITY_OVERLAY
        StartCoroutine(UpdateOverlay("testCases"));
#endif
    }

    public void ShowPurchaseScreen()
    {
        Debug.Log("MaestroTestApp: ShowPurchaseScreen() called");
        testCasesScreen.SetActive(false);
        purchaseScreen.SetActive(true);
        checkpointScreen.SetActive(false);
        customCheckpointScreen.SetActive(false);
        ClearError();
        if (paywallResultLabel != null)
        {
            paywallResultLabel.text = "Paywall result: not completed";
        }
        UpdateEntitlements();
#if ACCESSIBILITY_OVERLAY
        StartCoroutine(UpdateOverlay("purchase"));
#endif
    }

    private void ShowExitOfferScreen()
    {
        useExitOfferTestOffering = true;
        ShowPurchaseScreen();
    }

    public void ShowCheckpointScreen(string identifier, bool customErrorDialog = false)
    {
        Debug.Log($"MaestroTestApp: ShowCheckpointScreen({identifier}) called");
        detachCheckpoint?.Invoke();
        detachCheckpoint = null;
        checkpointIdentifier = identifier;
        this.customErrorDialog = customErrorDialog;
        checkpointInvocations = 0;
        testCasesScreen.SetActive(false);
        purchaseScreen.SetActive(false);
        checkpointScreen.SetActive(true);
        customCheckpointScreen.SetActive(false);
        checkpointErrorLabel.gameObject.SetActive(false);
        checkpointIdentifierLabel.text = "Identifier: " + identifier;
        checkpointStatusLabel.text = "Callback: not invoked";
        foreach (var line in checkpointLineLabels) line.text = "";
#if ACCESSIBILITY_OVERLAY
        StartCoroutine(UpdateOverlay("checkpoint"));
#endif
    }

    private void HitCheckpoint()
    {
        checkpointErrorLabel.gameObject.SetActive(false);
        detachCheckpoint?.Invoke();
        try
        {
            var parameters = new CheckpointParams(errorDialogFactory: customErrorDialog
                ? _ => Task.FromResult(CheckpointErrorDialogOutcome.Show(
                    new CheckpointErrorDialog("Custom checkpoint error dialog", "Got it")))
                : (CheckpointErrorDialogFactory)null);
            detachCheckpoint = Checkpoints.Checkpoint(checkpointIdentifier, OnCheckpointPassed, parameters);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MaestroTestApp: failed to hit checkpoint: {e}");
            checkpointErrorLabel.text = "Error: " + e.Message;
            checkpointErrorLabel.gameObject.SetActive(true);
        }
    }

    private void OnCheckpointPassed(CheckpointResult result)
    {
        checkpointInvocations++;
        var entitlements = result.Flow == null
            ? new List<string>()
            : result.Flow.ObtainedEntitlements.Select(value => value.EntitlementInfo.Identifier).ToList();
        var products = result.Flow == null
            ? new List<string>()
            : result.Flow.ObtainedProducts.Select(value => value.Product.Identifier).ToList();

        checkpointStatusLabel.text = result.Flow == null
            ? $"Callback: passed, no flow (call #{checkpointInvocations})"
            : $"Callback: passed, entitlements: {ListOrNone(entitlements)} (call #{checkpointInvocations})";
        // One line per fact, without regex specials, so Maestro matches each literally.
        checkpointLineLabels[0].text = $"Obtained products: {ListOrNone(products)}";
        checkpointLineLabels[1].text = $"Entitlement {CheckpointEntitlementId} obtained: {(entitlements.Contains(CheckpointEntitlementId) ? "true" : "false")}";
        checkpointLineLabels[2].text = $"Product {CheckpointProductId} obtained: {(products.Contains(CheckpointProductId) ? "true" : "false")}";
        checkpointLineLabels[3].text = $"Customer active entitlements: {ListOrNone(result.Customer.Entitlements.Active.Keys)}";
#if ACCESSIBILITY_OVERLAY
        StartCoroutine(UpdateOverlay("checkpoint"));
#endif
    }

    public void ShowCustomCheckpointScreen()
    {
        Debug.Log("MaestroTestApp: ShowCustomCheckpointScreen() called");
        detachCheckpoint?.Invoke();
        detachCheckpoint = null;
        checkpointOwner?.Detach?.Invoke();
        checkpointOwner = new CheckpointOwner { Id = ++nextOwnerId };
        presentation = null;
        completePresentation = null;
        awaitingCompletion = false;
        presentationCount = callbackCount = earlyCallbacks = staleCallbacks = completionCount = 0;
        lastCompletion = "none";
        customEntitlements = "none";
        purchaseStatus = "not started";
        customError = null;
        testCasesScreen.SetActive(false);
        purchaseScreen.SetActive(false);
        checkpointScreen.SetActive(false);
        customCheckpointScreen.SetActive(true);
        RenderCustomCheckpoint();
    }

    private void HitCustomCheckpoint()
    {
        var owner = checkpointOwner;
        if (owner == null) return;
        owner.Detach?.Invoke();
        customError = null;
        try
        {
            owner.Detach = Checkpoints.Checkpoint("donottouch_sdk_everyone",
                result => OnCustomCheckpointPassed(owner.Id, result),
                new CheckpointParams(
                    customVariables: new Dictionary<string, CustomVariableValue>
                    {
                        { "name", CustomVariableValue.String("Maestro") },
                        { "level", CustomVariableValue.Number(42) },
                        { "premium", CustomVariableValue.Boolean(true) }
                    },
                    presentationMode: FlowPresentationMode.ModalFullScreen,
                    paywallPresenter: PresentCustomPaywall));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"MaestroTestApp: failed to hit custom checkpoint: {e}");
            customError = e.Message;
        }
        RenderCustomCheckpoint();
    }

    private void PresentCustomPaywall(PaywallPresentationParams parameters, System.Action<PaywallPresentationResult> completion)
    {
        awaitingCompletion = true;
        presentationCount++;
        purchaseStatus = "not started";
        presentation = parameters;
        completePresentation = completion;
        RenderCustomCheckpoint();
    }

    private void OnCustomCheckpointPassed(int ownerId, CheckpointResult result)
    {
        callbackCount++;
        if (awaitingCompletion) earlyCallbacks++;
        if (checkpointOwner == null || checkpointOwner.Id != ownerId) staleCallbacks++;
        customEntitlements = ListOrNone(result.Customer.Entitlements.Active.Keys);
        RenderCustomCheckpoint();
    }

    private void FinishPresentation(PaywallPresentationResult result)
    {
        var completion = completePresentation;
        if (presentation == null || completion == null || purchaseStatus == "in progress") return;
        presentation = null;
        completePresentation = null;
        RenderCustomCheckpoint();
        StartCoroutine(CompleteAfterDismissal(completion, result));
    }

    // Remove the app-owned UI for a frame before resuming native work, as an app dismissing its paywall would.
    private IEnumerator CompleteAfterDismissal(System.Action<PaywallPresentationResult> completion, PaywallPresentationResult result)
    {
        yield return null;
        awaitingCompletion = false;
        completion(result);
        completionCount++;
        lastCompletion = result == PaywallPresentationResult.Continued ? "CONTINUED"
            : result == PaywallPresentationResult.Closed ? "CLOSED" : "NAVIGATED_BACK";
        RenderCustomCheckpoint();
    }

    private Purchases.Package MonthlyPackage => presentation?.Offering.AvailablePackages
        .FirstOrDefault(package => package.StoreProduct.Identifier == CheckpointProductId);

    private void BuyMonthlyPackage()
    {
        var package = MonthlyPackage;
        if (package == null || purchaseStatus == "in progress" || purchaseStatus == "complete") return;
        purchaseStatus = "in progress";
        customError = null;
        RenderCustomCheckpoint();
        // Use the package from the presenter so the purchase keeps its offering context.
        purchases.PurchasePackage(package, result =>
        {
            if (result.Error != null)
            {
                purchaseStatus = "failed";
                customError = result.Error.Message;
            }
            else
            {
                purchaseStatus = result.UserCancelled ? "cancelled" : "complete";
            }
            RenderCustomCheckpoint();
        });
    }

    private void UnmountCheckpointOwner()
    {
        if (purchaseStatus == "in progress" || checkpointOwner == null) return;
        checkpointOwner.Detach?.Invoke();
        checkpointOwner = null;
        RenderCustomCheckpoint();
    }

    private void RemountCheckpointOwner()
    {
        if (checkpointOwner != null || presentation != null) return;
        checkpointOwner = new CheckpointOwner { Id = ++nextOwnerId };
        RenderCustomCheckpoint();
    }

    private string DescribeVariable(string key)
    {
        if (presentation == null || !presentation.CustomVariables.TryGetValue(key, out var value)) return "missing";
        if (value.Equals(CustomVariableValue.String("Maestro"))) return "string Maestro";
        if (value.Equals(CustomVariableValue.Number(42))) return "number 42";
        if (value.Equals(CustomVariableValue.Boolean(true))) return "boolean true";
        return "unexpected " + value;
    }

    private void RenderCustomCheckpoint()
    {
        var screen = customCheckpointScreen.transform;
        void Set(string name, string text) => screen.Find(name + "Label").GetComponent<Text>().text = text;
        void Show(string name, bool visible) => screen.Find(name).gameObject.SetActive(visible);

        var presenting = presentation != null;
        var purchasing = purchaseStatus == "in progress";
        Set("Owner", "Checkpoint owner: " + (checkpointOwner == null ? "unmounted" : "mounted"));
        Set("PresentationCount", $"Presentation count: {presentationCount}");
        Set("CallbackCount", $"Callback count: {callbackCount}");
        Set("EarlyCallbacks", $"Callbacks before completion: {earlyCallbacks}");
        Set("StaleCallbacks", $"Stale callbacks: {staleCallbacks}");
        Set("CompletionCount", $"Completion count: {completionCount}");
        Set("LastCompletion", $"Last completion: {lastCompletion}");
        Set("Entitlements", $"Customer active entitlements: {customEntitlements}");
        Set("Error", customError == null ? "" : "Error: " + customError);

        Set("PaywallTitle", presenting ? "Custom checkpoint paywall" : "");
        Set("PaywallCheckpointIdentifier", presenting ? $"Checkpoint identifier: {presentation.CheckpointIdentifier}" : "");
        Set("PaywallOffering", presenting ? $"Offering identifier: {presentation.Offering.Identifier}" : "");
        Set("PaywallProduct", presenting ? $"Product available: {MonthlyPackage?.StoreProduct.Identifier ?? "missing"}" : "");
        Set("PaywallMode", presenting ? $"Presentation mode: {(presentation.PresentationMode == FlowPresentationMode.ModalFullScreen ? "MODAL_FULL_SCREEN" : presentation.PresentationMode == FlowPresentationMode.ModalSheet ? "MODAL_SHEET" : "DEFAULT")}" : "");
        Set("PaywallCustomString", presenting ? $"Custom string: {DescribeVariable("name")}" : "");
        Set("PaywallCustomNumber", presenting ? $"Custom number: {DescribeVariable("level")}" : "");
        Set("PaywallCustomBoolean", presenting ? $"Custom boolean: {DescribeVariable("premium")}" : "");
        Set("PaywallPurchase", presenting ? $"Purchase: {purchaseStatus}" : "");

        Show("HitCustomCheckpointButton", checkpointOwner != null && !presenting);
        Show("RemountCheckpointOwnerButton", checkpointOwner == null && !presenting);
        Show("BuyButton", presenting && MonthlyPackage != null && !purchasing && purchaseStatus != "complete");
        Show("ContinueButton", presenting && !purchasing);
        Show("CloseButton", presenting && !purchasing);
        Show("BackFromPaywallButton", presenting && !purchasing);
        Show("UnmountCheckpointOwnerButton", presenting && !purchasing && checkpointOwner != null);
#if ACCESSIBILITY_OVERLAY
        StartCoroutine(UpdateOverlay("customCheckpoint"));
#endif
    }

    private static string ListOrNone(IEnumerable<string> values)
    {
        var sorted = values.OrderBy(value => value, System.StringComparer.Ordinal).ToList();
        return sorted.Count == 0 ? "none" : string.Join(", ", sorted);
    }

    public void PresentPaywall()
    {
        ClearError();

        if (useExitOfferTestOffering)
        {
            purchases.GetOfferings((offerings, error) =>
            {
                if (error != null || offerings == null ||
                    !offerings.All.TryGetValue("default_workflows", out var offering))
                {
                    ShowError("Could not load default_workflows offering");
                    return;
                }

                _ = PresentPaywallAsync(offering);
            });
            return;
        }

        _ = PresentPaywallAsync(null);
    }

    private async Task PresentPaywallAsync(Purchases.Offering offering)
    {
        // Present() traps its own exceptions and reports failure through the result, so
        // there is nothing here to catch; the result has to be inspected instead.
        var options = offering == null ? new PaywallOptions() : new PaywallOptions(offering);
        var result = await PaywallsPresenter.Present(options);

        if (paywallResultLabel != null)
        {
            paywallResultLabel.text = "Paywall result: " + result.Result;
#if ACCESSIBILITY_OVERLAY
            StartCoroutine(UpdateOverlay("purchase"));
#endif
        }

        if (result.Result == PaywallResultType.Error)
        {
            ShowError("Paywall presentation failed");
            return;
        }

        // Kick off a refresh rather than relying on CustomerInfoReceived alone. The fetch
        // is async, so the label changes only once the callback lands, which is why the
        // flows wait for the entitlement text instead of asserting on it straight away.
        UpdateEntitlements();
    }

    public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        UpdateEntitlementsFromInfo(customerInfo);
    }

    private void UpdateEntitlements()
    {
        purchases.GetCustomerInfo((info, error) =>
        {
            if (error != null)
            {
                ShowError(error.Message);
                return;
            }

            if (info != null)
            {
                UpdateEntitlementsFromInfo(info);
            }
        });
    }

    private void UpdateEntitlementsFromInfo(Purchases.CustomerInfo info)
    {
        bool hasPro = info.Entitlements.Active.ContainsKey("pro");
        if (entitlementsLabel != null)
        {
            string text = "Entitlements: " + (hasPro ? "pro" : "none");
            entitlementsLabel.text = text;
#if ACCESSIBILITY_OVERLAY
            // Customer info also arrives while the test cases screen is showing. Exposing
            // the label then would let an "Entitlements: ..." assertion pass on the wrong
            // screen and hide a navigation that never happened.
            if (purchaseScreen != null && purchaseScreen.activeSelf)
            {
                SetOverlayElement("entitlements", text, entitlementsLabel.rectTransform);
            }
#endif
        }
    }

    private void ShowError(string message)
    {
        if (errorLabel != null)
        {
            errorLabel.text = "Error: " + message;
            errorLabel.gameObject.SetActive(true);
        }
    }

    private void ClearError()
    {
        if (errorLabel != null)
        {
            errorLabel.gameObject.SetActive(false);
        }
    }

#if ACCESSIBILITY_OVERLAY
    private void InitOverlay()
    {
        try
        {
#if UNITY_ANDROID
            nativeOverlay = new AndroidJavaClass("com.revenuecat.accessibility.NativeAccessibilityOverlay");
            nativeOverlay.CallStatic("init");
#else
            NativeAccessibilityOverlayInit();
#endif
            overlayReady = true;
            Debug.Log("MaestroTestApp: NativeAccessibilityOverlay initialized");
        }
        catch (System.Exception e)
        {
            Debug.LogError("NativeAccessibilityOverlay init failed: " + e);
            overlayReady = false;
        }
    }

    private IEnumerator UpdateOverlay(string screen)
    {
        yield return null; // wait one frame for layout

        if (!overlayReady) yield break;
#if UNITY_ANDROID
        nativeOverlay.CallStatic("clear");
#else
        NativeAccessibilityOverlayClear();
#endif

        if (screen == "testCases")
        {
            var title = testCasesScreen.transform.Find("Title");
            if (title != null)
                SetOverlayElement("title", "Test Cases", title.GetComponent<RectTransform>());

            var btn = testCasesScreen.transform.Find("PurchaseButton");
            if (btn != null)
                SetOverlayElement("purchaseBtn", "Purchase through paywall", btn.GetComponent<RectTransform>());

            var checkpointBtn = testCasesScreen.transform.Find("CheckpointButton");
            if (checkpointBtn != null)
                SetOverlayElement("checkpointBtn", "Checkpoint", checkpointBtn.GetComponent<RectTransform>());

            var unknownBtn = testCasesScreen.transform.Find("CheckpointUnknownButton");
            if (unknownBtn != null)
                SetOverlayElement("checkpointUnknownBtn", "Checkpoint unknown identifier", unknownBtn.GetComponent<RectTransform>());

            var customPresenterBtn = testCasesScreen.transform.Find("CustomCheckpointPresenterButton");
            if (customPresenterBtn != null)
                SetOverlayElement("customCheckpointPresenterBtn", "Custom checkpoint presenter", customPresenterBtn.GetComponent<RectTransform>());

            var customErrorDialogBtn = testCasesScreen.transform.Find("CheckpointCustomErrorDialogButton");
            if (customErrorDialogBtn != null)
                SetOverlayElement("checkpointCustomErrorDialogBtn", "Checkpoint custom error dialog", customErrorDialogBtn.GetComponent<RectTransform>());
        }
        else if (screen == "purchase")
        {
            if (entitlementsLabel != null)
                SetOverlayElement("entitlements", entitlementsLabel.text, entitlementsLabel.rectTransform);

            if (paywallResultLabel != null)
                SetOverlayElement("paywallResult", paywallResultLabel.text, paywallResultLabel.rectTransform);

            var paywallBtn = purchaseScreen.transform.Find("PaywallButton");
            if (paywallBtn != null)
                SetOverlayElement("paywallBtn", "Present Paywall", paywallBtn.GetComponent<RectTransform>());

            var backBtn = purchaseScreen.transform.Find("BackButton");
            if (backBtn != null)
                SetOverlayElement("backBtn", "Back", backBtn.GetComponent<RectTransform>());
        }
        else if (screen == "checkpoint")
        {
            SetOverlayElement("checkpointIdentifier", checkpointIdentifierLabel.text, checkpointIdentifierLabel.rectTransform);

            var hitBtn = checkpointScreen.transform.Find("HitCheckpointButton");
            if (hitBtn != null)
                SetOverlayElement("hitCheckpointBtn", "Hit checkpoint", hitBtn.GetComponent<RectTransform>());

            SetOverlayElement("checkpointStatus", checkpointStatusLabel.text, checkpointStatusLabel.rectTransform);
            for (var i = 0; i < checkpointLineLabels.Length; i++)
            {
                if (checkpointLineLabels[i].text.Length > 0)
                    SetOverlayElement("checkpointLine" + i, checkpointLineLabels[i].text, checkpointLineLabels[i].rectTransform);
            }

            var backBtn = checkpointScreen.transform.Find("BackButton");
            if (backBtn != null)
                SetOverlayElement("backBtn", "Back", backBtn.GetComponent<RectTransform>());
        }
        else if (screen == "customCheckpoint")
        {
            // Expose exactly what is on screen: hidden buttons and empty rows must not match.
            foreach (Transform child in customCheckpointScreen.transform)
            {
                if (!child.gameObject.activeSelf) continue;
                var button = child.GetComponent<Button>();
                var text = button != null ? child.GetComponentInChildren<Text>() : child.GetComponent<Text>();
                if (text != null && text.text.Length > 0)
                    SetOverlayElement(child.name, text.text, child.GetComponent<RectTransform>());
            }
        }
    }

    private void SetOverlayElement(string id, string text, RectTransform rt)
    {
        if (rt == null || !overlayReady) return;

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        // corners: 0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right
        int left   = Mathf.RoundToInt(corners[0].x);
        int right  = Mathf.RoundToInt(corners[2].x);
        int top    = Screen.height - Mathf.RoundToInt(corners[1].y);
        int bottom = Screen.height - Mathf.RoundToInt(corners[0].y);

#if UNITY_ANDROID
        nativeOverlay.CallStatic("setElement", id, text, left, top, right, bottom);
#else
        NativeAccessibilityOverlaySetElement(id, text, left, top, right, bottom);
#endif
    }
#endif
}

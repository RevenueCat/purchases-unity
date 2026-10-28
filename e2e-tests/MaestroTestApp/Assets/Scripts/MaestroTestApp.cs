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
using RevenueCatUI;

[DefaultExecutionOrder(100)]
public class MaestroTestApp : Purchases.UpdatedCustomerInfoListener
{
    private const string API_KEY = "MAESTRO_TESTS_REVENUECAT_API_KEY";

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetLaunchTestFlow();
#endif

    private static readonly Dictionary<string, System.Action<MaestroTestApp>> TestFlowScreenMap =
        new Dictionary<string, System.Action<MaestroTestApp>>
        {
            { "purchase_through_paywall", app => app.ShowPurchaseScreen() },
            // Presents its paywall to every user, with the monthly package preselected.
            { "checkpoint_purchase", app => app.ShowCheckpointScreen("donottouch_sdk_everyone") },
            // A well-formed identifier that is not configured in the dashboard.
            { "checkpoint_unknown_identifier", app => app.ShowCheckpointScreen("maestro_unknown_checkpoint") }
        };

    public GameObject testCasesScreen;
    public GameObject purchaseScreen;
    public Text entitlementsLabel;
    public Text errorLabel;

    public GameObject checkpointScreen;
    public Text checkpointIdentifierLabel;
    public Text checkpointStatusLabel;
    public Text[] checkpointLineLabels;
    public Text checkpointErrorLabel;

    private const string CheckpointEntitlementId = "pro";
    private const string CheckpointProductId = "pro_monthly_subscription";

    private Purchases purchases;
    private string checkpointIdentifier;
    private int checkpointInvocations;
    private System.Action detachCheckpoint;

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

        var config = Purchases.PurchasesConfiguration.Builder.Init(API_KEY).Build();
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

    private string GetTestFlow()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return GetLaunchTestFlow();
#elif UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = activity.Call<AndroidJavaObject>("getIntent");
            return intent.Call<string>("getStringExtra", "e2e_test_flow");
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
        WireButton(checkpointScreen, "BackButton", ShowTestCases);
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
        testCasesScreen.SetActive(true);
        purchaseScreen.SetActive(false);
        checkpointScreen.SetActive(false);
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
        ClearError();
        UpdateEntitlements();
#if ACCESSIBILITY_OVERLAY
        StartCoroutine(UpdateOverlay("purchase"));
#endif
    }

    public void ShowCheckpointScreen(string identifier)
    {
        Debug.Log($"MaestroTestApp: ShowCheckpointScreen({identifier}) called");
        detachCheckpoint?.Invoke();
        detachCheckpoint = null;
        checkpointIdentifier = identifier;
        checkpointInvocations = 0;
        testCasesScreen.SetActive(false);
        purchaseScreen.SetActive(false);
        checkpointScreen.SetActive(true);
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
            detachCheckpoint = Checkpoints.Checkpoint(checkpointIdentifier, OnCheckpointPassed);
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

    private static string ListOrNone(IEnumerable<string> values)
    {
        var sorted = values.OrderBy(value => value, System.StringComparer.Ordinal).ToList();
        return sorted.Count == 0 ? "none" : string.Join(", ", sorted);
    }

    public async void PresentPaywall()
    {
        ClearError();

        // Present() traps its own exceptions and reports failure through the result, so
        // there is nothing here to catch; the result has to be inspected instead.
        var result = await PaywallsPresenter.Present();

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
        }
        else if (screen == "purchase")
        {
            if (entitlementsLabel != null)
                SetOverlayElement("entitlements", entitlementsLabel.text, entitlementsLabel.rectTransform);

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

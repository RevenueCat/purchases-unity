// Both players need a native accessibility overlay for Maestro to see the uGUI.
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
#define ACCESSIBILITY_OVERLAY
#endif

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using RevenueCatUI;

[DefaultExecutionOrder(100)]
public class MaestroTestApp : Purchases.UpdatedCustomerInfoListener
{
    private const string API_KEY = "MAESTRO_TESTS_REVENUECAT_API_KEY";

    public GameObject testCasesScreen;
    public GameObject purchaseScreen;
    public Text entitlementsLabel;
    public Text errorLabel;

    private Purchases purchases;

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

        ShowTestCases();
        Debug.Log("MaestroTestApp: ShowTestCases() done");
    }

    // The buttons are wired here rather than in the scene because SceneSetup builds
    // Main.unity from an editor script, and only persistent listeners survive
    // serialization. Anything added there with onClick.AddListener is dropped when the
    // scene is saved, which ships an app whose buttons do nothing.
    private void WireButtons()
    {
        WireButton(testCasesScreen, "PurchaseButton", ShowPurchaseScreen);
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
        testCasesScreen.SetActive(true);
        purchaseScreen.SetActive(false);
#if ACCESSIBILITY_OVERLAY
        StartCoroutine(UpdateOverlay("testCases"));
#endif
    }

    public void ShowPurchaseScreen()
    {
        Debug.Log("MaestroTestApp: ShowPurchaseScreen() called");
        testCasesScreen.SetActive(false);
        purchaseScreen.SetActive(true);
        ClearError();
        UpdateEntitlements();
#if ACCESSIBILITY_OVERLAY
        StartCoroutine(UpdateOverlay("purchase"));
#endif
    }

    public async void PresentPaywall()
    {
        ClearError();
        try
        {
            await PaywallsPresenter.Present();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to present paywall: {e}");
            ShowError(e.Message);
        }
    }

    public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        UpdateEntitlementsFromInfo(customerInfo);
    }

    private void UpdateEntitlements()
    {
        purchases.GetCustomerInfo((info, error) =>
        {
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

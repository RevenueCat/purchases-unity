using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using RevenueCatUI;

public class MaestroTestApp : Purchases.UpdatedCustomerInfoListener
{
    private const string API_KEY = "MAESTRO_TESTS_REVENUECAT_API_KEY";

    public GameObject testCasesScreen;
    public GameObject purchaseScreen;
    public Text entitlementsLabel;
    public Text errorLabel;

    private Purchases purchases;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaClass nativeOverlay;
#endif

    void Start()
    {
        purchases = GetComponent<Purchases>();
        purchases.useRuntimeSetup = true;

        var config = Purchases.PurchasesConfiguration.Builder.Init(API_KEY).Build();
        purchases.Configure(config);
        purchases.SetLogLevel(Purchases.LogLevel.Verbose);
        purchases.listener = this;

        if (errorLabel != null)
        {
            errorLabel.gameObject.SetActive(false);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            nativeOverlay = new AndroidJavaClass("com.revenuecat.accessibility.NativeAccessibilityOverlay");
            nativeOverlay.CallStatic("init");
        }
        catch (System.Exception e)
        {
            Debug.LogError("NativeAccessibilityOverlay init failed: " + e);
            nativeOverlay = null;
        }
#endif

        ShowTestCases();
    }

    public void ShowTestCases()
    {
        testCasesScreen.SetActive(true);
        purchaseScreen.SetActive(false);
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(UpdateOverlay("testCases"));
#endif
    }

    public void ShowPurchaseScreen()
    {
        testCasesScreen.SetActive(false);
        purchaseScreen.SetActive(true);
        ClearError();
        UpdateEntitlements();
#if UNITY_ANDROID && !UNITY_EDITOR
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
#if UNITY_ANDROID && !UNITY_EDITOR
            SetOverlayElement("entitlements", text, entitlementsLabel.rectTransform);
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

#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator UpdateOverlay(string screen)
    {
        yield return null; // wait one frame for layout

        if (nativeOverlay == null) yield break;
        nativeOverlay.CallStatic("clear");

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
        if (rt == null || nativeOverlay == null) return;

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        // corners: 0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right (Unity screen coords, Y-up)
        int left   = Mathf.RoundToInt(corners[0].x);
        int right  = Mathf.RoundToInt(corners[2].x);
        int top    = Screen.height - Mathf.RoundToInt(corners[1].y);
        int bottom = Screen.height - Mathf.RoundToInt(corners[0].y);

        nativeOverlay.CallStatic("setElement", id, text, left, top, right, bottom);
    }
#endif
}


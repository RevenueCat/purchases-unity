using UnityEngine;
using UnityEngine.UI;
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
    }

    public void ShowPurchaseScreen()
    {
        Debug.Log("MaestroTestApp: ShowPurchaseScreen() called");
        testCasesScreen.SetActive(false);
        purchaseScreen.SetActive(true);
        ClearError();
        UpdateEntitlements();
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
            entitlementsLabel.text = "Entitlements: " + (hasPro ? "pro" : "none");
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
}

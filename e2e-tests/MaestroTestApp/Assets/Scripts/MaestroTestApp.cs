using UnityEngine;
using UnityEngine.UI;
using RevenueCatUI;

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

        ShowTestCases();
    }

    public void ShowTestCases()
    {
        testCasesScreen.SetActive(true);
        purchaseScreen.SetActive(false);
    }

    public void ShowPurchaseScreen()
    {
        testCasesScreen.SetActive(false);
        purchaseScreen.SetActive(true);
        ClearError();
        UpdateEntitlements();
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

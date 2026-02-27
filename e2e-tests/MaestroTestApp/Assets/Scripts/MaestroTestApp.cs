using UnityEngine;
using UnityEngine.UI;
using RevenueCatUI;

public class MaestroTestApp : Purchases.UpdatedCustomerInfoListener
{
    private const string API_KEY = "MAESTRO_TESTS_REVENUECAT_API_KEY";

    public GameObject testCasesScreen;
    public GameObject purchaseScreen;
    public Text entitlementsLabel;

    private Purchases purchases;

    void Start()
    {
        purchases = GetComponent<Purchases>();
        purchases.useRuntimeSetup = true;

        var config = Purchases.PurchasesConfiguration.Builder.Init(API_KEY).Build();
        purchases.Configure(config);
        purchases.SetLogLevel(Purchases.LogLevel.Verbose);
        purchases.listener = this;

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
        UpdateEntitlements();
    }

    public async void PresentPaywall()
    {
        await PaywallsPresenter.Present();
    }

    public override void CustomerInfoReceived(CustomerInfo customerInfo)
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

    private void UpdateEntitlementsFromInfo(CustomerInfo info)
    {
        bool hasPro = info.Entitlements.Active.ContainsKey("pro");
        if (entitlementsLabel != null)
        {
            entitlementsLabel.text = "Entitlements: " + (hasPro ? "pro" : "none");
        }
    }
}

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using RevenueCat.UI;

public class RevenueCatUITestButtons : MonoBehaviour
{
    [SerializeField] private Text infoLabel;
    [SerializeField] private string requiredEntitlementIdentifier = "premium";
    [SerializeField] private string offeringIdentifier = "";

    public async void ShowPaywall()
    {
        if (!RevenueCatUI.IsSupported())
        {
            LogAndSet("RevenueCatUI not supported on this platform");
            return;
        }

        var options = new PaywallOptions { OfferingIdentifier = string.IsNullOrEmpty(offeringIdentifier) ? null : offeringIdentifier };
        LogAndSet("Presenting paywall...");
        var result = await RevenueCatUI.PresentPaywall(options);
        LogAndSet($"Paywall result: {result}");
    }

    public async void ShowPaywallIfNeeded()
    {
        if (!RevenueCatUI.IsSupported())
        {
            LogAndSet("RevenueCatUI not supported on this platform");
            return;
        }

        var options = new PaywallOptions { OfferingIdentifier = string.IsNullOrEmpty(offeringIdentifier) ? null : offeringIdentifier };
        LogAndSet($"Presenting paywall if needed for '{requiredEntitlementIdentifier}'...");
        var result = await RevenueCatUI.PresentPaywallIfNeeded(requiredEntitlementIdentifier, options);
        LogAndSet($"Conditional paywall result: {result}");
    }

    public async void ShowCustomerCenter()
    {
        if (!RevenueCatUI.IsSupported())
        {
            LogAndSet("RevenueCatUI not supported on this platform");
            return;
        }

        LogAndSet("Presenting customer center...");
        await RevenueCatUI.PresentCustomerCenter();
        LogAndSet("Customer center dismissed");
    }

    private void LogAndSet(string msg)
    {
        Debug.Log($"[RevenueCatUI][Test] {msg}");
        if (infoLabel != null) infoLabel.text = msg;
    }
}


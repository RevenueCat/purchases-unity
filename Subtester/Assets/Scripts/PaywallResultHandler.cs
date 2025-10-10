using UnityEngine;
using UnityEngine.UI;
using RevenueCatUI;

public class PaywallResultHandler : MonoBehaviour
{
    [SerializeField] private Text infoLabel;

    public void OnPaywallPurchased()
    {
        Debug.Log("User purchased!");
        if (infoLabel != null)
        {
            infoLabel.text = "PURCHASED - User completed a purchase";
        }
    }

    public void OnPaywallRestored()
    {
        Debug.Log("User restored purchases");
        if (infoLabel != null)
        {
            infoLabel.text = "RESTORED - User restored previous purchases";
        }
    }

    public void OnPaywallCancelled()
    {
        Debug.Log("User cancelled the paywall");
        if (infoLabel != null)
        {
            infoLabel.text = "CANCELLED - User dismissed the paywall";
        }
    }

    public void OnPaywallNotPresented()
    {
        Debug.Log("Paywall not needed - user already has access");
        if (infoLabel != null)
        {
            infoLabel.text = "NOT PRESENTED - User already has entitlement";
        }
    }

    public void OnPaywallError()
    {
        Debug.LogError("Error presenting paywall");
        if (infoLabel != null)
        {
            infoLabel.text = "ERROR - An error occurred during paywall";
        }
    }
}

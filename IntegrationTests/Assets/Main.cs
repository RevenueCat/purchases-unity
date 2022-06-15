using UnityEngine;


public class CustomListener : Purchases.UpdatedCustomerInfoListener
{
    public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        throw new System.NotImplementedException();
    }
}

public class Main : MonoBehaviour
{
    
    // Use this for initialization
    private void Start()
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.SetDebugLogsEnabled(true);
        purchases.deprecatedLegacyRevenueCatAPIKey = "abc";
        purchases.revenueCatAPIKeyApple = "def";
        purchases.revenueCatAPIKeyGoogle = "ghi";
        purchases.appUserID = "abc";
        purchases.productIdentifiers = new[] { "a", "b", "c" };
        purchases.listener = new CustomListener();
        purchases.observerMode = true;
        purchases.userDefaultsSuiteName = "suitename";
        purchases.proxyURL = "https://proxy-url.revenuecat.com";
        
        
        Purchases.CustomerInfo receivedCustomerInfo;
        Purchases.Error receivedError;
        purchases.GetCustomerInfo((info, error) =>
        {
            receivedCustomerInfo = info;
            receivedError = error;
        });
        
        
    }

}

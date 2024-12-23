using UnityEngine;
using RevenueCat;

public class DeepLinkListener : MonoBehaviour
{
    public static DeepLinkListener Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;                
            Application.deepLinkActivated += onDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
 
    private void onDeepLinkActivated(string url)
    {
        GameObject purchasesManager = GameObject.Find("PurchasesManager");
        if (purchasesManager == null)
        {
            Debug.LogError("PurchasesManager not found");
            return;
        }
        Purchases purchases = purchasesManager.GetComponent<Purchases>();
        purchases.ParseAsWebPurchaseRedemption(url, (webPurchaseRedemption) =>
        {
            if (webPurchaseRedemption != null)
            {
                Debug.Log("Starting redemption: " + webPurchaseRedemption.ToString());
                purchases.RedeemWebPurchase(webPurchaseRedemption, (result) =>
                {
                    Debug.Log("Redemption result: " + result.ToString());
                });
            }
        });
    }
}

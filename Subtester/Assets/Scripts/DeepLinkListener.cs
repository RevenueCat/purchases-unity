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
                    switch (result) 
                    {
                        case Purchases.WebPurchaseRedemptionResult.Success success:
                            Debug.Log("Redemption successful: " + success.CustomerInfo.ToString());
                            break;
                        case Purchases.WebPurchaseRedemptionResult.RedemptionError error:
                            Debug.Log("Redemption failed: " + error.Error.ToString());
                            break;
                        case Purchases.WebPurchaseRedemptionResult.InvalidToken:
                            Debug.Log("Redemption failed: Invalid token");
                            break;
                        case Purchases.WebPurchaseRedemptionResult.PurchaseBelongsToOtherUser:
                            Debug.Log("Redemption failed: Purchase belongs to other user");
                            break;
                        case Purchases.WebPurchaseRedemptionResult.Expired expired:
                            Debug.Log("Redemption failed: Expired. Sent new email to " + expired.ObfuscatedEmail);
                            break;
                    }
                });
            }
        });
    }
}

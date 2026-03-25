using System;
using UnityEngine;

public class DeepLinkListener : MonoBehaviour
{
    public static DeepLinkListener Instance { get; private set; }
    public static event Action<string> OnDeepLinkReceived;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += HandleDeepLink;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                HandleDeepLink(Application.absoluteURL);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void HandleDeepLink(string url)
    {
        Debug.Log($"[DeepLink] Received: {url}");
        OnDeepLinkReceived?.Invoke(url);

        var purchases = FindFirstObjectByType<Purchases>();
        if (purchases == null)
        {
            Debug.LogError("[DeepLink] No Purchases component found");
            return;
        }

        purchases.ParseAsWebPurchaseRedemption(url, (webPurchaseRedemption) =>
        {
            if (webPurchaseRedemption == null) return;

            Debug.Log($"[DeepLink] Starting redemption: {webPurchaseRedemption}");
            purchases.RedeemWebPurchase(webPurchaseRedemption, (result) =>
            {
                switch (result)
                {
                    case Purchases.WebPurchaseRedemptionResult.Success success:
                        Debug.Log($"[DeepLink] Redemption successful: {success.CustomerInfo}");
                        break;
                    case Purchases.WebPurchaseRedemptionResult.RedemptionError error:
                        Debug.LogError($"[DeepLink] Redemption failed: {error.Error}");
                        break;
                    case Purchases.WebPurchaseRedemptionResult.InvalidToken:
                        Debug.LogError("[DeepLink] Redemption failed: Invalid token");
                        break;
                    case Purchases.WebPurchaseRedemptionResult.PurchaseBelongsToOtherUser:
                        Debug.LogError("[DeepLink] Redemption failed: Purchase belongs to other user");
                        break;
                    case Purchases.WebPurchaseRedemptionResult.Expired expired:
                        Debug.Log($"[DeepLink] Redemption expired. New email sent to {expired.ObfuscatedEmail}");
                        break;
                }
            });
        });
    }
}

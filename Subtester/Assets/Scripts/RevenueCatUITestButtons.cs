using UnityEngine;
using RevenueCat.UI;
using System.Threading.Tasks;

/// <summary>
/// Additional test script for RevenueCat UI buttons in Subtester.
/// Attach this to a GameObject for extra testing functionality beyond the main PurchasesListener.
/// </summary>
public class RevenueCatUITestButtons : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDetailedLogging = true;
    
    void Start()
    {
        // Check platform support on startup
        if (RevenueCatUI.IsSupported())
        {
            Debug.Log("✅ RevenueCat UI is supported on this platform");
        }
        else
        {
            Debug.LogWarning("⚠️ RevenueCat UI is not supported on this platform (Editor/Desktop)");
            Debug.Log("💡 Build to iOS or Android to test the actual functionality");
        }
    }

    /// <summary>
    /// Example of conditional paywall - only shows if user doesn't have "pro" entitlement
    /// </summary>
    public async void OnShowPaywallIfNeededButtonClicked()
    {
        LogDetailed("🔒 Show Paywall If Needed button clicked");
        
        if (!RevenueCatUI.IsSupported())
        {
            Debug.LogWarning("❌ Cannot show conditional paywall - platform not supported");
            return;
        }

        try
        {
            string requiredEntitlement = "pro"; // Change this to your entitlement ID
            var options = new PaywallOptions("premium", displayCloseButton: true);
            
            LogDetailed($"📱 Checking for entitlement '{requiredEntitlement}'...");
            var result = await RevenueCatUI.PresentPaywallIfNeeded(requiredEntitlement, options);
            
            if (result.Result == PaywallResultType.NotPresented)
            {
                LogDetailed($"✅ User already has '{requiredEntitlement}' entitlement - paywall not shown");
            }
            else
            {
                HandlePaywallResult(result);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error presenting conditional paywall: {e.Message}");
        }
    }

    /// <summary>
    /// Present paywall with specific offering and close button
    /// </summary>
    public async void OnShowPaywallWithOptionsButtonClicked()
    {
        LogDetailed("🎨 Show Paywall with Options button clicked");
        
        if (!RevenueCatUI.IsSupported())
        {
            Debug.LogWarning("❌ Cannot show paywall - platform not supported");
            return;
        }

        try
        {
            // Example with specific offering and close button
            var options = new PaywallOptions
            {
                OfferingIdentifier = "premium", // Change this to your offering ID
                DisplayCloseButton = true
            };

            LogDetailed($"📱 Presenting paywall with offering: {options.OfferingIdentifier}");
            var result = await RevenueCatUI.PresentPaywall(options);
            HandlePaywallResult(result);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error presenting paywall with options: {e.Message}");
        }
    }

    /// <summary>
    /// Handle the result of paywall presentations
    /// </summary>
    private void HandlePaywallResult(PaywallResult result)
    {
        switch (result.Result)
        {
            case PaywallResultType.Purchased:
                Debug.Log("🎉 SUCCESS: User made a purchase!");
                OnPurchaseCompleted();
                break;
                
            case PaywallResultType.Restored:
                Debug.Log("🔄 SUCCESS: User restored purchases!");
                OnPurchaseCompleted();
                break;
                
            case PaywallResultType.Cancelled:
                LogDetailed("❌ User cancelled the paywall");
                break;
                
            case PaywallResultType.Error:
                Debug.LogError("❌ Error occurred during paywall presentation");
                break;
                
            case PaywallResultType.NotPresented:
                LogDetailed("ℹ️ Paywall was not presented (user already has entitlement)");
                break;
                
            default:
                LogDetailed($"ℹ️ Paywall result: {result.Result}");
                break;
        }
    }

    /// <summary>
    /// Called when a purchase or restore is completed
    /// </summary>
    private void OnPurchaseCompleted()
    {
        Debug.Log("🚀 Purchase completed - you can now unlock premium features!");
        // Customer info will be automatically updated by the main SDK
        // You can check entitlements in your main PurchasesListener
    }

    /// <summary>
    /// Log detailed messages only if enabled
    /// </summary>
    private void LogDetailed(string message)
    {
        if (enableDetailedLogging)
        {
            Debug.Log($"[RevenueCatUITest] {message}");
        }
    }

    /// <summary>
    /// Public method to check platform support (useful for enabling/disabling buttons)
    /// </summary>
    public bool IsPlatformSupported()
    {
        return RevenueCatUI.IsSupported();
    }
} 
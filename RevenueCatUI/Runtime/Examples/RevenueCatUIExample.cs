using UnityEngine;
using RevenueCat;
using System.Threading.Tasks;

namespace RevenueCat.UI.Examples
{
    /// <summary>
    /// Example script demonstrating how to use RevenueCat UI components.
    /// Attach this to a GameObject and call the public methods from UI buttons.
    /// </summary>
    public class RevenueCatUIExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string offeringIdentifier = "premium";
        [SerializeField] private string requiredEntitlementIdentifier = "pro_access";

        /// <summary>
        /// Example: Present a paywall with default settings
        /// </summary>
        public async void ShowPaywall()
        {
            Debug.Log("[RevenueCatUIExample] Presenting paywall...");
            
            if (!RevenueCatUI.IsSupported())
            {
                Debug.LogWarning("[RevenueCatUIExample] RevenueCat UI is not supported on this platform");
                return;
            }

            try
            {
                var result = await RevenueCatUI.PresentPaywall();
                HandlePaywallResult(result);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RevenueCatUIExample] Error presenting paywall: {e.Message}");
            }
        }

        /// <summary>
        /// Example: Present a paywall with specific offering and close button
        /// </summary>
        public async void ShowPaywallWithOptions()
        {
            Debug.Log($"[RevenueCatUIExample] Presenting paywall with offering: {offeringIdentifier}");
            
            if (!RevenueCatUI.IsSupported())
            {
                Debug.LogWarning("[RevenueCatUIExample] RevenueCat UI is not supported on this platform");
                return;
            }

            try
            {
                var options = new PaywallOptions(offeringIdentifier, displayCloseButton: true);
                var result = await RevenueCatUI.PresentPaywall(options);
                HandlePaywallResult(result);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RevenueCatUIExample] Error presenting paywall with options: {e.Message}");
            }
        }

        /// <summary>
        /// Example: Present paywall only if user doesn't have specific entitlement
        /// </summary>
        public async void ShowPaywallIfNeeded()
        {
            Debug.Log($"[RevenueCatUIExample] Checking for entitlement '{requiredEntitlementIdentifier}'...");
            
            if (!RevenueCatUI.IsSupported())
            {
                Debug.LogWarning("[RevenueCatUIExample] RevenueCat UI is not supported on this platform");
                return;
            }

            try
            {
                var options = new PaywallOptions(offeringIdentifier, displayCloseButton: true);
                var result = await RevenueCatUI.PresentPaywallIfNeeded(requiredEntitlementIdentifier, options);
                
                if (result.Result == PaywallResultType.NotPresented)
                {
                    Debug.Log($"[RevenueCatUIExample] User already has '{requiredEntitlementIdentifier}' entitlement!");
                }
                else
                {
                    HandlePaywallResult(result);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RevenueCatUIExample] Error presenting conditional paywall: {e.Message}");
            }
        }

        /// <summary>
        /// Example: Present customer center
        /// </summary>
        public async void ShowCustomerCenter()
        {
            Debug.Log("[RevenueCatUIExample] Presenting customer center...");
            
            if (!RevenueCatUI.IsSupported())
            {
                Debug.LogWarning("[RevenueCatUIExample] RevenueCat UI is not supported on this platform");
                return;
            }

            try
            {
                await RevenueCatUI.PresentCustomerCenter();
                Debug.Log("[RevenueCatUIExample] Customer center was dismissed");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RevenueCatUIExample] Error presenting customer center: {e.Message}");
            }
        }

        /// <summary>
        /// Handle the result of a paywall presentation
        /// </summary>
        private void HandlePaywallResult(PaywallResult result)
        {
            switch (result.Result)
            {
                case PaywallResultType.Purchased:
                    Debug.Log("[RevenueCatUIExample] ✅ User made a purchase!");
                    OnPurchaseCompleted();
                    break;
                    
                case PaywallResultType.Restored:
                    Debug.Log("[RevenueCatUIExample] ✅ User restored purchases!");
                    OnPurchaseCompleted();
                    break;
                    
                case PaywallResultType.Cancelled:
                    Debug.Log("[RevenueCatUIExample] ❌ User cancelled the paywall");
                    break;
                    
                case PaywallResultType.Error:
                    Debug.LogError("[RevenueCatUIExample] ❌ Error during paywall presentation");
                    break;
                    
                default:
                    Debug.Log($"[RevenueCatUIExample] Paywall result: {result.Result}");
                    break;
            }
        }

        /// <summary>
        /// Called when a purchase or restore is completed
        /// </summary>
        private void OnPurchaseCompleted()
        {
            Debug.Log("[RevenueCatUIExample] Purchase completed - unlocking premium features!");
            
            // Here you would typically:
            // 1. Refresh customer info
            // 2. Update UI to reflect new entitlements
            // 3. Unlock premium features
            // 4. Show success message to user
        }

        /// <summary>
        /// Check platform support on start
        /// </summary>
        void Start()
        {
            if (RevenueCatUI.IsSupported())
            {
                Debug.Log("[RevenueCatUIExample] ✅ RevenueCat UI is supported on this platform");
            }
            else
            {
                Debug.LogWarning("[RevenueCatUIExample] ⚠️ RevenueCat UI is not supported on this platform");
            }
        }
    }
} 
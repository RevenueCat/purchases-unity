using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UI;

namespace RevenueCat.UI.Examples
{
    /// <summary>
    /// Complete example demonstrating RevenueCat UI functionality.
    /// This script shows how to use paywalls and customer center after the iOS implementation is complete.
    /// </summary>
    public class RevenueCatUICompleteExample : MonoBehaviour
    {
        [Header("Paywall Configuration")]
        public string offeringIdentifier = ""; // Leave empty for default offering
        public bool displayCloseButton = true;
        
        [Header("Conditional Paywall")]
        public string requiredEntitlementIdentifier = "premium"; // Example entitlement ID
        
        [Header("UI")]
        public UnityEngine.UI.Button paywallButton;
        public UnityEngine.UI.Button conditionalPaywallButton;
        public UnityEngine.UI.Button customerCenterButton;
        public UnityEngine.UI.Text statusText;

        private void Start()
        {
            // Setup button listeners
            if (paywallButton != null)
                paywallButton.onClick.AddListener(OnPresentPaywallClicked);
                
            if (conditionalPaywallButton != null)
                conditionalPaywallButton.onClick.AddListener(OnPresentConditionalPaywallClicked);
                
            if (customerCenterButton != null)
                customerCenterButton.onClick.AddListener(OnPresentCustomerCenterClicked);

            // Check platform support
            bool isSupported = RevenueCatUI.IsSupported();
            UpdateStatus($"RevenueCat UI Supported: {isSupported}");
            
            // Enable/disable buttons based on support
            if (paywallButton != null) paywallButton.interactable = isSupported;
            if (conditionalPaywallButton != null) conditionalPaywallButton.interactable = isSupported;
            if (customerCenterButton != null) customerCenterButton.interactable = isSupported;

            if (!isSupported)
            {
                Debug.LogWarning("[RevenueCatUI] Platform not supported. UI components will be disabled.");
            }
        }

        /// <summary>
        /// Present a paywall with the configured options.
        /// </summary>
        public async void OnPresentPaywallClicked()
        {
            UpdateStatus("Presenting paywall...");

            var options = new PaywallOptions
            {
                OfferingIdentifier = string.IsNullOrEmpty(offeringIdentifier) ? null : offeringIdentifier,
                DisplayCloseButton = displayCloseButton
            };

            try
            {
                var result = await RevenueCatUI.PresentPaywall(options);
                HandlePaywallResult(result);
            }
            catch (Exception e)
            {
                UpdateStatus($"Error presenting paywall: {e.Message}");
                Debug.LogError($"[RevenueCatUI] Paywall presentation error: {e}");
            }
        }

        /// <summary>
        /// Present a paywall only if the user doesn't have the required entitlement.
        /// </summary>
        public async void OnPresentConditionalPaywallClicked()
        {
            if (string.IsNullOrEmpty(requiredEntitlementIdentifier))
            {
                UpdateStatus("Error: Please set a required entitlement identifier");
                return;
            }

            UpdateStatus($"Checking entitlement '{requiredEntitlementIdentifier}' and presenting paywall if needed...");

            var options = new PaywallOptions
            {
                OfferingIdentifier = string.IsNullOrEmpty(offeringIdentifier) ? null : offeringIdentifier,
                DisplayCloseButton = displayCloseButton
            };

            try
            {
                var result = await RevenueCatUI.PresentPaywallIfNeeded(requiredEntitlementIdentifier, options);
                HandlePaywallResult(result);
            }
            catch (Exception e)
            {
                UpdateStatus($"Error presenting conditional paywall: {e.Message}");
                Debug.LogError($"[RevenueCatUI] Conditional paywall presentation error: {e}");
            }
        }

        /// <summary>
        /// Present the customer center for subscription management.
        /// </summary>
        public async void OnPresentCustomerCenterClicked()
        {
            UpdateStatus("Presenting customer center...");

            try
            {
                await RevenueCatUI.PresentCustomerCenter();
                UpdateStatus("Customer center dismissed");
            }
            catch (Exception e)
            {
                UpdateStatus($"Error presenting customer center: {e.Message}");
                Debug.LogError($"[RevenueCatUI] Customer center presentation error: {e}");
            }
        }

        /// <summary>
        /// Handle the result from paywall presentation.
        /// </summary>
        private void HandlePaywallResult(PaywallResult result)
        {
            string message = result.Result switch
            {
                PaywallResultType.NotPresented => "Paywall not presented (user may already have entitlement)",
                PaywallResultType.Cancelled => "User cancelled the paywall",
                PaywallResultType.Error => "An error occurred during paywall presentation",
                PaywallResultType.Purchased => "User completed a purchase! 🎉",
                PaywallResultType.Restored => "User restored their purchases! ✨",
                _ => $"Unknown result: {result.Result}"
            };

            UpdateStatus($"Paywall result: {message}");
            Debug.Log($"[RevenueCatUI] Paywall result: {result}");

            // Handle successful transactions
            if (result.Result == PaywallResultType.Purchased || result.Result == PaywallResultType.Restored)
            {
                OnSuccessfulTransaction(result.Result);
            }
        }

        /// <summary>
        /// Called when a transaction is successful (purchase or restore).
        /// </summary>
        private void OnSuccessfulTransaction(PaywallResultType transactionType)
        {
            Debug.Log($"[RevenueCatUI] Successful transaction: {transactionType}");
            
            // Here you would typically:
            // 1. Update your UI to reflect the new entitlements
            // 2. Unlock premium features
            // 3. Show a success message
            // 4. Refresh customer info if needed
            
            // Example:
            // await RefreshCustomerInfo();
            // EnablePremiumFeatures();
            // ShowSuccessMessage();
        }

        /// <summary>
        /// Update the status text (if available).
        /// </summary>
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            
            Debug.Log($"[RevenueCatUI] Status: {message}");
        }

        /// <summary>
        /// Test method to validate the complete implementation.
        /// Call this from a button or in your test suite.
        /// </summary>
        [ContextMenu("Test RevenueCat UI Implementation")]
        public void TestImplementation()
        {
            Debug.Log("=== RevenueCat UI Implementation Test ===");
            
            // Test platform support
            bool isSupported = RevenueCatUI.IsSupported();
            Debug.Log($"Platform supported: {isSupported}");
            
            if (!isSupported)
            {
                Debug.LogWarning("Platform not supported. Cannot test further.");
                return;
            }

            Debug.Log("✅ RevenueCat UI implementation appears complete!");
            Debug.Log("You can now:");
            Debug.Log("- Present paywalls with RevenueCatUI.PresentPaywall()");
            Debug.Log("- Present conditional paywalls with RevenueCatUI.PresentPaywallIfNeeded()");
            Debug.Log("- Present customer center with RevenueCatUI.PresentCustomerCenter()");
            Debug.Log("- Handle results with proper async/await patterns");
        }
    }
} 
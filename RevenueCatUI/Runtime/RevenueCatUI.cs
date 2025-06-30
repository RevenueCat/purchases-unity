using System;
using System.Threading.Tasks;
using UnityEngine;

namespace RevenueCat.UI
{
    /// <summary>
    /// Main interface for RevenueCat UI components.
    /// Provides methods to present paywalls and customer center.
    /// </summary>
    public static class RevenueCatUI
    {
        /// <summary>
        /// Presents a paywall configured in the RevenueCat dashboard.
        /// </summary>
        /// <param name="options">Options for presenting the paywall</param>
        /// <returns>A PaywallResult indicating what happened during the paywall presentation</returns>
        public static async Task<PaywallResult> PresentPaywall(PaywallOptions options = null)
        {
            try 
            {
                Debug.Log("[RevenueCatUI] Presenting paywall...");
                
                // Use the platform-specific implementation
                var presenter = PaywallPresenter.Instance;
                return await presenter.PresentPaywallAsync(options ?? new PaywallOptions());
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting paywall: {e.Message}");
                return PaywallResult.Error;
            }
        }

        /// <summary>
        /// Presents a paywall only if the user does not have the specified entitlement.
        /// </summary>
        /// <param name="requiredEntitlementIdentifier">Entitlement identifier to check before presenting</param>
        /// <param name="options">Options for presenting the paywall</param>
        /// <returns>A PaywallResult indicating what happened during the paywall presentation</returns>
        public static async Task<PaywallResult> PresentPaywallIfNeeded(
            string requiredEntitlementIdentifier, 
            PaywallOptions options = null)
        {
            if (string.IsNullOrEmpty(requiredEntitlementIdentifier))
            {
                Debug.LogError("[RevenueCatUI] Required entitlement identifier cannot be null or empty");
                return PaywallResult.Error;
            }

            try
            {
                Debug.Log($"[RevenueCatUI] Presenting paywall if needed for entitlement: {requiredEntitlementIdentifier}");
                
                var presenter = PaywallPresenter.Instance;
                return await presenter.PresentPaywallIfNeededAsync(requiredEntitlementIdentifier, options ?? new PaywallOptions());
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting paywall if needed: {e.Message}");
                return PaywallResult.Error;
            }
        }

        /// <summary>
        /// Presents the customer center where users can manage their subscriptions.
        /// </summary>
        /// <returns>A task that completes when the customer center is dismissed</returns>
        public static async Task PresentCustomerCenter()
        {
            try
            {
                Debug.Log("[RevenueCatUI] Presenting customer center...");
                
                var presenter = CustomerCenterPresenter.Instance;
                await presenter.PresentCustomerCenterAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting customer center: {e.Message}");
            }
        }

        /// <summary>
        /// Checks if the RevenueCat UI components are available on the current platform.
        /// </summary>
        /// <returns>True if UI components are supported on this platform</returns>
        public static bool IsSupported()
        {
            try
            {
                var paywallPresenter = PaywallPresenter.Instance;
                var customerCenterPresenter = CustomerCenterPresenter.Instance;
                return paywallPresenter.IsSupported() && customerCenterPresenter.IsSupported();
            }
            catch
            {
                return false;
            }
        }
    }
} 
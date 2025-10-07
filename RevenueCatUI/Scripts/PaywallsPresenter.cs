using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCatUI.Internal;

namespace RevenueCatUI
{
    /// <summary>
    /// Main interface for RevenueCatUI paywall presentation.
    /// Provides static methods to present paywalls.
    /// </summary>
    public static class PaywallsPresenter
    {
        
        /// <summary>
        /// Presents a paywall configured in the RevenueCat dashboard.
        /// </summary>
        /// <param name="options">Options for presenting the paywall</param>
        /// <returns>A PaywallResult indicating what happened during the paywall presentation</returns>
        public static async Task<PaywallResult> Present(PaywallOptions options = null)
        {
            try 
            {
                Debug.Log("[RevenueCatUI] Presenting paywall...");
                
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
        public static async Task<PaywallResult> PresentIfNeeded(
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
    }
} 

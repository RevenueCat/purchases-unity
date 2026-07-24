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
        /// Must be called from the Unity main thread.
        /// </summary>
        /// <param name="options">Options for presenting the paywall</param>
        /// <returns>A PaywallResult indicating what happened during the paywall presentation</returns>
        public static async Task<PaywallResult> Present(PaywallOptions options = null)
        {
            try
            {
                WarnIfListenerCombinedWithPurchaseLogic(options);
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
        /// Must be called from the Unity main thread.
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

                WarnIfListenerCombinedWithPurchaseLogic(options);
                var presenter = PaywallPresenter.Instance;
                return await presenter.PresentPaywallIfNeededAsync(requiredEntitlementIdentifier, options ?? new PaywallOptions());
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting paywall if needed: {e.Message}");
                return PaywallResult.Error;
            }
        }

        private static void WarnIfListenerCombinedWithPurchaseLogic(PaywallOptions options)
        {
            if (options?.Listener != null && options.PurchaseLogic != null)
            {
                Debug.LogWarning("[RevenueCatUI] PaywallListener is combined with PurchaseLogic (MY_APP mode). " +
                                 "In this mode the native paywall does not emit purchase/restore started or " +
                                 "completed events; only error and cancellation events fire. Observe purchase " +
                                 "outcomes through the PurchaseLogic handlers instead.");
            }
        }
    }
} 

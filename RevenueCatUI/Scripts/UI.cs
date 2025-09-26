using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UnityUI;

namespace RevenueCat
{
    /// <summary>
    /// Static entry point for presenting RevenueCat paywalls without requiring a MonoBehaviour component.
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// Presents a paywall configured in the RevenueCat dashboard using the default presenter.
        /// </summary>
        /// <param name="options">Optional presentation options.</param>
        /// <returns><see cref="PaywallResult"/> describing the outcome.</returns>
        public static Task<PaywallResult> PresentPaywallAsync(PaywallOptions options = null)
        {
            return PresentPaywallAsyncInternal(null, options);
        }

        /// <summary>
        /// Presents a paywall only if the user lacks the specified entitlement.
        /// </summary>
        /// <param name="requiredEntitlementIdentifier">Entitlement identifier to check before presenting.</param>
        /// <param name="options">Optional presentation options.</param>
        /// <returns><see cref="PaywallResult"/> describing the outcome.</returns>
        public static Task<PaywallResult> PresentPaywallIfNeededAsync(
            string requiredEntitlementIdentifier,
            PaywallOptions options = null)
        {
            if (string.IsNullOrEmpty(requiredEntitlementIdentifier))
            {
                Debug.LogError("[RevenueCatUI] Required entitlement identifier cannot be null or empty");
                return Task.FromResult(PaywallResult.Error);
            }

            return PresentPaywallIfNeededAsyncInternal(null, requiredEntitlementIdentifier, options);
        }

        /// <summary>
        /// Checks whether paywalls are supported on the current platform.
        /// </summary>
        /// <returns>True if supported, otherwise false.</returns>
        public static bool IsSupported()
        {
            try
            {
                return PaywallPresenter.Instance.IsSupported();
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error checking paywall support: {e.Message}");
                return false;
            }
        }

        internal static Task<PaywallResult> PresentPaywallAsync(string gameObjectName, PaywallOptions options = null)
        {
            return PresentPaywallAsyncInternal(gameObjectName, options);
        }

        internal static Task<PaywallResult> PresentPaywallIfNeededAsync(
            string gameObjectName,
            string requiredEntitlementIdentifier,
            PaywallOptions options = null)
        {
            if (string.IsNullOrEmpty(requiredEntitlementIdentifier))
            {
                Debug.LogError("[RevenueCatUI] Required entitlement identifier cannot be null or empty");
                return Task.FromResult(PaywallResult.Error);
            }

            return PresentPaywallIfNeededAsyncInternal(gameObjectName, requiredEntitlementIdentifier, options);
        }

        private static Task<PaywallResult> PresentPaywallAsyncInternal(string gameObjectName, PaywallOptions options)
        {
            try
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log("[RevenueCatUI] Presenting paywall...");
                }

                return PaywallPresenter.Instance.PresentPaywallAsync(gameObjectName, options ?? new PaywallOptions());
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting paywall: {e.Message}");
                return Task.FromResult(PaywallResult.Error);
            }
        }

        private static Task<PaywallResult> PresentPaywallIfNeededAsyncInternal(
            string gameObjectName,
            string requiredEntitlementIdentifier,
            PaywallOptions options)
        {
            try
            {
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"[RevenueCatUI] Presenting paywall if needed for entitlement: {requiredEntitlementIdentifier}");
                }

                return PaywallPresenter.Instance.PresentPaywallIfNeededAsync(
                    gameObjectName,
                    requiredEntitlementIdentifier,
                    options ?? new PaywallOptions());
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting paywall if needed: {e.Message}");
                return Task.FromResult(PaywallResult.Error);
            }
        }
    }
}

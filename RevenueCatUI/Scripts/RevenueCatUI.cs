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
        private static bool? _isSupportedCache;
        // Whether the UnitySendMessage fallback receiver is enabled. Disabled by default.
        private static bool _unityMessageFallbackEnabled;
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
                // Pure-code path by default. If the optional UnitySendMessage fallback is enabled,
                // ensure the receiver is registered before presenting (no effect if already added).
                if (_unityMessageFallbackEnabled)
                {
                    PaywallCallbackReceiver.EnsureExists();
                }
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
                if (_unityMessageFallbackEnabled)
                {
                    PaywallCallbackReceiver.EnsureExists();
                }
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
        /// Checks if RevenueCat UI is available on the current platform.
        /// Returns true on iOS/Android device builds when both paywall and
        /// customer center are supported; returns false on other platforms
        /// (Editor, Windows, macOS, WebGL, etc.).
        /// </summary>
        /// <returns>True if UI is supported on this platform, otherwise false.</returns>
        public static bool IsSupported()
        {
            if (_isSupportedCache.HasValue)
            {
                return _isSupportedCache.Value;
            }
            try
            {
                var paywallPresenter = PaywallPresenter.Instance;
                var customerCenterPresenter = CustomerCenterPresenter.Instance;
                var paywall = paywallPresenter.IsSupported();
                var customerCenter = customerCenterPresenter.IsSupported();
                var supported = paywall && customerCenter;
                _isSupportedCache = supported;
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"[RevenueCatUI] IsSupported -> Paywall={paywall}, CustomerCenter={customerCenter}, Result={supported}");
                }
                return supported;
            }
            catch
            {
                Debug.Log("[RevenueCatUI] IsSupported check threw; returning false");
                _isSupportedCache = false;
                return false;
            }
        }

        /// <summary>
        /// Whether the Paywall feature is supported on the current platform.
        /// </summary>
        public static bool IsPaywallSupported()
        {
            try { return PaywallPresenter.Instance.IsSupported(); }
            catch { return false; }
        }

        /// <summary>
        /// Whether the Customer Center feature is supported on the current platform.
        /// </summary>
        public static bool IsCustomerCenterSupported()
        {
            try { return CustomerCenterPresenter.Instance.IsSupported(); }
            catch { return false; }
        }

        /// <summary>
        /// Enables an optional UnitySendMessage fallback for receiving native callbacks.
        /// Disabled by default. When enabled, a persistent receiver GameObject is created.
        /// Pure-code (delegate) callbacks remain the primary mechanism.
        /// </summary>
        /// <param name="receiverName">Optional receiver GameObject name</param>
        public static void EnableUnityMessageFallback(string receiverName = null)
        {
            _unityMessageFallbackEnabled = true;
            PaywallCallbackReceiver.EnsureExists(receiverName);
            Debug.Log("[RevenueCatUI] UnitySendMessage fallback enabled");
        }

        /// <summary>
        /// Disables the UnitySendMessage fallback and removes the receiver GameObject if present.
        /// </summary>
        public static void DisableUnityMessageFallback()
        {
            _unityMessageFallbackEnabled = false;
            PaywallCallbackReceiver.Disable();
            Debug.Log("[RevenueCatUI] UnitySendMessage fallback disabled");
        }
    }
} 

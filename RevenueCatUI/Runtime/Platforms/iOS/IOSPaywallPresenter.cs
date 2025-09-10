#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UI;

namespace RevenueCat.UI.Platforms
{
    /// <summary>
    /// iOS implementation of the paywall presenter.
    /// Uses purchases-hybrid-common PaywallProxy.
    /// </summary>
    internal class IOSPaywallPresenter : IPaywallPresenter
    {
        #region DLL Imports
        
        [DllImport("__Internal")]
        private static extern void initializeRevenueCatUI();
        
        [DllImport("__Internal")]
        private static extern void presentPaywall(string offeringIdentifier, bool displayCloseButton, PaywallResultCallback callback);
        
        [DllImport("__Internal")]
        private static extern void presentPaywallIfNeeded(string requiredEntitlementIdentifier, string offeringIdentifier, bool displayCloseButton, PaywallResultCallback callback);
        
        [DllImport("__Internal")]
        private static extern bool isRevenueCatUISupported();
        
        private delegate void PaywallResultCallback(string result);
        
        #endregion

        private TaskCompletionSource<PaywallResult> currentPaywallTcs;
        private static TaskCompletionSource<PaywallResult> staticCurrentPaywallTcs;

        public IOSPaywallPresenter()
        {
            // Initialize the native iOS bridge
            initializeRevenueCatUI();
        }

        public bool IsSupported()
        {
            return isRevenueCatUISupported();
        }

        public async Task<PaywallResult> PresentPaywallAsync(PaywallOptions options)
        {
            if (currentPaywallTcs != null && !currentPaywallTcs.Task.IsCompleted)
            {
                Debug.LogWarning("[RevenueCatUI] Another paywall is already being presented. Cancelling previous request.");
                currentPaywallTcs.TrySetCanceled();
            }

            currentPaywallTcs = new TaskCompletionSource<PaywallResult>();
            staticCurrentPaywallTcs = currentPaywallTcs; // Store for static callback

            try
            {
                Debug.Log($"[RevenueCatUI] Presenting paywall with offering: {options?.OfferingIdentifier ?? "default"}, displayCloseButton: {options?.DisplayCloseButton ?? false}");
                
                presentPaywall(
                    options?.OfferingIdentifier,
                    options?.DisplayCloseButton ?? false,
                    OnPaywallResult
                );

                return await currentPaywallTcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting paywall: {ex.Message}");
                currentPaywallTcs?.TrySetException(ex);
                return PaywallResult.Error;
            }
        }

        public async Task<PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallOptions options)
        {
            if (string.IsNullOrEmpty(requiredEntitlementIdentifier))
            {
                Debug.LogError("[RevenueCatUI] requiredEntitlementIdentifier cannot be null or empty for PresentPaywallIfNeededAsync");
                return PaywallResult.Error;
            }

            if (currentPaywallTcs != null && !currentPaywallTcs.Task.IsCompleted)
            {
                Debug.LogWarning("[RevenueCatUI] Another paywall is already being presented. Cancelling previous request.");
                currentPaywallTcs.TrySetCanceled();
            }

            currentPaywallTcs = new TaskCompletionSource<PaywallResult>();
            staticCurrentPaywallTcs = currentPaywallTcs; // Store for static callback

            try
            {
                Debug.Log($"[RevenueCatUI] Presenting paywall if needed for entitlement: {requiredEntitlementIdentifier}, offering: {options?.OfferingIdentifier ?? "default"}, displayCloseButton: {options?.DisplayCloseButton ?? false}");
                
                presentPaywallIfNeeded(
                    requiredEntitlementIdentifier,
                    options?.OfferingIdentifier,
                    options?.DisplayCloseButton ?? false,
                    OnPaywallResult
                );

                return await currentPaywallTcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RevenueCatUI] Error presenting conditional paywall: {ex.Message}");
                currentPaywallTcs?.TrySetException(ex);
                return PaywallResult.Error;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(PaywallResultCallback))]
        private static void OnPaywallResult(string result)
        {
            Debug.Log($"[RevenueCatUI] Paywall result received: {result}");
            
            var currentTcs = staticCurrentPaywallTcs;
            if (currentTcs != null)
            {
                HandlePaywallResult(currentTcs, result);
                staticCurrentPaywallTcs = null; // Clear after handling
            }
        }
        
        private static void HandlePaywallResult(TaskCompletionSource<PaywallResult> tcs, string result)
        {
            try
            {
                if (string.IsNullOrEmpty(result))
                {
                    tcs.TrySetResult(PaywallResult.Error);
                    return;
                }

                // Handle error responses
                if (result.Contains("error") || result.Contains("Error"))
                {
                    Debug.LogError($"[RevenueCatUI] Paywall error: {result}");
                    tcs.TrySetResult(PaywallResult.Error);
                    return;
                }

                // Parse result based on expected formats from PaywallProxy
                // The actual result format depends on RevenueCat UI implementation
                // Common results: purchased, cancelled, restored, not_needed
                var lowerResult = result.ToLower();
                
                if (lowerResult.Contains("purchased") || lowerResult.Contains("success"))
                {
                    tcs.TrySetResult(PaywallResult.Purchased);
                }
                else if (lowerResult.Contains("cancelled") || lowerResult.Contains("cancel"))
                {
                    tcs.TrySetResult(PaywallResult.Cancelled);
                }
                else if (lowerResult.Contains("restored") || lowerResult.Contains("restore"))
                {
                    tcs.TrySetResult(PaywallResult.Restored);
                }
                else if (lowerResult.Contains("not_needed") || lowerResult.Contains("not needed"))
                {
                    tcs.TrySetResult(PaywallResult.NotNeeded);
                }
                else
                {
                    Debug.LogWarning($"[RevenueCatUI] Unknown paywall result: {result}, defaulting to Cancelled");
                    tcs.TrySetResult(PaywallResult.Cancelled);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RevenueCatUI] Error handling paywall result: {ex.Message}");
                tcs.TrySetResult(PaywallResult.Error);
            }
        }
    }
}
#endif

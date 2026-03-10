using System;
using System.Threading;
using UnityEngine;
using RevenueCat.SimpleJSON;

namespace RevenueCatUI.Internal
{
    /// <summary>
    /// Internal bridge managing the async round-trip between native purchase logic callbacks and C#.
    /// When the native paywall triggers a purchase or restore, this bridge:
    /// 1. Receives the callback from native with a requestId (and package data for purchases)
    /// 2. Invokes the user's PurchaseLogic handler
    /// 3. Resolves the result back to native via platform-specific calls
    ///
    /// On Android, the callbacks arrive via AndroidJavaProxy.Invoke, which runs on the main
    /// thread but without Unity's SynchronizationContext set. This causes async continuations
    /// to run on thread pool threads where JNI calls silently fail. To fix this, we capture
    /// Unity's SynchronizationContext at setup time and Post the async work through it.
    /// </summary>
    internal static class PurchaseLogicBridge
    {
        private static PurchaseLogic s_currentPurchaseLogic;
        private static SynchronizationContext s_mainThreadContext;

        internal static bool HasPurchaseLogic => s_currentPurchaseLogic != null;

        internal static void SetCurrentPurchaseLogic(PurchaseLogic logic)
        {
            s_currentPurchaseLogic = logic;
            // Capture Unity's main thread SynchronizationContext.
            // This is called from the presenter on the main thread.
            s_mainThreadContext = SynchronizationContext.Current;
        }

        internal static void ClearCurrentPurchaseLogic()
        {
            s_currentPurchaseLogic = null;
        }

        /// <summary>
        /// Called from native when a purchase is requested.
        /// Dispatches to the main thread SynchronizationContext to ensure
        /// async continuations and JNI calls run on the correct thread.
        /// </summary>
        internal static void OnPerformPurchase(string requestId, string packageJson)
        {
            if (string.IsNullOrEmpty(requestId)) return;

            if (s_mainThreadContext != null)
            {
                s_mainThreadContext.Post(_ => OnPerformPurchaseAsync(requestId, packageJson), null);
            }
            else
            {
                OnPerformPurchaseAsync(requestId, packageJson);
            }
        }

        /// <summary>
        /// Called from native when a restore is requested.
        /// </summary>
        internal static void OnPerformRestore(string requestId)
        {
            if (string.IsNullOrEmpty(requestId)) return;

            if (s_mainThreadContext != null)
            {
                s_mainThreadContext.Post(_ => OnPerformRestoreAsync(requestId), null);
            }
            else
            {
                OnPerformRestoreAsync(requestId);
            }
        }

        private static async void OnPerformPurchaseAsync(string requestId, string packageJson)
        {
            if (s_currentPurchaseLogic?.PerformPurchase == null)
            {
                Debug.LogError("[RevenueCatUI] PurchaseLogic.PerformPurchase is null");
                ResolveResult(requestId, PurchaseLogicResult.Error, "No PerformPurchase handler", "PURCHASE");
                return;
            }

            try
            {
                var packageNode = JSON.Parse(packageJson);
                var package_ = new Purchases.Package(packageNode);
                var purchaseParams = new PurchaseLogicPurchaseParams(package_);
                var result = await s_currentPurchaseLogic.PerformPurchase(purchaseParams);
                ResolveResult(requestId, result, null, "PURCHASE");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error in PerformPurchase handler: {e.Message}");
                ResolveResult(requestId, PurchaseLogicResult.Error, e.Message, "PURCHASE");
            }
        }

        private static async void OnPerformRestoreAsync(string requestId)
        {
            if (s_currentPurchaseLogic?.PerformRestore == null)
            {
                Debug.LogError("[RevenueCatUI] PurchaseLogic.PerformRestore is null");
                ResolveResult(requestId, PurchaseLogicResult.Error, "No PerformRestore handler", "RESTORE");
                return;
            }

            try
            {
                var result = await s_currentPurchaseLogic.PerformRestore();
                ResolveResult(requestId, result, null, "RESTORE");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Error in PerformRestore handler: {e.Message}");
                ResolveResult(requestId, PurchaseLogicResult.Error, e.Message, "RESTORE");
            }
        }

        private static void ResolveResult(string requestId, PurchaseLogicResult result, string errorMessage, string operationType)
        {
#if UNITY_IOS && !UNITY_EDITOR
            IOSResolveResult(requestId, result.ToNativeString(), errorMessage);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidResolveResult(requestId, result.ToNativeString(), errorMessage, operationType);
#else
            Debug.LogWarning("[RevenueCatUI] PurchaseLogicBridge.ResolveResult called on unsupported platform");
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void rcui_resolvePurchaseLogicResult(string requestId, string resultString, string errorMessage);

        private static void IOSResolveResult(string requestId, string resultString, string errorMessage)
        {
            rcui_resolvePurchaseLogicResult(requestId, resultString, errorMessage);
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void AndroidResolveResult(string requestId, string resultString, string errorMessage, string operationType)
        {
            try
            {
                using var cls = new AndroidJavaClass("com.revenuecat.purchasesunity.ui.RevenueCatUI");
                // Notify before resolving so lastResult is set before the ViewModel potentially dismisses the paywall.
                cls.CallStatic("notifyPurchaseLogicResult", operationType, resultString);
                cls.CallStatic("resolvePurchaseLogicResult", requestId, resultString, errorMessage);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI] Failed to resolve purchase logic result: {e.Message}");
            }
        }
#endif
    }
}

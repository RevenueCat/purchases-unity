#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RevenueCatUI.Internal;

namespace RevenueCatUI.Platforms
{
    internal class IOSPaywallPresenter : IPaywallPresenter
    {
        private delegate void PaywallResultCallback(string result);
        private delegate void PurchaseLogicPurchaseCallback(string requestId, string packageJson);
        private delegate void PurchaseLogicRestoreCallback(string requestId);

        [DllImport("__Internal")] private static extern void rcui_presentPaywall(string offeringIdentifier, string presentedOfferingContextJson, bool displayCloseButton, bool useFullScreenPresentation, PaywallResultCallback cb);
        [DllImport("__Internal")] private static extern void rcui_presentPaywallIfNeeded(string requiredEntitlementIdentifier, string offeringIdentifier, string presentedOfferingContextJson, bool displayCloseButton, bool useFullScreenPresentation, PaywallResultCallback cb);
        [DllImport("__Internal")] private static extern void rcui_presentPaywallWithPurchaseLogic(string offeringIdentifier, string presentedOfferingContextJson, bool displayCloseButton, bool useFullScreenPresentation, PurchaseLogicPurchaseCallback purchaseCallback, PurchaseLogicRestoreCallback restoreCallback, PaywallResultCallback resultCallback);
        [DllImport("__Internal")] private static extern void rcui_presentPaywallIfNeededWithPurchaseLogic(string requiredEntitlementIdentifier, string offeringIdentifier, string presentedOfferingContextJson, bool displayCloseButton, bool useFullScreenPresentation, PurchaseLogicPurchaseCallback purchaseCallback, PurchaseLogicRestoreCallback restoreCallback, PaywallResultCallback resultCallback);

        private static TaskCompletionSource<PaywallResult> s_current;

        public Task<PaywallResult> PresentPaywallAsync(PaywallOptions options)
        {
            if (s_current != null && !s_current.Task.IsCompleted)
            {
                UnityEngine.Debug.LogWarning("[RevenueCatUI][iOS] Paywall presentation already in progress; rejecting new request.");
                return Task.FromResult(PaywallResult.Error);
            }

            var tcs = new TaskCompletionSource<PaywallResult>();
            s_current = tcs;
            try
            {
                var presentedOfferingContextJson = options?.PresentedOfferingContext?.ToJsonString();
                var useFullScreen = options?.PresentationConfiguration?.IOS == IOSPaywallPresentationStyle.FullScreen;
                if (options?.PurchaseLogic != null)
                {
                    PurchaseLogicBridge.SetCurrentPurchaseLogic(options.PurchaseLogic);
                    rcui_presentPaywallWithPurchaseLogic(
                        options.OfferingIdentifier,
                        presentedOfferingContextJson,
                        options.DisplayCloseButton,
                        useFullScreen,
                        OnPerformPurchase,
                        OnPerformRestore,
                        OnResultWithPurchaseLogic);
                }
                else
                {
                    rcui_presentPaywall(options?.OfferingIdentifier, presentedOfferingContextJson, options?.DisplayCloseButton ?? false, useFullScreen, OnResult);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Exception in presentPaywall: {e.Message}");
                tcs.TrySetResult(PaywallResult.Error);
                s_current = null;
                PurchaseLogicBridge.ClearCurrentPurchaseLogic();
            }
            return tcs.Task;
        }

        public Task<PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallOptions options)
        {
            if (s_current != null && !s_current.Task.IsCompleted)
            {
                UnityEngine.Debug.LogWarning("[RevenueCatUI][iOS] Paywall presentation already in progress; rejecting new request.");
                return Task.FromResult(PaywallResult.Error);
            }

            var tcs = new TaskCompletionSource<PaywallResult>();
            s_current = tcs;
            try
            {
                var presentedOfferingContextJson = options?.PresentedOfferingContext?.ToJsonString();
                var useFullScreen = options?.PresentationConfiguration?.IOS == IOSPaywallPresentationStyle.FullScreen;
                if (options?.PurchaseLogic != null)
                {
                    PurchaseLogicBridge.SetCurrentPurchaseLogic(options.PurchaseLogic);
                    rcui_presentPaywallIfNeededWithPurchaseLogic(
                        requiredEntitlementIdentifier,
                        options.OfferingIdentifier,
                        presentedOfferingContextJson,
                        options.DisplayCloseButton,
                        useFullScreen,
                        OnPerformPurchase,
                        OnPerformRestore,
                        OnResultWithPurchaseLogic);
                }
                else
                {
                    rcui_presentPaywallIfNeeded(requiredEntitlementIdentifier, options?.OfferingIdentifier, presentedOfferingContextJson, options?.DisplayCloseButton ?? true, useFullScreen, OnResult);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Exception in presentPaywallIfNeeded: {e.Message}");
                tcs.TrySetResult(PaywallResult.Error);
                s_current = null;
                PurchaseLogicBridge.ClearCurrentPurchaseLogic();
            }
            return tcs.Task;
        }

        [AOT.MonoPInvokeCallback(typeof(PaywallResultCallback))]
        private static void OnResult(string result)
        {
            try
            {
                var token = (result ?? "ERROR");
                var native = token.Split('|')[0];
                var type = PaywallResultTypeExtensions.FromNativeString(native);
                s_current?.TrySetResult(new PaywallResult(type));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Failed to handle paywall result '{result}': {e.Message}. Setting Error.");
                s_current?.TrySetResult(PaywallResult.Error);
            }
            finally
            {
                s_current = null;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(PaywallResultCallback))]
        private static void OnResultWithPurchaseLogic(string result)
        {
            PurchaseLogicBridge.ClearCurrentPurchaseLogic();
            OnResult(result);
        }

        [AOT.MonoPInvokeCallback(typeof(PurchaseLogicPurchaseCallback))]
        private static void OnPerformPurchase(string requestId, string packageJson)
        {
            PurchaseLogicBridge.OnPerformPurchase(requestId, packageJson);
        }

        [AOT.MonoPInvokeCallback(typeof(PurchaseLogicRestoreCallback))]
        private static void OnPerformRestore(string requestId)
        {
            PurchaseLogicBridge.OnPerformRestore(requestId);
        }
    }
}
#endif

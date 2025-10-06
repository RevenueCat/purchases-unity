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

        [DllImport("__Internal")] private static extern void rcui_presentPaywall(string offeringIdentifier, bool displayCloseButton, PaywallResultCallback cb);
        [DllImport("__Internal")] private static extern void rcui_presentPaywallIfNeeded(string requiredEntitlementIdentifier, string offeringIdentifier, bool displayCloseButton, PaywallResultCallback cb);

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
                rcui_presentPaywall(options?.OfferingIdentifier, options?.DisplayCloseButton ?? false, OnResult);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Exception in presentPaywall: {e.Message}");
                tcs.TrySetResult(PaywallResult.Error);
                s_current = null;
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
                rcui_presentPaywallIfNeeded(requiredEntitlementIdentifier, options?.OfferingIdentifier, options?.DisplayCloseButton ?? true, OnResult);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Exception in presentPaywallIfNeeded: {e.Message}");
                tcs.TrySetResult(PaywallResult.Error);
                s_current = null;
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
    }
}
#endif

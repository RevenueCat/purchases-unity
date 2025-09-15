#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RevenueCat.UI.Platforms
{
    internal class IOSPaywallPresenter : IPaywallPresenter
    {
        private delegate void PaywallResultCallback(string result);

        [DllImport("__Internal")] private static extern void rcui_presentPaywall(string offeringIdentifier, bool displayCloseButton, PaywallResultCallback cb);
        [DllImport("__Internal")] private static extern void rcui_presentPaywallIfNeeded(string requiredEntitlementIdentifier, string offeringIdentifier, bool displayCloseButton, PaywallResultCallback cb);
        [DllImport("__Internal")] private static extern bool rcui_isSupported();

        private static TaskCompletionSource<PaywallResult> s_current;

        public bool IsSupported() => rcui_isSupported();

        public Task<PaywallResult> PresentPaywallAsync(PaywallOptions options)
        {
            s_current = new TaskCompletionSource<PaywallResult>();
            try
            {
                rcui_presentPaywall(options?.OfferingIdentifier, options?.DisplayCloseButton ?? false, OnResult);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Exception in presentPaywall: {e.Message}");
                s_current.TrySetResult(PaywallResult.Error);
            }
            return s_current.Task;
        }

        public Task<PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallOptions options)
        {
            s_current = new TaskCompletionSource<PaywallResult>();
            try
            {
                rcui_presentPaywallIfNeeded(requiredEntitlementIdentifier, options?.OfferingIdentifier, options?.DisplayCloseButton ?? false, OnResult);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[RevenueCatUI][iOS] Exception in presentPaywallIfNeeded: {e.Message}");
                s_current.TrySetResult(PaywallResult.Error);
            }
            return s_current.Task;
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

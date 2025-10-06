#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using RevenueCatUI.Internal;

namespace RevenueCatUI.Platforms
{
    internal class AndroidPaywallPresenter : IPaywallPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private readonly CallbacksProxy _callbacks;
        private TaskCompletionSource<PaywallResult> _current;

        public AndroidPaywallPresenter()
        {
            try
            {
                _plugin = new AndroidJavaClass("com.revenuecat.purchasesunity.ui.RevenueCatUI");
                _callbacks = new CallbacksProxy(this);
                _plugin.CallStatic("registerPaywallCallbacks", _callbacks);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Failed to initialize RevenueCatUI plugin: {e.Message}");
            }
        }

        ~AndroidPaywallPresenter()
        {
            try { _plugin?.CallStatic("unregisterPaywallCallbacks"); } catch { }
        }

        public Task<PaywallResult> PresentPaywallAsync(PaywallOptions options)
        {
            if (_plugin == null)
            {
                Debug.LogError("[RevenueCatUI][Android] Plugin not initialized. Cannot present paywall.");
                return Task.FromResult(PaywallResult.Error);
            }

            if (_current != null && !_current.Task.IsCompleted)
            {
                Debug.LogWarning("[RevenueCatUI][Android] Paywall presentation already in progress; rejecting new request.");
                return Task.FromResult(PaywallResult.Error);
            }

            _current = new TaskCompletionSource<PaywallResult>();
            try
            {
                var offering = options?.OfferingIdentifier;
                var displayCloseButton = options?.DisplayCloseButton ?? false;
                
                Debug.Log($"[RevenueCatUI][Android] presentPaywall offering='{offering ?? "<null>"}', displayCloseButton={displayCloseButton}");
                var currentActivity = AndroidApplication.currentActivity;
                _plugin.CallStatic("presentPaywall", new object[] { currentActivity, offering, displayCloseButton });
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Exception in presentPaywall: {e.Message}");
                _current.TrySetResult(PaywallResult.Error);
                _current = null;
            }
            return _current.Task;
        }

        public Task<PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallOptions options)
        {
            if (_plugin == null)
            {
                Debug.LogError("[RevenueCatUI][Android] Plugin not initialized. Cannot present paywall.");
                return Task.FromResult(PaywallResult.Error);
            }

            if (_current != null && !_current.Task.IsCompleted)
            {
                Debug.LogWarning("[RevenueCatUI][Android] Paywall presentation already in progress; rejecting new request.");
                return Task.FromResult(PaywallResult.Error);
            }

            _current = new TaskCompletionSource<PaywallResult>();
            try
            {
                var offering = options?.OfferingIdentifier;
                var displayCloseButton = options?.DisplayCloseButton ?? true;
                Debug.Log($"[RevenueCatUI][Android] presentPaywallIfNeeded entitlement='{requiredEntitlementIdentifier}', offering='{offering ?? "<null>"}', displayCloseButton={displayCloseButton}");
                var currentActivity = AndroidApplication.currentActivity;
                _plugin.CallStatic("presentPaywallIfNeeded", new object[] { currentActivity, requiredEntitlementIdentifier, offering, displayCloseButton });
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Exception in presentPaywallIfNeeded: {e.Message}");
                _current.TrySetResult(PaywallResult.Error);
                _current = null;
            }
            return _current.Task;
        }

        // Called from RevenueCatUI MonoBehaviour or Java via AndroidJavaProxy
        public void OnPaywallResult(string resultData)
        {
            if (_current == null) return;
            try
            {
                var token = resultData?.Split('|')[0] ?? "ERROR";
                var type = PaywallResultTypeExtensions.FromNativeString(token);
                _current.TrySetResult(new PaywallResult(type));
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Failed to handle paywall result '{resultData}': {e.Message}. Setting Error.");
                _current.TrySetResult(PaywallResult.Error);
            }
            finally
            {
                _current = null;
            }
        }

        private class CallbacksProxy : AndroidJavaProxy
        {
            private readonly AndroidPaywallPresenter _owner;
            public CallbacksProxy(AndroidPaywallPresenter owner) : base("com.revenuecat.purchasesunity.ui.RevenueCatUI$PaywallCallbacks")
            {
                _owner = owner;
            }

            // Signature matches Java interface
            public void onPaywallResult(string result)
            {
                _owner.OnPaywallResult(result);
            }
        }
    }
}
#endif

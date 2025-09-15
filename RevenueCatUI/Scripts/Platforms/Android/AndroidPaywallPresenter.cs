#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UI.Internal;

namespace RevenueCat.UI.Platforms
{
    internal class AndroidPaywallPresenter : IPaywallPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private readonly CallbacksProxy _callbacks;
        private TaskCompletionSource<PaywallResult> _current;

        public AndroidPaywallPresenter()
        {
            _plugin = new AndroidJavaClass("com.revenuecat.unity.RevenueCatUI");
            _callbacks = new CallbacksProxy(this);
            try { _plugin.CallStatic("registerCallbacks", _callbacks); } catch { /* ignore */ }
        }

        public bool IsSupported()
        {
            try { return _plugin.CallStatic<bool>("isSupported"); }
            catch { return false; }
        }

        public Task<PaywallResult> PresentPaywallAsync(PaywallOptions options)
        {
            _current = new TaskCompletionSource<PaywallResult>();
            try
            {
                var offering = options?.OfferingIdentifier;
                _plugin.CallStatic("presentPaywall", offering);
            }
            catch (Exception)
            {
                _current.TrySetResult(PaywallResult.Error);
            }
            return _current.Task;
        }

        public Task<PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallOptions options)
        {
            _current = new TaskCompletionSource<PaywallResult>();
            try
            {
                var offering = options?.OfferingIdentifier;
                _plugin.CallStatic("presentPaywallIfNeeded", requiredEntitlementIdentifier, offering);
            }
            catch (Exception)
            {
                _current.TrySetResult(PaywallResult.Error);
            }
            return _current.Task;
        }

        // Called from Java via AndroidJavaProxy
        public void OnPaywallResult(string resultData)
        {
            if (_current == null) return;
            try
            {
                var token = resultData?.Split('|')[0] ?? "ERROR";
                var type = PaywallResultTypeExtensions.FromNativeString(token);
                _current.TrySetResult(new PaywallResult(type));
            }
            catch
            {
                _current.TrySetResult(PaywallResult.Error);
            }
        }

        private class CallbacksProxy : AndroidJavaProxy
        {
            private readonly AndroidPaywallPresenter _owner;
            public CallbacksProxy(AndroidPaywallPresenter owner) : base("com.revenuecat.unity.RevenueCatUI$RevenueCatUICallbacks")
            {
                _owner = owner;
            }

            // Signature matches Java interface
            public void onPaywallResult(string result)
            {
                _owner.OnPaywallResult(result);
            }

            public void onCustomerCenterResult(string result)
            {
                // No-op for paywall presenter
            }
        }
    }
}
#endif

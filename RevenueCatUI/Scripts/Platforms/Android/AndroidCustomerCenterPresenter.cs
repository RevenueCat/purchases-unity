#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;
// No Internal handler needed; Android uses direct callbacks via AndroidJavaProxy

namespace RevenueCat.UI.Platforms
{
    internal class AndroidCustomerCenterPresenter : ICustomerCenterPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private readonly CallbacksProxy _callbacks;
        private TaskCompletionSource<bool> _current;

        public AndroidCustomerCenterPresenter()
        {
            _plugin = new AndroidJavaClass("com.revenuecat.unity.ui.RevenueCatUI");
            _callbacks = new CallbacksProxy(this);
            try { _plugin.CallStatic("registerCustomerCenterCallbacks", _callbacks); } catch { /* ignore */ }
        }

        ~AndroidCustomerCenterPresenter()
        {
            try { _plugin.CallStatic("unregisterCustomerCenterCallbacks"); } catch { }
        }

        public bool IsSupported()
        {
            try { return _plugin.CallStatic<bool>("isSupported"); }
            catch { return false; }
        }

        public Task PresentCustomerCenterAsync()
        {
            if (_current != null && !_current.Task.IsCompleted)
            {
                Debug.LogWarning("[RevenueCatUI][Android] Customer Center already in progress; rejecting new request.");
                return Task.CompletedTask;
            }

            _current = new TaskCompletionSource<bool>();
            try { _plugin.CallStatic("presentCustomerCenter"); }
            catch { _current.TrySetResult(false); _current = null; }
            return _current?.Task ?? Task.CompletedTask;
        }

        // Called from Java via AndroidJavaProxy
        public void OnCustomerCenterResult(string resultData)
        {
            _current?.TrySetResult(true);
            _current = null;
        }

        private class CallbacksProxy : AndroidJavaProxy
        {
            private readonly AndroidCustomerCenterPresenter _owner;
            public CallbacksProxy(AndroidCustomerCenterPresenter owner) : base("com.revenuecat.unity.ui.RevenueCatUI$CustomerCenterCallbacks")
            {
                _owner = owner;
            }

            public void onCustomerCenterResult(string result)
            {
                _owner.OnCustomerCenterResult(result);
            }
        }
    }
}
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using RevenueCatUI.Internal;
using UnityEngine;
using UnityEngine.Android;

namespace RevenueCatUI.Platforms
{
    internal class AndroidCustomerCenterPresenter : ICustomerCenterPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private readonly CallbacksProxy _callbacks;
        private TaskCompletionSource<CustomerCenterResult> _current;

        public AndroidCustomerCenterPresenter()
        {
            try
            {
                _plugin = new AndroidJavaClass("com.revenuecat.purchasesunity.ui.RevenueCatUI");
                _callbacks = new CallbacksProxy(this);
                _plugin.CallStatic("registerCustomerCenterCallbacks", _callbacks);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Failed to initialize RevenueCatUI plugin for Customer Center: {e.Message}");
            }
        }

        ~AndroidCustomerCenterPresenter()
        {
            try { _plugin?.CallStatic("unregisterCustomerCenterCallbacks"); } catch { }
        }

        public Task<CustomerCenterResult> PresentAsync(CustomerCenterCallbacks callbacks)
        {
            if (_plugin == null)
            {
                Debug.LogError("[RevenueCatUI][Android] Plugin not initialized. Cannot present Customer Center.");
                return Task.FromResult(CustomerCenterResult.Error);
            }

            if (_current != null && !_current.Task.IsCompleted)
            {
                Debug.LogWarning("[RevenueCatUI][Android] Customer Center presentation already in progress; rejecting new request.");
                return _current.Task;
            }

            var currentActivity = AndroidApplication.currentActivity;
            if (currentActivity == null)
            {
                Debug.LogError("[RevenueCatUI][Android] Current activity is null. Cannot present Customer Center.");
                return Task.FromResult(CustomerCenterResult.Error);
            }

            _current = new TaskCompletionSource<CustomerCenterResult>();
            try
            {
                _plugin.CallStatic("presentCustomerCenter", currentActivity);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Exception in presentCustomerCenter: {e.Message}");
                _current.TrySetResult(CustomerCenterResult.Error);
                var task = _current.Task;
                _current = null;
                return task;
            }

            return _current.Task;
        }

        private void OnCustomerCenterResult(string resultData)
        {
            if (_current == null)
            {
                Debug.LogWarning("[RevenueCatUI][Android] Received Customer Center result with no pending request.");
                return;
            }

            try
            {
                var token = resultData?.Split('|')[0] ?? "ERROR";
                var type = CustomerCenterResultTypeExtensions.FromNativeString(token);
                _current.TrySetResult(new CustomerCenterResult(type));
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Failed to handle Customer Center result '{resultData}': {e.Message}. Setting Error.");
                _current.TrySetResult(CustomerCenterResult.Error);
            }
            finally
            {
                _current = null;
            }
        }

        private class CallbacksProxy : AndroidJavaProxy
        {
            private readonly AndroidCustomerCenterPresenter _owner;

            public CallbacksProxy(AndroidCustomerCenterPresenter owner) : base("com.revenuecat.purchasesunity.ui.RevenueCatUI$CustomerCenterCallbacks")
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

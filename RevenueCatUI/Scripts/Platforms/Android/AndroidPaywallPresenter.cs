#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCatUI.Internal;

namespace RevenueCatUI.Platforms
{
    internal class AndroidPaywallPresenter : IPaywallPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private readonly CallbacksProxy _callbacks;
        private readonly PurchaseLogicCallbacksProxy _purchaseLogicCallbacks;
        private TaskCompletionSource<PaywallResult> _current;

        public AndroidPaywallPresenter()
        {
            try
            {
                _plugin = new AndroidJavaClass("com.revenuecat.purchasesunity.ui.RevenueCatUI");
                _callbacks = new CallbacksProxy(this);
                _purchaseLogicCallbacks = new PurchaseLogicCallbacksProxy();
                _plugin.CallStatic("registerPaywallCallbacks", _callbacks);
                _plugin.CallStatic("registerPurchaseLogicCallbacks", _purchaseLogicCallbacks);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Failed to initialize RevenueCatUI plugin: {e.Message}");
            }
        }

        ~AndroidPaywallPresenter()
        {
            try { _plugin?.CallStatic("unregisterPaywallCallbacks"); } catch { }
            try { _plugin?.CallStatic("unregisterPurchaseLogicCallbacks"); } catch { }
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
                var offeringIdentifier = options?.OfferingIdentifier;
                var displayCloseButton = options?.DisplayCloseButton ?? false;
                var presentedOfferingContextJson = options?.PresentedOfferingContext?.ToJsonString();
                var hasPurchaseLogic = options?.PurchaseLogic != null;

                if (hasPurchaseLogic)
                {
                    PurchaseLogicBridge.SetCurrentPurchaseLogic(options.PurchaseLogic);
                }

                var currentActivity = AndroidActivityUtils.GetCurrentActivity();
                _plugin.CallStatic("presentPaywall", new object[] { currentActivity, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, hasPurchaseLogic });
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Exception in presentPaywall: {e.Message}");
                _current.TrySetResult(PaywallResult.Error);
                _current = null;
                PurchaseLogicBridge.ClearCurrentPurchaseLogic();
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
                var offeringIdentifier = options?.OfferingIdentifier;
                var displayCloseButton = options?.DisplayCloseButton ?? true;
                var presentedOfferingContextJson = options?.PresentedOfferingContext?.ToJsonString();
                var hasPurchaseLogic = options?.PurchaseLogic != null;

                if (hasPurchaseLogic)
                {
                    PurchaseLogicBridge.SetCurrentPurchaseLogic(options.PurchaseLogic);
                }

                var currentActivity = AndroidActivityUtils.GetCurrentActivity();
                _plugin.CallStatic("presentPaywallIfNeeded", new object[] { currentActivity, requiredEntitlementIdentifier, offeringIdentifier, presentedOfferingContextJson, displayCloseButton, hasPurchaseLogic });
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Exception in presentPaywallIfNeeded: {e.Message}");
                _current.TrySetResult(PaywallResult.Error);
                _current = null;
                PurchaseLogicBridge.ClearCurrentPurchaseLogic();
            }
            return _current.Task;
        }

        // Called from Java via AndroidJavaProxy when paywall is dismissed
        public void OnPaywallResult(string resultData)
        {
            PurchaseLogicBridge.ClearCurrentPurchaseLogic();
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
            internal CallbacksProxy(AndroidPaywallPresenter owner) : base("com.revenuecat.purchasesunity.ui.RevenueCatUI$PaywallCallbacks")
            {
                _owner = owner;
            }

            public void onPaywallResult(string result)
            {
                _owner.OnPaywallResult(result);
            }
        }

        private class PurchaseLogicCallbacksProxy : AndroidJavaProxy
        {
            internal PurchaseLogicCallbacksProxy() : base("com.revenuecat.purchasesunity.ui.RevenueCatUI$PurchaseLogicCallbacks")
            {
            }

            public void onPerformPurchase(string requestId, string packageJson)
            {
                PurchaseLogicBridge.OnPerformPurchase(requestId, packageJson);
            }

            public void onPerformRestore(string requestId)
            {
                PurchaseLogicBridge.OnPerformRestore(requestId);
            }
        }
    }
}
#endif

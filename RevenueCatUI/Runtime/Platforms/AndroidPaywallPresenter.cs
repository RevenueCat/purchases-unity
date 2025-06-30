#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;
using RevenueCat.UI.Internal;

namespace RevenueCat.UI.Platforms
{
    /// <summary>
    /// Android implementation of the paywall presenter.
    /// Uses purchases-hybrid-common presentPaywallFromFragment.
    /// </summary>
    internal class AndroidPaywallPresenter : IPaywallPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private TaskCompletionSource<PaywallResult> _currentPaywallTask;

        public AndroidPaywallPresenter()
        {
            _plugin = new AndroidJavaClass("com.revenuecat.purchases.ui.unity.RevenueCatUIPlugin");
            RevenueCatUICallbackHandler.Initialize();
            RevenueCatUICallbackHandler.SetAndroidPresenter(this);
        }

        public bool IsSupported()
        {
            try
            {
                return _plugin.CallStatic<bool>("isSupported");
            }
            catch (Exception e)
            {
                Debug.LogError($"RevenueCatUI: Error checking if supported: {e.Message}");
                return false;
            }
        }

        public Task<PaywallResult> PresentPaywallAsync(PaywallOptions options)
        {
            if (_currentPaywallTask != null && !_currentPaywallTask.Task.IsCompleted)
            {
                _currentPaywallTask.TrySetCanceled();
            }

            _currentPaywallTask = new TaskCompletionSource<PaywallResult>();

            try
            {
                string offeringId = options?.OfferingIdentifier;

                // For regular paywall presentation, we don't need an entitlement ID
                _plugin.CallStatic("presentPaywall", "", offeringId ?? "");
            }
            catch (Exception e)
            {
                Debug.LogError($"RevenueCatUI: Error presenting paywall: {e.Message}");
                _currentPaywallTask.TrySetResult(PaywallResult.Error);
            }

            return _currentPaywallTask.Task;
        }

        public Task<PaywallResult> PresentPaywallIfNeededAsync(string requiredEntitlementIdentifier, PaywallOptions options)
        {
            if (_currentPaywallTask != null && !_currentPaywallTask.Task.IsCompleted)
            {
                _currentPaywallTask.TrySetCanceled();
            }

            _currentPaywallTask = new TaskCompletionSource<PaywallResult>();

            try
            {
                string entitlementId = requiredEntitlementIdentifier ?? "";
                string offeringId = options?.OfferingIdentifier;

                _plugin.CallStatic("presentPaywallIfNeeded", entitlementId, offeringId ?? "");
            }
            catch (Exception e)
            {
                Debug.LogError($"RevenueCatUI: Error presenting paywall if needed: {e.Message}");
                _currentPaywallTask.TrySetResult(PaywallResult.Error);
            }

            return _currentPaywallTask.Task;
        }

        // Called from Java via UnitySendMessage
        public void OnPaywallResult(string resultData)
        {
            if (_currentPaywallTask == null || _currentPaywallTask.Task.IsCompleted)
                return;

            try
            {
                string[] parts = resultData.Split('|');
                string resultString = parts.Length > 0 ? parts[0] : "ERROR";
                string message = parts.Length > 1 ? parts[1] : "";

                PaywallResult result;
                switch (resultString.ToUpper())
                {
                    case "PURCHASED":
                        result = PaywallResult.Purchased;
                        break;
                    case "CANCELLED":
                    case "CANCELED":
                        result = PaywallResult.Cancelled;
                        break;
                    case "RESTORED":
                        result = PaywallResult.Restored;
                        break;
                    case "NOT_PRESENTED":
                        result = PaywallResult.NotNeeded;
                        break;
                    default:
                        result = PaywallResult.Error;
                        break;
                }

                _currentPaywallTask.TrySetResult(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"RevenueCatUI: Error processing paywall result: {e.Message}");
                _currentPaywallTask.TrySetResult(PaywallResult.Error);
            }
        }
    }
}
#endif


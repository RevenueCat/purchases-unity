#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using RevenueCat;
using RevenueCat.SimpleJSON;
using RevenueCatUI.Internal;
using UnityEngine;
using UnityEngine.Android;

namespace RevenueCatUI.Platforms
{
    internal class AndroidCustomerCenterPresenter : ICustomerCenterPresenter
    {
        private readonly AndroidJavaClass _plugin;
        private readonly CallbacksProxy _callbacks;
        private TaskCompletionSource<bool> _current;
        private CustomerCenterCallbacks _storedCallbacks;

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
                Debug.LogException(e);
            }
        }

        ~AndroidCustomerCenterPresenter()
        {
            try 
            { 
                _plugin?.CallStatic("unregisterCustomerCenterCallbacks");
                _storedCallbacks = null;
                _current = null;
            } 
            catch { }
        }

        public Task PresentAsync(CustomerCenterCallbacks callbacks)
        {
            if (_plugin == null)
            {
                throw new InvalidOperationException("[RevenueCatUI][Android] Plugin not initialized. Cannot present Customer Center.");
            }

            if (_current != null && !_current.Task.IsCompleted)
            {
                Debug.LogWarning("[RevenueCatUI][Android] Customer Center presentation already in progress; returning existing task.");
                return _current.Task;
            }

            var currentActivity = AndroidApplication.currentActivity;
            if (currentActivity == null)
            {
                throw new InvalidOperationException("[RevenueCatUI][Android] Current activity is null. Cannot present Customer Center.");
            }

            _storedCallbacks = callbacks;
            _current = new TaskCompletionSource<bool>();
            
            try
            {
                _plugin.CallStatic("presentCustomerCenter", currentActivity);
            }
            catch (Exception e)
            {
                _storedCallbacks = null;
                _current = null;
                throw new InvalidOperationException($"[RevenueCatUI][Android] Exception in presentCustomerCenter: {e.Message}", e);
            }

            return _current.Task;
        }

        private void OnCustomerCenterDismissed()
        {
            if (_current == null)
            {
                Debug.LogWarning("[RevenueCatUI][Android] Received Customer Center dismissed with no pending request.");
                return;
            }

            try
            {
                _current.TrySetResult(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Failed to handle Customer Center dismissed: {e.Message}");
                _current.TrySetException(e);
            }
            finally
            {
                _storedCallbacks = null;
                _current = null;
            }
        }

        private void OnCustomerCenterError()
        {
            if (_current == null)
            {
                Debug.LogWarning("[RevenueCatUI][Android] Received Customer Center error with no pending request.");
                return;
            }

            try
            {
                Debug.LogError("[RevenueCatUI][Android] Customer Center presentation failed.");
                _current.TrySetException(new Exception("Customer Center presentation failed"));
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Failed to handle Customer Center error: {e.Message}");
                _current.TrySetException(e);
            }
            finally
            {
                _storedCallbacks = null;
                _current = null;
            }
        }

        private void OnFeedbackSurveyCompleted(string feedbackSurveyOptionId)
        {
            _storedCallbacks?.OnFeedbackSurveyCompleted?.Invoke(
                new FeedbackSurveyCompletedEventArgs(feedbackSurveyOptionId)
            );
        }

        private void OnShowingManageSubscriptions()
        {
            _storedCallbacks?.OnShowingManageSubscriptions?.Invoke();
        }

        private void OnRestoreCompleted(string customerInfoJson)
        {
            var customerInfo = new Purchases.CustomerInfo(JSON.Parse(customerInfoJson));
            _storedCallbacks?.OnRestoreCompleted?.Invoke(
                new RestoreCompletedEventArgs(customerInfo)
            );
        }

        private void OnRestoreFailed(string errorJson)
        {
            var error = new Purchases.Error(JSON.Parse(errorJson));
            _storedCallbacks?.OnRestoreFailed?.Invoke(
                new RestoreFailedEventArgs(error)
            );
        }

        private void OnRestoreStarted()
        {
            _storedCallbacks?.OnRestoreStarted?.Invoke();
        }

        private void OnRefundRequestStarted(string productIdentifier)
        {
            _storedCallbacks?.OnRefundRequestStarted?.Invoke(
                new RefundRequestStartedEventArgs(productIdentifier)
            );
        }

        private void OnRefundRequestCompleted(string productIdentifier, string refundRequestStatus)
        {
            string status;
            switch (refundRequestStatus?.ToUpperInvariant())
            {
                case "SUCCESS":
                    status = RevenueCatUI.RefundRequestStatus.Success;
                    break;
                case "USERCANCELLED":
                case "USER_CANCELLED":
                    status = RevenueCatUI.RefundRequestStatus.UserCancelled;
                    break;
                default:
                    status = RevenueCatUI.RefundRequestStatus.Error;
                    break;
            }
            
            _storedCallbacks?.OnRefundRequestCompleted?.Invoke(
                new RefundRequestCompletedEventArgs(productIdentifier, status)
            );
        }

        private void OnManagementOptionSelected(string option, string url)
        {
            string optionString;
            switch (option?.ToLowerInvariant())
            {
                case "cancel":
                    optionString = RevenueCatUI.CustomerCenterManagementOption.Cancel;
                    break;
                case "custom_url":
                    optionString = RevenueCatUI.CustomerCenterManagementOption.CustomUrl;
                    break;
                case "missing_purchase":
                    optionString = RevenueCatUI.CustomerCenterManagementOption.MissingPurchase;
                    break;
                case "refund_request":
                    optionString = RevenueCatUI.CustomerCenterManagementOption.RefundRequest;
                    break;
                case "change_plans":
                    optionString = RevenueCatUI.CustomerCenterManagementOption.ChangePlans;
                    break;
                default:
                    optionString = RevenueCatUI.CustomerCenterManagementOption.Unknown;
                    break;
            }
            
            _storedCallbacks?.OnManagementOptionSelected?.Invoke(
                new ManagementOptionSelectedEventArgs(optionString, url)
            );
        }

        private void OnCustomActionSelected(string actionId, string purchaseIdentifier)
        {
            _storedCallbacks?.OnCustomActionSelected?.Invoke(
                new CustomActionSelectedEventArgs(actionId, purchaseIdentifier)
            );
        }

        private class CallbacksProxy : AndroidJavaProxy
        {
            private readonly AndroidCustomerCenterPresenter _owner;

            internal CallbacksProxy(AndroidCustomerCenterPresenter owner) : base("com.revenuecat.purchasesunity.ui.RevenueCatUI$CustomerCenterCallbacks")
            {
                _owner = owner;
            }

            public void onCustomerCenterDismissed()
            {
                _owner.OnCustomerCenterDismissed();
            }

            public void onCustomerCenterError()
            {
                _owner.OnCustomerCenterError();
            }

            public void onFeedbackSurveyCompleted(string feedbackSurveyOptionId)
            {
                _owner.OnFeedbackSurveyCompleted(feedbackSurveyOptionId);
            }

            public void onShowingManageSubscriptions()
            {
                _owner.OnShowingManageSubscriptions();
            }

            public void onRestoreCompleted(string customerInfoJson)
            {
                _owner.OnRestoreCompleted(customerInfoJson);
            }

            public void onRestoreFailed(string errorJson)
            {
                _owner.OnRestoreFailed(errorJson);
            }

            public void onRestoreStarted()
            {
                _owner.OnRestoreStarted();
            }

            public void onRefundRequestStarted(string productIdentifier)
            {
                _owner.OnRefundRequestStarted(productIdentifier);
            }

            public void onRefundRequestCompleted(string productIdentifier, string refundRequestStatus)
            {
                _owner.OnRefundRequestCompleted(productIdentifier, refundRequestStatus);
            }

            public void onManagementOptionSelected(string option, string url)
            {
                _owner.OnManagementOptionSelected(option, url);
            }

            public void onCustomActionSelected(string actionId, string purchaseIdentifier)
            {
                _owner.OnCustomActionSelected(actionId, purchaseIdentifier);
            }
        }
    }
}
#endif

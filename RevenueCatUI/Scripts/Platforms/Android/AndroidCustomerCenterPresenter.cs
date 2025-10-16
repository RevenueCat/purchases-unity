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
                Debug.Log("[RevenueCatUI][Android] Customer Center dismissed.");
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
            try
            {
                _storedCallbacks?.OnFeedbackSurveyCompleted?.Invoke(
                    new FeedbackSurveyCompletedEventArgs(feedbackSurveyOptionId)
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Error in OnFeedbackSurveyCompleted callback: {e.Message}");
            }
        }

        private void OnShowingManageSubscriptions()
        {
            try
            {
                _storedCallbacks?.OnShowingManageSubscriptions?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Error in OnShowingManageSubscriptions callback: {e.Message}");
            }
        }

        private void OnRestoreCompleted(string customerInfoJson)
        {
            try
            {
                var customerInfo = new Purchases.CustomerInfo(JSON.Parse(customerInfoJson));
                _storedCallbacks?.OnRestoreCompleted?.Invoke(
                    new RestoreCompletedEventArgs(customerInfo)
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Error in OnRestoreCompleted callback: {e.Message}");
            }
        }

        private void OnRestoreFailed(string errorJson)
        {
            try
            {
                var error = new Purchases.Error(JSON.Parse(errorJson));
                _storedCallbacks?.OnRestoreFailed?.Invoke(
                    new RestoreFailedEventArgs(error)
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Error in OnRestoreFailed callback: {e.Message}");
            }
        }

        private void OnRestoreStarted()
        {
            try
            {
                _storedCallbacks?.OnRestoreStarted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Error in OnRestoreStarted callback: {e.Message}");
            }
        }

        private void OnRefundRequestStarted(string productIdentifier)
        {
            try
            {
                _storedCallbacks?.OnRefundRequestStarted?.Invoke(
                    new RefundRequestStartedEventArgs(productIdentifier)
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Error in OnRefundRequestStarted callback: {e.Message}");
            }
        }

        private void OnRefundRequestCompleted(string productIdentifier, string refundRequestStatus)
        {
            try
            {
                RefundRequestStatus status;
                switch (refundRequestStatus?.ToUpperInvariant())
                {
                    case "SUCCESS":
                        status = RefundRequestStatus.Success;
                        break;
                    case "USERCANCELLED":
                    case "USER_CANCELLED":
                        status = RefundRequestStatus.UserCancelled;
                        break;
                    default:
                        status = RefundRequestStatus.Error;
                        break;
                }
                
                _storedCallbacks?.OnRefundRequestCompleted?.Invoke(
                    new RefundRequestCompletedEventArgs(productIdentifier, status)
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Error in OnRefundRequestCompleted callback: {e.Message}");
            }
        }

        private void OnManagementOptionSelected(string option, string url)
        {
            try
            {
                CustomerCenterManagementOption optionEnum;
                switch (option?.ToLowerInvariant())
                {
                    case "cancel":
                        optionEnum = CustomerCenterManagementOption.Cancel;
                        break;
                    case "custom_url":
                        optionEnum = CustomerCenterManagementOption.CustomUrl;
                        break;
                    case "missing_purchase":
                        optionEnum = CustomerCenterManagementOption.MissingPurchase;
                        break;
                    case "refund_request":
                        optionEnum = CustomerCenterManagementOption.RefundRequest;
                        break;
                    case "change_plans":
                        optionEnum = CustomerCenterManagementOption.ChangePlans;
                        break;
                    default:
                        optionEnum = CustomerCenterManagementOption.Unknown;
                        break;
                }
                
                _storedCallbacks?.OnManagementOptionSelected?.Invoke(
                    new ManagementOptionSelectedEventArgs(optionEnum, url)
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Error in OnManagementOptionSelected callback: {e.Message}");
            }
        }

        private void OnCustomActionSelected(string actionId, string purchaseIdentifier)
        {
            try
            {
                _storedCallbacks?.OnCustomActionSelected?.Invoke(
                    new CustomActionSelectedEventArgs(actionId, purchaseIdentifier)
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][Android] Error in OnCustomActionSelected callback: {e.Message}");
            }
        }

        private class CallbacksProxy : AndroidJavaProxy
        {
            private readonly AndroidCustomerCenterPresenter _owner;

            public CallbacksProxy(AndroidCustomerCenterPresenter owner) : base("com.revenuecat.purchasesunity.ui.RevenueCatUI$CustomerCenterCallbacks")
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

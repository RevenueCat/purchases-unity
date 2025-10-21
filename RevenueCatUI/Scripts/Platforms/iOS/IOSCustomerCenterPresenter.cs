#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RevenueCat;
using RevenueCat.SimpleJSON;
using RevenueCatUI.Internal;
using UnityEngine;

namespace RevenueCatUI.Platforms
{
    internal class IOSCustomerCenterPresenter : ICustomerCenterPresenter
    {
        private delegate void CustomerCenterDismissedCallback();
        private delegate void CustomerCenterErrorCallback();
        private delegate void CustomerCenterEventCallback(string eventName, string payload);

        [DllImport("__Internal")] 
        private static extern void rcui_presentCustomerCenter(
            CustomerCenterDismissedCallback dismissedCallback,
            CustomerCenterErrorCallback errorCallback,
            CustomerCenterEventCallback eventCallback);

        private static TaskCompletionSource<bool> s_current;
        private static CustomerCenterCallbacks s_storedCallbacks;

        ~IOSCustomerCenterPresenter()
        {
            s_storedCallbacks = null;
            s_current = null;
        }

        public Task PresentAsync(CustomerCenterCallbacks callbacks)
        {
            if (s_current != null && !s_current.Task.IsCompleted)
            {
                Debug.LogWarning("[RevenueCatUI][iOS] Customer Center presentation already in progress; returning existing task.");
                return s_current.Task;
            }

            s_storedCallbacks = callbacks;
            var tcs = new TaskCompletionSource<bool>();
            s_current = tcs;
            
            try
            {
                rcui_presentCustomerCenter(OnDismissed, OnError, OnEvent);
            }
            catch (Exception e)
            {
                s_storedCallbacks = null;
                s_current = null;
                throw new InvalidOperationException($"[RevenueCatUI][iOS] Exception in presentCustomerCenter: {e.Message}", e);
            }
            
            return tcs.Task;
        }

        [AOT.MonoPInvokeCallback(typeof(CustomerCenterDismissedCallback))]
        private static void OnDismissed()
        {
            if (s_current == null)
            {
                Debug.LogWarning("[RevenueCatUI][iOS] Received Customer Center dismissed with no pending request.");
                return;
            }

            try
            {
                s_current.TrySetResult(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][iOS] Failed to handle Customer Center dismissed: {e.Message}");
                s_current.TrySetException(e);
            }
            finally
            {
                s_storedCallbacks = null;
                s_current = null;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(CustomerCenterErrorCallback))]
        private static void OnError()
        {
            if (s_current == null)
            {
                Debug.LogWarning("[RevenueCatUI][iOS] Received Customer Center error with no pending request.");
                return;
            }

            try
            {
                Debug.LogError("[RevenueCatUI][iOS] Customer Center presentation failed.");
                s_current.TrySetException(new Exception("Customer Center presentation failed"));
            }
            catch (Exception e)
            {
                Debug.LogError($"[RevenueCatUI][iOS] Failed to handle Customer Center error: {e.Message}");
                s_current.TrySetException(e);
            }
            finally
            {
                s_storedCallbacks = null;
                s_current = null;
            }
        }

        private static string NormalizeNullString(string value)
        {
            return string.IsNullOrEmpty(value) || value == "null" ? null : value;
        }

        [AOT.MonoPInvokeCallback(typeof(CustomerCenterEventCallback))]
        private static void OnEvent(string eventName, string payload)
        {
            switch (eventName)
            {
                case "onRestoreStarted":
                    s_storedCallbacks?.OnRestoreStarted?.Invoke();
                    break;
                    
                case "onRestoreCompleted":
                    if (!string.IsNullOrEmpty(payload))
                    {
                        var customerInfo = new Purchases.CustomerInfo(JSON.Parse(payload));
                        s_storedCallbacks?.OnRestoreCompleted?.Invoke(
                            new RestoreCompletedEventArgs(customerInfo)
                        );
                    }
                    break;
                    
                case "onRestoreFailed":
                    if (!string.IsNullOrEmpty(payload))
                    {
                        var error = new Purchases.Error(JSON.Parse(payload));
                        s_storedCallbacks?.OnRestoreFailed?.Invoke(
                            new RestoreFailedEventArgs(error)
                        );
                    }
                    break;
                    
                case "onShowingManageSubscriptions":
                    s_storedCallbacks?.OnShowingManageSubscriptions?.Invoke();
                    break;
                    
                case "onRefundRequestStarted":
                    if (!string.IsNullOrEmpty(payload))
                    {
                        s_storedCallbacks?.OnRefundRequestStarted?.Invoke(
                            new RefundRequestStartedEventArgs(payload)
                        );
                    }
                    break;
                    
                case "onRefundRequestCompleted":
                    if (!string.IsNullOrEmpty(payload))
                    {
                        var data = JSON.Parse(payload);
                        var productIdentifier = data["productIdentifier"]?.Value;
                        var statusString = data["refundRequestStatus"]?.Value;
                        
                        RefundRequestStatus status;
                        switch (statusString?.ToUpperInvariant())
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
                        
                        s_storedCallbacks?.OnRefundRequestCompleted?.Invoke(
                            new RefundRequestCompletedEventArgs(productIdentifier, status)
                        );
                    }
                    break;
                    
                case "onFeedbackSurveyCompleted":
                    if (!string.IsNullOrEmpty(payload))
                    {
                        s_storedCallbacks?.OnFeedbackSurveyCompleted?.Invoke(
                            new FeedbackSurveyCompletedEventArgs(payload)
                        );
                    }
                    break;
                    
                case "onManagementOptionSelected":
                    if (!string.IsNullOrEmpty(payload))
                    {
                        var data = JSON.Parse(payload);
                        var option = data["option"]?.Value;
                        var url = NormalizeNullString(data["url"]?.Value);
                        
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
                        
                        s_storedCallbacks?.OnManagementOptionSelected?.Invoke(
                            new ManagementOptionSelectedEventArgs(optionEnum, url)
                        );
                    }
                    break;
                    
                case "onCustomActionSelected":
                    if (!string.IsNullOrEmpty(payload))
                    {
                        var data = JSON.Parse(payload);
                        var actionId = data["actionId"]?.Value;
                        var purchaseIdentifier = NormalizeNullString(data["purchaseIdentifier"]?.Value);
                        
                        s_storedCallbacks?.OnCustomActionSelected?.Invoke(
                            new CustomActionSelectedEventArgs(actionId, purchaseIdentifier)
                        );
                    }
                    break;
                    
                default:
                    Debug.LogWarning($"[RevenueCatUI][iOS] Unknown customer center event: {eventName}");
                    break;
            }
        }
    }
}
#endif

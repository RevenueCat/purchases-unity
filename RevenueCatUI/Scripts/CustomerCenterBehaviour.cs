using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using RevenueCat;

namespace RevenueCatUI
{
    /// <summary>
    /// MonoBehaviour component for presenting the RevenueCat Customer Center from the Unity Editor.
    /// Mirrors <see cref="CustomerCenterPresenter"/> but exposes UnityEvents so flows can be wired
    /// directly through the Inspector.
    /// </summary>
    [AddComponentMenu("RevenueCat/Customer Center Behaviour")]
    public class CustomerCenterBehaviour : MonoBehaviour
    {
        [Serializable] public class StringEvent : UnityEvent<string> { }
        [Serializable] public class RefundRequestCompletedEvent : UnityEvent<string, RefundRequestStatus> { }
        [Serializable] public class ManagementOptionSelectedEvent : UnityEvent<CustomerCenterManagementOption, string> { }
        [Serializable] public class CustomActionSelectedEvent : UnityEvent<string, string> { }

        [Header("Events")]
        [Tooltip("Invoked after the Customer Center is successfully dismissed.")]
        public UnityEvent OnDismissed = new UnityEvent();

        [Tooltip("Invoked if an error prevents the Customer Center from showing.")]
        public UnityEvent OnError = new UnityEvent();

        [Tooltip("Invoked when the manage subscriptions screen is about to be shown.")]
        public UnityEvent OnShowingManageSubscriptions = new UnityEvent();

        [Tooltip("Invoked when a restore operation starts from the Customer Center.")]
        public UnityEvent OnRestoreStarted = new UnityEvent();

        [Tooltip("Invoked when a restore operation finishes successfully.")]
        public UnityEvent OnRestoreCompleted = new UnityEvent();

        [Tooltip("Invoked when a restore operation fails.")]
        public UnityEvent OnRestoreFailed = new UnityEvent();

        [Tooltip("Invoked when a feedback survey is completed. Provides the option identifier.")]
        public StringEvent OnFeedbackSurveyCompleted = new StringEvent();

        [Tooltip("Invoked when a refund request is initiated. Provides the product identifier.")]
        public StringEvent OnRefundRequestStarted = new StringEvent();

        [Tooltip("Invoked when a refund request finishes. Provides the product identifier and resulting status.")]
        public RefundRequestCompletedEvent OnRefundRequestCompleted = new RefundRequestCompletedEvent();

        [Tooltip("Invoked when a management option is selected. Provides the option and optional URL.")]
        public ManagementOptionSelectedEvent OnManagementOptionSelected = new ManagementOptionSelectedEvent();

        [Tooltip("Invoked when a custom action is selected. Provides the action identifier and optional purchase identifier.")]
        public CustomActionSelectedEvent OnCustomActionSelected = new CustomActionSelectedEvent();

        private SynchronizationContext unitySynchronizationContext;
        private bool isPresenting;
        private Exception lastPresentationException;
        private Purchases.CustomerInfo lastRestoreCustomerInfo;
        private Purchases.Error lastRestoreError;
        private string lastFeedbackSurveyOptionId;
        private string lastRefundRequestStartedProductId;
        private string lastRefundRequestCompletedProductId;
        private RefundRequestStatus lastRefundRequestStatus = RefundRequestStatus.Error;
        private CustomerCenterManagementOption lastManagementOption = CustomerCenterManagementOption.Unknown;
        private string lastManagementOptionUrl;
        private string lastCustomActionId;
        private string lastCustomActionPurchaseId;

        public bool IsPresenting => isPresenting;
        public Exception LastPresentationException => lastPresentationException;
        public Purchases.CustomerInfo LastRestoreCustomerInfo => lastRestoreCustomerInfo;
        public Purchases.Error LastRestoreError => lastRestoreError;
        public string LastFeedbackSurveyOptionId => lastFeedbackSurveyOptionId;
        public string LastRefundRequestStartedProductId => lastRefundRequestStartedProductId;
        public string LastRefundRequestCompletedProductId => lastRefundRequestCompletedProductId;
        public RefundRequestStatus LastRefundRequestStatus => lastRefundRequestStatus;
        public CustomerCenterManagementOption LastManagementOption => lastManagementOption;
        public string LastManagementOptionUrl => lastManagementOptionUrl;
        public string LastCustomActionId => lastCustomActionId;
        public string LastCustomActionPurchaseId => lastCustomActionPurchaseId;

        private void Awake()
        {
            unitySynchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        /// <summary>
        /// Presents the Customer Center. Can be wired to Unity UI (e.g. buttons) or invoked programmatically.
        /// </summary>
        public async void PresentCustomerCenter()
        {
            if (isPresenting)
            {
                Debug.LogWarning("[RevenueCatUI] Customer Center is already being presented.");
                return;
            }

            isPresenting = true;
            lastPresentationException = null;
            ResetLastEventData();

            try
            {
                var callbacks = CreateCallbacks();
                await CustomerCenterPresenter.Present(callbacks);
                InvokeUnityEvent(OnDismissed, nameof(OnDismissed));
            }
            catch (Exception e)
            {
                lastPresentationException = e;
                Debug.LogError($"[RevenueCatUI] Exception in CustomerCenterBehaviour: {e.Message}");
                InvokeUnityEvent(OnError, nameof(OnError));
            }
            finally
            {
                isPresenting = false;
            }
        }

        private CustomerCenterCallbacks CreateCallbacks()
        {
            return new CustomerCenterCallbacks
            {
                OnFeedbackSurveyCompleted = args =>
                {
                    lastFeedbackSurveyOptionId = args?.FeedbackSurveyOptionId;
                    DispatchToUnityThread(() =>
                        InvokeUnityEvent(OnFeedbackSurveyCompleted, lastFeedbackSurveyOptionId, nameof(OnFeedbackSurveyCompleted)));
                },
                OnShowingManageSubscriptions = () =>
                {
                    DispatchToUnityThread(() =>
                        InvokeUnityEvent(OnShowingManageSubscriptions, nameof(OnShowingManageSubscriptions)));
                },
                OnRestoreCompleted = args =>
                {
                    lastRestoreCustomerInfo = args?.CustomerInfo;
                    DispatchToUnityThread(() =>
                        InvokeUnityEvent(OnRestoreCompleted, nameof(OnRestoreCompleted)));
                },
                OnRestoreFailed = args =>
                {
                    lastRestoreError = args?.Error;
                    DispatchToUnityThread(() =>
                        InvokeUnityEvent(OnRestoreFailed, nameof(OnRestoreFailed)));
                },
                OnRestoreStarted = () =>
                {
                    DispatchToUnityThread(() =>
                        InvokeUnityEvent(OnRestoreStarted, nameof(OnRestoreStarted)));
                },
                OnRefundRequestStarted = args =>
                {
                    lastRefundRequestStartedProductId = args?.ProductIdentifier;
                    DispatchToUnityThread(() =>
                        InvokeUnityEvent(OnRefundRequestStarted, lastRefundRequestStartedProductId, nameof(OnRefundRequestStarted)));
                },
                OnRefundRequestCompleted = args =>
                {
                    lastRefundRequestCompletedProductId = args?.ProductIdentifier;
                    lastRefundRequestStatus = args?.RefundRequestStatus ?? RefundRequestStatus.Error;
                    DispatchToUnityThread(() =>
                        InvokeUnityEvent(OnRefundRequestCompleted, lastRefundRequestCompletedProductId, lastRefundRequestStatus, nameof(OnRefundRequestCompleted)));
                },
                OnManagementOptionSelected = args =>
                {
                    lastManagementOption = args?.Option ?? CustomerCenterManagementOption.Unknown;
                    lastManagementOptionUrl = args?.Url;
                    DispatchToUnityThread(() =>
                        InvokeUnityEvent(OnManagementOptionSelected, lastManagementOption, lastManagementOptionUrl, nameof(OnManagementOptionSelected)));
                },
                OnCustomActionSelected = args =>
                {
                    lastCustomActionId = args?.ActionId;
                    lastCustomActionPurchaseId = args?.PurchaseIdentifier;
                    DispatchToUnityThread(() =>
                        InvokeUnityEvent(OnCustomActionSelected, lastCustomActionId, lastCustomActionPurchaseId, nameof(OnCustomActionSelected)));
                }
            };
        }

        private void DispatchToUnityThread(Action action)
        {
            if (unitySynchronizationContext == null)
            {
                action?.Invoke();
                return;
            }

            if (SynchronizationContext.Current == unitySynchronizationContext)
            {
                action?.Invoke();
            }
            else
            {
                unitySynchronizationContext.Post(_ => action?.Invoke(), null);
            }
        }

        private void InvokeUnityEvent(UnityEvent unityEvent, string eventName)
        {
            if (unityEvent == null)
            {
                return;
            }

            if (unityEvent.GetPersistentEventCount() == 0)
            {
                Debug.Log($"[RevenueCatUI] Customer Center {eventName} event has no listeners.");
            }

            unityEvent.Invoke();
        }

        private void InvokeUnityEvent(StringEvent unityEvent, string argument, string eventName)
        {
            if (unityEvent == null)
            {
                return;
            }

            if (unityEvent.GetPersistentEventCount() == 0)
            {
                Debug.Log($"[RevenueCatUI] Customer Center {eventName} event has no listeners.");
            }

            unityEvent.Invoke(argument);
        }

        private void InvokeUnityEvent(RefundRequestCompletedEvent unityEvent, string productId, RefundRequestStatus status, string eventName)
        {
            if (unityEvent == null)
            {
                return;
            }

            if (unityEvent.GetPersistentEventCount() == 0)
            {
                Debug.Log($"[RevenueCatUI] Customer Center {eventName} event has no listeners.");
            }

            unityEvent.Invoke(productId, status);
        }

        private void InvokeUnityEvent(ManagementOptionSelectedEvent unityEvent, CustomerCenterManagementOption option, string url, string eventName)
        {
            if (unityEvent == null)
            {
                return;
            }

            if (unityEvent.GetPersistentEventCount() == 0)
            {
                Debug.Log($"[RevenueCatUI] Customer Center {eventName} event has no listeners.");
            }

            unityEvent.Invoke(option, url);
        }

        private void InvokeUnityEvent(CustomActionSelectedEvent unityEvent, string actionId, string purchaseIdentifier, string eventName)
        {
            if (unityEvent == null)
            {
                return;
            }

            if (unityEvent.GetPersistentEventCount() == 0)
            {
                Debug.Log($"[RevenueCatUI] Customer Center {eventName} event has no listeners.");
            }

            unityEvent.Invoke(actionId, purchaseIdentifier);
        }

        private void ResetLastEventData()
        {
            lastRestoreCustomerInfo = null;
            lastRestoreError = null;
            lastFeedbackSurveyOptionId = null;
            lastRefundRequestStartedProductId = null;
            lastRefundRequestCompletedProductId = null;
            lastRefundRequestStatus = RefundRequestStatus.Error;
            lastManagementOption = CustomerCenterManagementOption.Unknown;
            lastManagementOptionUrl = null;
            lastCustomActionId = null;
            lastCustomActionPurchaseId = null;
        }
    }
}

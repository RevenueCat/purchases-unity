using System.Threading.Tasks;
using RevenueCatUI;
using UnityEngine;

namespace DefaultNamespace
{
    public class CustomerCenterPresenterAPITests : MonoBehaviour
    {
        private bool _presentationCompleted;
        private bool _restoreStarted;
        private bool _restoreCompleted;
        private bool _restoreFailed;
        private bool _manageSubscriptionsShown;
        private bool _refundRequestStarted;
        private bool _refundRequestCompletedSuccessfully;
        private string _refundRequestProductIdentifier;
        private string _managementOption;
        private string _managementUrl;
        private string _customActionId;
        private string _customActionPurchaseIdentifier;
        private string _feedbackOptionId;
        private Purchases.CustomerInfo _customerInfo;
        private Purchases.Error _restoreError;

        private void Start()
        {
            CustomerCenterCallbacks callbacks = new CustomerCenterCallbacks
            {
                OnRestoreStarted = () => { _restoreStarted = true; },
                OnRestoreCompleted = args =>
                {
                    _restoreCompleted = true;
                    _customerInfo = args.CustomerInfo;
                },
                OnRestoreFailed = args =>
                {
                    _restoreFailed = true;
                    _restoreError = args.Error;
                },
                OnShowingManageSubscriptions = () => { _manageSubscriptionsShown = true; },
                OnRefundRequestStarted = args =>
                {
                    _refundRequestStarted = true;
                    _refundRequestProductIdentifier = args.ProductIdentifier;
                },
                OnRefundRequestCompleted = args =>
                {
                    _refundRequestProductIdentifier = args.ProductIdentifier;
                    if (args.RefundRequestStatus == RefundRequestStatus.Success)
                    {
                        _refundRequestCompletedSuccessfully = true;
                    }
                },
                OnManagementOptionSelected = args =>
                {
                    _managementOption = args.Option;
                    _managementUrl = args.Url;
                },
                OnCustomActionSelected = args =>
                {
                    _customActionId = args.ActionId;
                    _customActionPurchaseIdentifier = args.PurchaseIdentifier;
                },
                OnFeedbackSurveyCompleted = args => { _feedbackOptionId = args.FeedbackSurveyOptionId; }
            };

            Task defaultPresentationTask = CustomerCenterPresenter.Present();
            Task callbacksPresentationTask = CustomerCenterPresenter.Present(callbacks);

            defaultPresentationTask.ContinueWith(_ => { _presentationCompleted = true; });
            callbacksPresentationTask.ContinueWith(_ => { _presentationCompleted = true; });

            string cancelOption = CustomerCenterManagementOption.Cancel;
            string refundSuccess = RefundRequestStatus.Success;
            string refundUserCancelled = RefundRequestStatus.UserCancelled;
        }
    }
}


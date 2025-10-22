using System;

namespace RevenueCatUI
{
    public static class RefundRequestStatus
    {
        public const string Success = "Success";
        public const string UserCancelled = "UserCancelled";
        public const string Error = "Error";
    }

    public static class CustomerCenterManagementOption
    {
        public const string Cancel = "Cancel";
        public const string CustomUrl = "CustomUrl";
        public const string MissingPurchase = "MissingPurchase";
        public const string RefundRequest = "RefundRequest";
        public const string ChangePlans = "ChangePlans";
        public const string Unknown = "Unknown";
    }

    public sealed class FeedbackSurveyCompletedEventArgs
    {
        public string FeedbackSurveyOptionId { get; }

        internal FeedbackSurveyCompletedEventArgs(string feedbackSurveyOptionId)
        {
            FeedbackSurveyOptionId = feedbackSurveyOptionId;
        }
    }

    public sealed class RestoreCompletedEventArgs
    {
        public Purchases.CustomerInfo CustomerInfo { get; }

        internal RestoreCompletedEventArgs(Purchases.CustomerInfo customerInfo)
        {
            CustomerInfo = customerInfo;
        }
    }

    public sealed class RestoreFailedEventArgs
    {
        public Purchases.Error Error { get; }

        internal RestoreFailedEventArgs(Purchases.Error error)
        {
            Error = error;
        }
    }

    public sealed class RefundRequestStartedEventArgs
    {
        public string ProductIdentifier { get; }

        internal RefundRequestStartedEventArgs(string productIdentifier)
        {
            ProductIdentifier = productIdentifier;
        }
    }

    public sealed class RefundRequestCompletedEventArgs
    {
        public string ProductIdentifier { get; }
        public string RefundRequestStatus { get; }

        internal RefundRequestCompletedEventArgs(string productIdentifier, string refundRequestStatus)
        {
            ProductIdentifier = productIdentifier;
            RefundRequestStatus = refundRequestStatus;
        }
    }

    public sealed class ManagementOptionSelectedEventArgs
    {
        public string Option { get; }
        public string Url { get; }

        internal ManagementOptionSelectedEventArgs(string option, string url = null)
        {
            Option = option;
            Url = url;
        }
    }

    public sealed class CustomActionSelectedEventArgs
    {
        public string ActionId { get; }
        public string PurchaseIdentifier { get; }

        internal CustomActionSelectedEventArgs(string actionId, string purchaseIdentifier = null)
        {
            ActionId = actionId;
            PurchaseIdentifier = purchaseIdentifier;
        }
    }

    /// <summary>
    /// Callbacks for Customer Center events.
    /// 
    /// IMPORTANT: All callbacks execute on a background thread, not Unity's main thread.
    /// You CANNOT directly call Unity APIs (GameObject, Transform, UI components, etc.) from these callbacks.
    /// </summary>
    public sealed class CustomerCenterCallbacks
    {
        internal static readonly CustomerCenterCallbacks None = new CustomerCenterCallbacks();

        /// <summary>
        /// Called when a feedback survey is completed with the selected option ID.
        /// </summary>
        public Action<FeedbackSurveyCompletedEventArgs> OnFeedbackSurveyCompleted { get; set; }

        /// <summary>
        /// Called when the manage subscriptions section is being shown.
        /// </summary>
        public Action OnShowingManageSubscriptions { get; set; }

        /// <summary>
        /// Called when a restore operation is completed successfully.
        /// </summary>
        public Action<RestoreCompletedEventArgs> OnRestoreCompleted { get; set; }

        /// <summary>
        /// Called when a restore operation fails.
        /// </summary>
        public Action<RestoreFailedEventArgs> OnRestoreFailed { get; set; }

        /// <summary>
        /// Called when a restore operation starts.
        /// </summary>
        public Action OnRestoreStarted { get; set; }

        /// <summary>
        /// Called when a refund request starts with the product identifier.
        /// iOS only - This callback will never be called on Android as refund requests are not supported on that platform.
        /// </summary>
        public Action<RefundRequestStartedEventArgs> OnRefundRequestStarted { get; set; }

        /// <summary>
        /// Called when a refund request completes with status information.
        /// iOS only - This callback will never be called on Android as refund requests are not supported on that platform.
        /// </summary>
        public Action<RefundRequestCompletedEventArgs> OnRefundRequestCompleted { get; set; }

        /// <summary>
        /// Called when a customer center management option is selected.
        /// For 'CustomUrl' options, the Url parameter will contain the URL.
        /// For all other options, the Url parameter will be null.
        /// </summary>
        public Action<ManagementOptionSelectedEventArgs> OnManagementOptionSelected { get; set; }

        /// <summary>
        /// Called when a custom action is selected in the customer center.
        /// </summary>
        public Action<CustomActionSelectedEventArgs> OnCustomActionSelected { get; set; }
    }
}

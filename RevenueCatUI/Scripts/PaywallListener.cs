using System;

namespace RevenueCatUI
{
    /// <summary>
    /// Optional listener for paywall lifecycle events during a single presentation.
    /// Assign only the callbacks you need; unassigned callbacks are ignored.
    ///
    /// All callbacks are invoked on the Unity main thread while the paywall is presented.
    /// Events fire before the Task returned by <see cref="PaywallsPresenter.Present"/> or
    /// <see cref="PaywallsPresenter.PresentIfNeeded"/> completes.
    /// Callbacks never fire in the Unity Editor or on unsupported platforms.
    ///
    /// When combined with <see cref="PurchaseLogic"/> (purchasesAreCompletedBy MY_APP),
    /// most listener events will not be sent. Observe purchase outcomes through the
    /// PurchaseLogic handlers instead.
    /// </summary>
    public class PaywallListener
    {
        /// <summary>
        /// Invoked when a purchase is started for the given package.
        /// </summary>
        public Action<Purchases.Package> OnPurchaseStarted { get; set; }

        /// <summary>
        /// Invoked when a purchase completes successfully.
        /// The StoreTransaction may be null in rare cases on iOS.
        /// </summary>
        public Action<Purchases.CustomerInfo, Purchases.StoreTransaction> OnPurchaseCompleted { get; set; }

        /// <summary>
        /// Invoked when a purchase fails.
        /// </summary>
        public Action<Purchases.Error> OnPurchaseError { get; set; }

        /// <summary>
        /// Invoked when the user cancels a purchase.
        /// </summary>
        public Action OnPurchaseCancelled { get; set; }

        /// <summary>
        /// Invoked when a restore is started.
        /// </summary>
        public Action OnRestoreStarted { get; set; }

        /// <summary>
        /// Invoked when a restore completes successfully.
        /// </summary>
        public Action<Purchases.CustomerInfo> OnRestoreCompleted { get; set; }

        /// <summary>
        /// Invoked when a restore fails.
        /// </summary>
        public Action<Purchases.Error> OnRestoreError { get; set; }
    }
}

using System;
using System.Threading.Tasks;

namespace RevenueCatUI
{
    /// <summary>
    /// Custom purchase and restore handlers for apps that manage their own
    /// purchase flow (purchasesAreCompletedBy set to MY_APP).
    /// When provided to paywall presentation, the paywall delegates purchase and restore
    /// operations to these handlers instead of using RevenueCat's built-in purchase flow.
    /// </summary>
    public class PurchaseLogic
    {
        /// <summary>
        /// Delegate for performing a custom purchase.
        /// </summary>
        /// <param name="purchaseParams">The parameters for the purchase, including the package to purchase.</param>
        /// <returns>A task that resolves to the result of the purchase operation.</returns>
        public delegate Task<PurchaseLogicResult> PerformPurchaseHandler(PurchaseLogicPurchaseParams purchaseParams);

        /// <summary>
        /// Delegate for performing a custom restore.
        /// </summary>
        /// <returns>A task that resolves to the result of the restore operation.</returns>
        public delegate Task<PurchaseLogicResult> PerformRestoreHandler();

        /// <summary>
        /// The handler called when the user initiates a purchase from the paywall.
        /// </summary>
        public PerformPurchaseHandler PerformPurchase { get; }

        /// <summary>
        /// The handler called when the user initiates a restore from the paywall.
        /// </summary>
        public PerformRestoreHandler PerformRestore { get; }

        /// <summary>
        /// Creates a new PurchaseLogic instance with custom purchase and restore handlers.
        /// </summary>
        /// <param name="performPurchase">Handler called when the user initiates a purchase.</param>
        /// <param name="performRestore">Handler called when the user initiates a restore.</param>
        public PurchaseLogic(PerformPurchaseHandler performPurchase, PerformRestoreHandler performRestore)
        {
            PerformPurchase = performPurchase ?? throw new ArgumentNullException(nameof(performPurchase));
            PerformRestore = performRestore ?? throw new ArgumentNullException(nameof(performRestore));
        }
    }
}

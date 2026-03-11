namespace RevenueCatUI
{
    /// <summary>
    /// Parameters provided to <see cref="PurchaseLogic.PerformPurchaseHandler"/> when a paywall
    /// initiates a purchase. Contains the package to purchase along with any additional
    /// information needed for the purchase flow.
    /// </summary>
    public class PurchaseLogicPurchaseParams
    {
        /// <summary>
        /// The package the user wants to purchase.
        /// </summary>
        public Purchases.Package PackageToPurchase { get; }

        internal PurchaseLogicPurchaseParams(Purchases.Package packageToPurchase)
        {
            PackageToPurchase = packageToPurchase;
        }
    }
}

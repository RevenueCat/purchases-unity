namespace RevenueCatUI
{
    /// <summary>
    /// The result of a purchase or restore operation performed by custom app-based logic.
    /// Used when purchasesAreCompletedBy is set to MY_APP.
    /// </summary>
    public enum PurchaseLogicResult
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The user cancelled the operation.
        /// </summary>
        Cancellation,

        /// <summary>
        /// An error occurred during the operation.
        /// </summary>
        Error
    }

    internal static class PurchaseLogicResultExtensions
    {
        internal static string ToNativeString(this PurchaseLogicResult result)
        {
            return result switch
            {
                PurchaseLogicResult.Success => "SUCCESS",
                PurchaseLogicResult.Cancellation => "CANCELLATION",
                PurchaseLogicResult.Error => "ERROR",
                _ => "ERROR"
            };
        }
    }
}

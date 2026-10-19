public partial class Purchases
{
    /// Enum indicating the billing plan type of an installment subscription.
    /// iOS only.
    public enum BillingPlanType
    {
        /// Upfront billing plan, where the user pays in full when purchasing the product.
        UP_FRONT = 0,

        /// Monthly billing plan, where the user pays in monthly installments.
        MONTHLY = 1,

        /// Unknown billing plan type.
        UNKNOWN = 2
    }
}

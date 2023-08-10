public partial class Purchases
{
    public enum ProductCategory
    {
        /// A type of product for non-subscription.
        NON_SUBSCRIPTION = 0,

        /// A type of product for subscriptions.
        SUBSCRIPTION = 1,

        /// The user is eligible for a free trial or intro pricing for this product.
        UNKNOWN = 2
    }
}
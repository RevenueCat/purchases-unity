public partial class Purchases
{
    public enum ProductCategory
    {
        /// A type of product for non-subscription.
        ProductCategoryNonSubscription = 0,

        /// A type of product for subscriptions.
        ProductCategorySubscription = 1,

        /// The user is eligible for a free trial or intro pricing for this product.
        ProductCategoryUnknown = 2
    }
}
namespace RevenueCat
{
    public enum ProductCategory
    {
        /// <summary>
        /// A type of product for non-subscription.
        /// </summary>
        NON_SUBSCRIPTION = 0,
        /// <summary>
        /// A type of product for subscriptions.
        /// </summary>
        SUBSCRIPTION = 1,
        /// <summary>
        /// The user is eligible for a free trial or intro pricing for this product.
        /// </summary>
        UNKNOWN = 2
    }
}
public partial class Purchases
{
    /// <summary>
    /// Enum for billing features.
    /// Currently, these are only relevant for Google Play Android users:
    /// https://developer.android.com/reference/com/android/billingclient/api/BillingClient.FeatureType
    /// </summary>
    public enum BillingFeature
    {
        /// <summary>
        /// Purchase/query for subscriptions
        /// </summary>
        Subscriptions = 0,
        
        /// <summary>
        /// Subscriptions update/replace
        /// </summary>
        SubscriptionsUpdate = 1,

        /// <summary>
        /// Purchase/query for in-app items on VR
        /// </summary>
        InAppItemsOnVR = 2,

        /// <summary>
        /// Purchase/query for subscriptions on VR
        /// </summary>
        SubscriptionsOnVR = 3,
        
        /// <summary>
        /// Launch a price change confirmation flow
        /// </summary>
        PriceChangeConfirmation = 4
    }
}
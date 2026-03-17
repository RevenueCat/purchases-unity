public partial class Purchases
{
    /// <summary>
    /// Parameters for tracking a custom paywall impression event.
    /// </summary>
    public class CustomPaywallImpressionParams
    {
        /// <summary>
        /// An optional identifier for the custom paywall being shown.
        /// </summary>
        public string PaywallId { get; private set; }

        /// <summary>
        /// An optional identifier for the offering associated with the custom paywall.
        /// If not provided, the SDK will use the current offering identifier from the cache.
        /// </summary>
        public string OfferingId { get; private set; }

        /// <summary>
        /// Creates parameters for a custom paywall impression.
        /// </summary>
        /// <param name="paywallId">An optional identifier for the custom paywall being shown.</param>
        /// <param name="offeringId">An optional identifier for the offering associated with the custom paywall.
        /// If not provided, the SDK will use the current offering identifier from the cache.</param>
        public CustomPaywallImpressionParams(string paywallId = null, string offeringId = null)
        {
            PaywallId = paywallId;
            OfferingId = offeringId;
        }
    }
}

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
        /// Creates parameters for a custom paywall impression.
        /// </summary>
        /// <param name="paywallId">An optional identifier for the custom paywall being shown.</param>
        public CustomPaywallImpressionParams(string paywallId = null)
        {
            PaywallId = paywallId;
        }
    }
}

using System;

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
        /// Deprecated: use the Offering object instead so the SDK can derive placement and targeting context.
        /// </summary>
        public string OfferingId { get; private set; }

        /// <summary>
        /// The offering associated with the custom paywall, if provided.
        /// </summary>
        public Offering Offering { get; private set; }

        internal PresentedOfferingContext PresentedOfferingContext
        {
            get
            {
                return Offering != null && Offering.AvailablePackages != null && Offering.AvailablePackages.Count > 0
                    ? Offering.AvailablePackages[0].PresentedOfferingContext
                    : null;
            }
        }

        /// <summary>
        /// Creates parameters for a custom paywall impression.
        /// </summary>
        public CustomPaywallImpressionParams()
        {
            SetValues(null, null, null);
        }

        /// <summary>
        /// Creates parameters for a custom paywall impression.
        /// </summary>
        /// <param name="paywallId">An optional identifier for the custom paywall being shown.</param>
        public CustomPaywallImpressionParams(string paywallId)
        {
            SetValues(paywallId, null, null);
        }

        /// <summary>
        /// Creates parameters for a custom paywall impression from an offering.
        /// The SDK derives the offering identifier and presented offering context from this offering.
        /// </summary>
        /// <param name="offering">The offering associated with the custom paywall.</param>
        public CustomPaywallImpressionParams(Offering offering)
        {
            SetValues(null, null, offering);
        }

        /// <summary>
        /// Creates parameters for a custom paywall impression from an offering.
        /// The SDK derives the offering identifier and presented offering context from this offering.
        /// </summary>
        /// <param name="paywallId">An optional identifier for the custom paywall being shown.</param>
        /// <param name="offering">The offering associated with the custom paywall.</param>
        public CustomPaywallImpressionParams(string paywallId, Offering offering)
        {
            SetValues(paywallId, null, offering);
        }

        /// <summary>
        /// Creates parameters for a custom paywall impression with an offering identifier string.
        /// </summary>
        /// <param name="paywallId">An optional identifier for the custom paywall being shown.</param>
        /// <param name="offeringId">An optional identifier for the offering associated with the custom paywall.
        /// Deprecated: pass an Offering instead so the SDK can derive placement and targeting context.</param>
        [Obsolete("Pass an Offering instead.", false)]
        public CustomPaywallImpressionParams(string paywallId = null, string offeringId = null)
        {
            SetValues(paywallId, offeringId, null);
        }

        private CustomPaywallImpressionParams(string paywallId, string offeringId, Offering offering)
        {
            SetValues(paywallId, offeringId, offering);
        }

        private void SetValues(string paywallId, string offeringId, Offering offering)
        {
            PaywallId = paywallId;
            Offering = offering;
            var resolvedOfferingId = offering != null ? offering.Identifier : offeringId;
            OfferingId = resolvedOfferingId;
        }
    }
}

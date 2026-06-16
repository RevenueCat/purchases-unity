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
        /// If neither this nor an Offering is provided, no offering data is sent and the
        /// native SDK handles its default fallback behavior.
        /// </summary>
        public string OfferingId { get; private set; }

        private PresentedOfferingContext _presentedOfferingContext;

        internal string PresentedOfferingContextJson
        {
            get { return _presentedOfferingContext?.ToJsonString(); }
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
        /// Creates parameters for a custom paywall impression.
        /// </summary>
        /// <param name="paywallId">An optional identifier for the custom paywall being shown.</param>
        /// <param name="offeringId">An optional identifier for the offering associated with the custom paywall.
        /// If neither this nor an Offering is provided, no offering data is sent and the native SDK
        /// handles its default fallback behavior.
        /// Deprecated: use FromOffering instead so the SDK can derive placement and targeting context.</param>
        [Obsolete("Use FromOffering instead.", false)]
        public CustomPaywallImpressionParams(string paywallId = null, string offeringId = null)
        {
            SetValues(paywallId, offeringId, null);
        }

        /// <summary>
        /// Creates parameters for a custom paywall impression from an Offering.
        /// </summary>
        /// <param name="offering">An optional offering associated with the custom paywall. When provided,
        /// the SDK derives the offering identifier and presented offering context from this offering's first
        /// available package.</param>
        /// <param name="paywallId">An optional identifier for the custom paywall being shown.</param>
        public static CustomPaywallImpressionParams FromOffering(Offering offering, string paywallId = null)
        {
            return new CustomPaywallImpressionParams(paywallId, null, offering);
        }

        private CustomPaywallImpressionParams(string paywallId, string offeringId, Offering offering)
        {
            SetValues(paywallId, offeringId, offering);
        }

        private void SetValues(string paywallId, string offeringId, Offering offering)
        {
            PaywallId = paywallId;
            var resolvedOfferingId = offering != null ? offering.Identifier : offeringId;
            OfferingId = resolvedOfferingId;
            _presentedOfferingContext = GetPresentedOfferingContext(offering);
        }

        private static PresentedOfferingContext GetPresentedOfferingContext(Offering offering)
        {
            return offering != null && offering.AvailablePackages != null && offering.AvailablePackages.Count > 0
                ? offering.AvailablePackages[0].PresentedOfferingContext
                : null;
        }
    }
}

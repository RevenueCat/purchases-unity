using System;

namespace RevenueCatUI
{
    internal abstract class OfferingSelection
    {
        internal sealed class OfferingType : OfferingSelection
        {
            internal Purchases.Offering Offering { get; }

            internal OfferingType(Purchases.Offering offering)
            {
                Offering = offering;
            }

            internal override Purchases.Offering GetOffering() => Offering;
            internal override string GetOfferingIdentifier() => Offering.Identifier;
            internal override Purchases.PresentedOfferingContext GetPresentedOfferingContext() =>
                Offering.AvailablePackages != null && Offering.AvailablePackages.Count > 0
                    ? Offering.AvailablePackages[0].PresentedOfferingContext
                    : null;
        }

        internal sealed class IdentifierType : OfferingSelection
        {
            internal string OfferingId { get; }

            internal IdentifierType(string offeringId)
            {
                OfferingId = offeringId;
                _presentedOfferingContext = new Purchases.PresentedOfferingContext(offeringId);
            }

            private Purchases.PresentedOfferingContext _presentedOfferingContext;

            internal override Purchases.Offering GetOffering() => null;
            internal override string GetOfferingIdentifier() => OfferingId;
            internal override Purchases.PresentedOfferingContext GetPresentedOfferingContext() => _presentedOfferingContext;
        }

        internal abstract Purchases.Offering GetOffering();
        internal abstract string GetOfferingIdentifier();
        internal abstract Purchases.PresentedOfferingContext GetPresentedOfferingContext();
    }

    /// <summary>
    /// Options for configuring paywall presentation.
    /// </summary>
    [Serializable]
    public class PaywallOptions
    {
        internal readonly OfferingSelection _offeringSelection;

        internal bool DisplayCloseButton { get; }
        internal string OfferingIdentifier => _offeringSelection?.GetOfferingIdentifier();
        internal Purchases.PresentedOfferingContext PresentedOfferingContext => _offeringSelection?.GetPresentedOfferingContext();
        internal PaywallPresentationConfiguration PresentationConfiguration { get; }

        /// <summary>
        /// Creates a new PaywallOptions instance.
        /// Will present the current offering.
        /// </summary>
        /// <param name="displayCloseButton">Whether to display a close button. Only applicable for original template paywalls, ignored for V2 Paywalls.</param>
        /// <param name="presentationConfiguration">Optional configuration for how the paywall should be presented on each platform.</param>
        public PaywallOptions(bool displayCloseButton = false, PaywallPresentationConfiguration presentationConfiguration = null)
        {
            _offeringSelection = null;
            DisplayCloseButton = displayCloseButton;
            PresentationConfiguration = presentationConfiguration;
        }

        /// <summary>
        /// Creates a new PaywallOptions instance from an Offering object.
        /// </summary>
        /// <param name="offering">The offering to present. If null, the current offering will be used.</param>
        /// <param name="displayCloseButton">Whether to display a close button. Only applicable for original template paywalls, ignored for V2 Paywalls.</param>
        /// <param name="presentationConfiguration">Optional configuration for how the paywall should be presented on each platform.</param>
        public PaywallOptions(Purchases.Offering offering, bool displayCloseButton = false, PaywallPresentationConfiguration presentationConfiguration = null)
        {
            _offeringSelection = offering != null ? new OfferingSelection.OfferingType(offering) : null;
            DisplayCloseButton = displayCloseButton;
            PresentationConfiguration = presentationConfiguration;
        }

        internal PaywallOptions(string offeringIdentifier, bool displayCloseButton = false, PaywallPresentationConfiguration presentationConfiguration = null)
        {
            _offeringSelection = !string.IsNullOrEmpty(offeringIdentifier) ? new OfferingSelection.IdentifierType(offeringIdentifier) : null;
            DisplayCloseButton = displayCloseButton;
            PresentationConfiguration = presentationConfiguration;
        }
    }
} 

using System;

namespace RevenueCatUI
{
    /// <summary>
    /// Options for configuring paywall presentation.
    /// </summary>
    [Serializable]
    public class PaywallOptions
    {
        /// <summary>
        /// The offering to present.
        /// If not provided, the current offering will be used.
        /// </summary>
        public Purchases.Offering Offering { get; set; }

        /// <summary>
        /// Whether to display a close button on the paywall.
        /// Only applicable for original template paywalls, ignored for V2 Paywalls.
        /// </summary>
        public bool DisplayCloseButton { get; set; } = false;

        internal string OfferingIdentifier => Offering?.Identifier;
        
        internal Purchases.PresentedOfferingContext PresentedOfferingContext => 
            Offering?.AvailablePackages != null && Offering.AvailablePackages.Count > 0 
                ? Offering.AvailablePackages[0].PresentedOfferingContext 
                : null;

        /// <summary>
        /// Creates a new PaywallOptions instance.
        /// Will present the current offering.
        /// </summary>
        /// <param name="displayCloseButton">Whether to display a close button. Only applicable for original template paywalls, ignored for V2 Paywalls.</param>
        public PaywallOptions(bool displayCloseButton = false)
        {
            DisplayCloseButton = displayCloseButton;
        }

        /// <summary>
        /// Creates a new PaywallOptions instance from an Offering object.
        /// </summary>
        /// <param name="offering">The offering to present</param>
        /// <param name="displayCloseButton">Whether to display a close button. Only applicable for original template paywalls, ignored for V2 Paywalls.</param>
        public PaywallOptions(Purchases.Offering offering, bool displayCloseButton = false)
        {
            Offering = offering ?? throw new ArgumentNullException(nameof(offering));
            DisplayCloseButton = displayCloseButton;
        }
    }
} 

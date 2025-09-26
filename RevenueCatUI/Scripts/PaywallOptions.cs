using System;

namespace RevenueCat.UnityUI
{
    /// <summary>
    /// Options for configuring paywall presentation.
    /// </summary>
    [Serializable]
    public class PaywallOptions
    {
        /// <summary>
        /// The identifier of the offering to present.
        /// If not provided, the current offering will be used.
        /// </summary>
        public string OfferingIdentifier { get; set; }

        /// <summary>
        /// Whether to display a close button on the paywall.
        /// Only applicable for original template paywalls, ignored for V2 Paywalls.
        /// </summary>
        public bool DisplayCloseButton { get; set; } = false;

        /// <summary>
        /// Creates a new PaywallOptions instance.
        /// </summary>
        public PaywallOptions()
        {
        }

        /// <summary>
        /// Creates a new PaywallOptions instance with the specified offering identifier.
        /// </summary>
        /// <param name="offeringIdentifier">The offering identifier to present</param>
        public PaywallOptions(string offeringIdentifier)
        {
            OfferingIdentifier = offeringIdentifier;
        }

        /// <summary>
        /// Creates a new PaywallOptions instance with the specified offering identifier and close button option.
        /// </summary>
        /// <param name="offeringIdentifier">The offering identifier to present</param>
        /// <param name="displayCloseButton">Whether to display a close button</param>
        public PaywallOptions(string offeringIdentifier, bool displayCloseButton)
        {
            OfferingIdentifier = offeringIdentifier;
            DisplayCloseButton = displayCloseButton;
        }
    }
} 

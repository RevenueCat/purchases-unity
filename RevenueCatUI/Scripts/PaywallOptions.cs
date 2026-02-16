using System;
using System.Collections.Generic;

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
        internal Dictionary<string, CustomVariableValue> CustomVariables { get; }
        internal string OfferingIdentifier => _offeringSelection?.GetOfferingIdentifier();
        internal Purchases.PresentedOfferingContext PresentedOfferingContext => _offeringSelection?.GetPresentedOfferingContext();

        /// <summary>
        /// Creates a new PaywallOptions instance.
        /// Will present the current offering.
        /// </summary>
        /// <param name="displayCloseButton">Whether to display a close button. Only applicable for original template paywalls, ignored for V2 Paywalls.</param>
        /// <param name="customVariables">Custom variables for text substitution in paywalls using {{ custom.variable_name }} syntax. Only available for V2 Paywalls.</param>
        public PaywallOptions(bool displayCloseButton = false, Dictionary<string, CustomVariableValue> customVariables = null)
        {
            _offeringSelection = null;
            DisplayCloseButton = displayCloseButton;
            CustomVariables = customVariables;
        }

        /// <summary>
        /// Creates a new PaywallOptions instance from an Offering object.
        /// </summary>
        /// <param name="offering">The offering to present. If null, the current offering will be used.</param>
        /// <param name="displayCloseButton">Whether to display a close button. Only applicable for original template paywalls, ignored for V2 Paywalls.</param>
        /// <param name="customVariables">Custom variables for text substitution in paywalls using {{ custom.variable_name }} syntax. Only available for V2 Paywalls.</param>
        public PaywallOptions(Purchases.Offering offering, bool displayCloseButton = false, Dictionary<string, CustomVariableValue> customVariables = null)
        {
            _offeringSelection = offering != null ? new OfferingSelection.OfferingType(offering) : null;
            DisplayCloseButton = displayCloseButton;
            CustomVariables = customVariables;
        }

        internal PaywallOptions(string offeringIdentifier, bool displayCloseButton = false, Dictionary<string, CustomVariableValue> customVariables = null)
        {
            _offeringSelection = !string.IsNullOrEmpty(offeringIdentifier) ? new OfferingSelection.IdentifierType(offeringIdentifier) : null;
            DisplayCloseButton = displayCloseButton;
            CustomVariables = customVariables;
        }

        /// <summary>
        /// Serializes custom variables to JSON string for native layer communication.
        /// </summary>
        internal string CustomVariablesToJsonString()
        {
            if (CustomVariables == null || CustomVariables.Count == 0) return null;
            var stringDict = CustomVariables.ToStringDictionary();
            var dict = new SimpleJSON.JSONObject();
            foreach (var kvp in stringDict)
            {
                dict[kvp.Key] = kvp.Value;
            }
            return dict.ToString();
        }
    }
} 

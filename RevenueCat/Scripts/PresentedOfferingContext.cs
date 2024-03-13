using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// Stores context about the offering a package was presented from.
    /// </summary>
    public class PresentedOfferingContext
    {
        public readonly string OfferingIdentifier;
        [CanBeNull] public readonly string PlacementIdentifier;
        [CanBeNull] public readonly PresentedOfferingTargetingContext TargetingContext;

        public PresentedOfferingContext(JSONNode response)
        {
            OfferingIdentifier = response["offeringIdentifier"];
            PlacementIdentifier = response["placementIdentifier"];
            TargetingContext = new PresentedOfferingTargetingContext(response["targetingContext"]);
        }

        public override string ToString()
        {
            return $"{nameof(OfferingIdentifier)}: {OfferingIdentifier}\n" +
                   $"{nameof(PlacementIdentifier)}: {PlacementIdentifier}\n" + 
                   $"{nameof(TargetingContext)}: {TargetingContext}";
        }
    }

    /// <summary>
    /// Stores revision information about the targeting rule.
    /// </summary>
    public class PresentedOfferingTargetingContext
    {
        public readonly int Revision;
        public readonly string RuleId;

        public PresentedOfferingTargetingContext(JSONNode response)
        {
            Revision = response["revision"];
            RuleId = response["ruleId"];
        }

        public override string ToString()
        {
            return $"{nameof(Revision)}: {Revision}\n" +
                   $"{nameof(RuleId)}: {RuleId}";
        }
    }
}
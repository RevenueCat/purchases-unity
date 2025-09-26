using JetBrains.Annotations;
using Newtonsoft.Json;

namespace RevenueCat
{
    /// <summary>
    /// Stores context about the offering a package was presented from.
    /// </summary>
    public class PresentedOfferingContext
    {
        [JsonProperty("offeringIdentifier")]
        public string OfferingIdentifier { get; }

        [CanBeNull]
        [JsonProperty("placementIdentifier")]
        public string PlacementIdentifier { get; }

        [CanBeNull]
        [JsonProperty("targetingContext")]
        public PresentedOfferingTargetingContext TargetingContext { get; }

        [JsonConstructor]
        internal PresentedOfferingContext(
            [JsonProperty("offeringIdentifier")] string offeringIdentifier,
            [JsonProperty("placementIdentifier")] string placementIdentifier,
            [JsonProperty("targetingContext")] PresentedOfferingTargetingContext targetingContext)
        {
            OfferingIdentifier = offeringIdentifier;
            PlacementIdentifier = placementIdentifier;
            TargetingContext = targetingContext;
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
        [JsonProperty("revision")]
        public int Revision { get; }

        [JsonProperty("ruleId")]
        public string RuleId { get; }

        [JsonConstructor]
        internal PresentedOfferingTargetingContext(
            [JsonProperty("revision")] int revision,
            [JsonProperty("ruleId")] string ruleId)
        {
            Revision = revision;
            RuleId = ruleId;
        }

        public override string ToString()
        {
            return $"{nameof(Revision)}: {Revision}\n" +
                   $"{nameof(RuleId)}: {RuleId}";
        }
    }
}
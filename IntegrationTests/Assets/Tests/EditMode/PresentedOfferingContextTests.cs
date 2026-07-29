using NUnit.Framework;
using RevenueCat.SimpleJSON;

namespace RevenueCat.Tests
{
    public class PresentedOfferingContextTests
    {
        [Test]
        public void JsonRoundTripPreservesAllContext()
        {
            var originalJson = JSONNode.Parse(
                "{\"offeringIdentifier\":\"default\",\"placementIdentifier\":\"onboarding\"," +
                "\"targetingContext\":{\"revision\":7,\"ruleId\":\"rule-id\"}}"
            );

            var context = new Purchases.PresentedOfferingContext(originalJson);
            var serializedContext = JSONNode.Parse(context.ToJsonString());

            Assert.That(serializedContext["offeringIdentifier"].Value, Is.EqualTo("default"));
            Assert.That(serializedContext["placementIdentifier"].Value, Is.EqualTo("onboarding"));
            Assert.That(serializedContext["targetingContext"]["revision"].AsInt, Is.EqualTo(7));
            Assert.That(serializedContext["targetingContext"]["ruleId"].Value, Is.EqualTo("rule-id"));
        }

        [Test]
        public void OfferingIdentifierConstructorOmitsOptionalContext()
        {
            var context = new Purchases.PresentedOfferingContext("default");
            var serializedContext = JSONNode.Parse(context.ToJsonString());

            Assert.That(serializedContext["offeringIdentifier"].Value, Is.EqualTo("default"));
            Assert.That(serializedContext.HasKey("placementIdentifier"), Is.False);
            Assert.That(serializedContext.HasKey("targetingContext"), Is.False);
        }
    }
}

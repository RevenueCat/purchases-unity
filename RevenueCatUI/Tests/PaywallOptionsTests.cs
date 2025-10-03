using NUnit.Framework;
using RevenueCat.UI;

namespace RevenueCat.Tests
{
    public class PaywallOptionsTests
    {
        [Test]
        public void PaywallOptions_DefaultConstructor_SetsDefaultValues()
        {
            var options = new PaywallOptions();
            
            Assert.That(options.OfferingIdentifier, Is.Null);
            Assert.That(options.DisplayCloseButton, Is.False);
        }

        [Test]
        public void PaywallOptions_ConstructorWithOfferingIdentifier_SetsValues()
        {
            const string offeringId = "premium_monthly";
            
            var options = new PaywallOptions(offeringId);
            
            Assert.That(options.OfferingIdentifier, Is.EqualTo(offeringId));
            Assert.That(options.DisplayCloseButton, Is.False);
        }

        [Test]
        public void PaywallOptions_ConstructorWithOfferingIdentifierAndCloseButton_SetsValues()
        {
            const string offeringId = "premium_yearly";
            const bool displayCloseButton = true;
            
            var options = new PaywallOptions(offeringId, displayCloseButton);
            
            Assert.That(options.OfferingIdentifier, Is.EqualTo(offeringId));
            Assert.That(options.DisplayCloseButton, Is.EqualTo(displayCloseButton));
        }

        [Test]
        public void PaywallOptions_Properties_CanBeModified()
        {
            var options = new PaywallOptions();
            
            options.OfferingIdentifier = "test_offering";
            options.DisplayCloseButton = true;
            
            Assert.That(options.OfferingIdentifier, Is.EqualTo("test_offering"));
            Assert.That(options.DisplayCloseButton, Is.True);
        }

        [Test]
        public void PaywallOptions_WithNullOfferingIdentifier_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new PaywallOptions(null));
            Assert.DoesNotThrow(() => new PaywallOptions(null, true));
            
            var options = new PaywallOptions();
            Assert.DoesNotThrow(() => options.OfferingIdentifier = null);
        }

        [Test]
        public void PaywallOptions_WithEmptyOfferingIdentifier_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new PaywallOptions(""));
            Assert.DoesNotThrow(() => new PaywallOptions("", false));
            
            var options = new PaywallOptions();
            Assert.DoesNotThrow(() => options.OfferingIdentifier = "");
        }
    }
}
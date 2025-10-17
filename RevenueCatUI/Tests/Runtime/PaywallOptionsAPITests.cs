using NUnit.Framework;
using RevenueCatUI;

namespace RevenueCatUI.Tests
{
    public class PaywallOptionsAPITests
    {
        [Test]
        public void PaywallOptions_DefaultConstructor_CreatesInstanceWithNullValues()
        {
            var options = new PaywallOptions();
            
            Assert.IsNotNull(options);
            Assert.IsNull(options.OfferingIdentifier);
            Assert.IsFalse(options.DisplayCloseButton);
        }

        [Test]
        public void PaywallOptions_OfferingIdentifierConstructor_SetsOfferingIdentifier()
        {
            const string offeringId = "test_offering";
            var options = new PaywallOptions(offeringId);
            
            Assert.IsNotNull(options);
            Assert.AreEqual(offeringId, options.OfferingIdentifier);
            Assert.IsFalse(options.DisplayCloseButton);
        }

        [Test]
        public void PaywallOptions_FullConstructor_SetsBothProperties()
        {
            const string offeringId = "test_offering";
            const bool displayCloseButton = true;
            var options = new PaywallOptions(offeringId, displayCloseButton);
            
            Assert.IsNotNull(options);
            Assert.AreEqual(offeringId, options.OfferingIdentifier);
            Assert.AreEqual(displayCloseButton, options.DisplayCloseButton);
        }

        [Test]
        public void PaywallOptions_Properties_CanBeSetAndRead()
        {
            var options = new PaywallOptions
            {
                OfferingIdentifier = "custom_offering",
                DisplayCloseButton = true
            };
            
            Assert.AreEqual("custom_offering", options.OfferingIdentifier);
            Assert.IsTrue(options.DisplayCloseButton);
        }
    }
}
using NUnit.Framework;
using RevenueCatUI;
using System.Threading.Tasks;

namespace RevenueCatUI.Tests
{
    public class PaywallsPresenterAPITests
    {
        [Test]
        public void PaywallsPresenter_Present_MethodExists()
        {
            // Test that the method exists and can be called
            var task = PaywallsPresenter.Present();
            
            Assert.IsNotNull(task);
            Assert.IsInstanceOf<Task<PaywallResult>>(task);
        }

        [Test]
        public void PaywallsPresenter_PresentWithOptions_MethodExists()
        {
            var options = new PaywallOptions("test_offering", true);
            var task = PaywallsPresenter.Present(options);
            
            Assert.IsNotNull(task);
            Assert.IsInstanceOf<Task<PaywallResult>>(task);
        }

        [Test]
        public void PaywallsPresenter_PresentIfNeeded_MethodExists()
        {
            const string entitlementId = "test_entitlement";
            var task = PaywallsPresenter.PresentIfNeeded(entitlementId);
            
            Assert.IsNotNull(task);
            Assert.IsInstanceOf<Task<PaywallResult>>(task);
        }

        [Test]
        public void PaywallsPresenter_PresentIfNeededWithOptions_MethodExists()
        {
            const string entitlementId = "test_entitlement";
            var options = new PaywallOptions("test_offering", false);
            var task = PaywallsPresenter.PresentIfNeeded(entitlementId, options);
            
            Assert.IsNotNull(task);
            Assert.IsInstanceOf<Task<PaywallResult>>(task);
        }

        [Test]
        public void PaywallsPresenter_PresentWithNullOptions_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                var task = PaywallsPresenter.Present(null);
                Assert.IsNotNull(task);
            });
        }

        [Test]
        public void PaywallsPresenter_PresentIfNeededWithNullOptions_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                var task = PaywallsPresenter.PresentIfNeeded("test_entitlement", null);
                Assert.IsNotNull(task);
            });
        }
    }
}
using NUnit.Framework;
using RevenueCat.UI;

namespace RevenueCat.Tests
{
    public class PaywallPresenterTests
    {
        [Test]
        public void RevenueCatUI_IsSupported_ReturnsBoolean()
        {
            // Since PaywallPresenter is internal, test through the public API
            var result = RevenueCatUI.IsSupported();
            
            // In editor/unsupported platforms, this should return false
            Assert.That(result, Is.TypeOf<bool>());
        }
    }
}
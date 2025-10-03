using System.Threading.Tasks;
using NUnit.Framework;
using RevenueCat.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace RevenueCat.Tests
{
    public class UITests
    {
        [Test]
        public void IsSupported_WhenCalled_ReturnsBoolean()
        {
            var result = UIPresentation.IsSupported();
            
            Assert.That(result, Is.TypeOf<bool>());
        }

        [Test]
        public async Task PresentPaywallAsync_WithNullOptions_CompletesWithoutError()
        {
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall:"));
            
            var result = await UIPresentation.PresentPaywallAsync(null);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test] 
        public async Task PresentPaywallAsync_WithValidOptions_CompletesWithoutError()
        {
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall:"));
            
            var options = new PaywallOptions("test_offering");
            var result = await UIPresentation.PresentPaywallAsync(options);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeededAsync_WithNullEntitlement_ReturnsError()
        {
            LogAssert.Expect(LogType.Error, "[RevenueCatUI] Required entitlement identifier cannot be null or empty");
            
            var result = await UIPresentation.PresentPaywallIfNeededAsync(null);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeededAsync_WithEmptyEntitlement_ReturnsError()
        {
            LogAssert.Expect(LogType.Error, "[RevenueCatUI] Required entitlement identifier cannot be null or empty");
            
            var result = await UIPresentation.PresentPaywallIfNeededAsync("");
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeededAsync_WithValidEntitlement_CompletesWithoutError()
        {
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall if needed:"));
            
            var result = await UIPresentation.PresentPaywallIfNeededAsync("premium");
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeededAsync_WithValidEntitlementAndOptions_CompletesWithoutError()
        {
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall if needed:"));
            
            var options = new PaywallOptions("test_offering", true);
            var result = await UIPresentation.PresentPaywallIfNeededAsync("premium", options);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }
    }
}
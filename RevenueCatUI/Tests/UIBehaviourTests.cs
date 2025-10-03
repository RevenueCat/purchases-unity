using System.Threading.Tasks;
using NUnit.Framework;
using RevenueCat.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace RevenueCat.Tests
{
    public class UIBehaviourTests
    {
        private UIBehaviour uiBehaviour;
        private GameObject testGameObject;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject("TestUIBehaviour");
            uiBehaviour = testGameObject.AddComponent<UIBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void IsSupported_WhenCalled_ReturnsBoolean()
        {
            var result = uiBehaviour.IsSupported();
            
            Assert.That(result, Is.TypeOf<bool>());
        }

        [Test]
        public async Task PresentPaywall_WithNullOptions_CompletesWithoutError()
        {
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall:"));
            
            var result = await uiBehaviour.PresentPaywall(null);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywall_WithValidOptions_CompletesWithoutError()
        {
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall:"));
            
            var options = new PaywallOptions("test_offering");
            var result = await uiBehaviour.PresentPaywall(options);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeeded_WithNullEntitlement_ReturnsError()
        {
            LogAssert.Expect(LogType.Error, "[RevenueCatUI] Required entitlement identifier cannot be null or empty");
            
            var result = await uiBehaviour.PresentPaywallIfNeeded(null);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeeded_WithEmptyEntitlement_ReturnsError()
        {
            LogAssert.Expect(LogType.Error, "[RevenueCatUI] Required entitlement identifier cannot be null or empty");
            
            var result = await uiBehaviour.PresentPaywallIfNeeded("");
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeeded_WithValidEntitlement_CompletesWithoutError()
        {
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall if needed:"));
            
            var result = await uiBehaviour.PresentPaywallIfNeeded("premium");
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeeded_WithValidEntitlementAndOptions_CompletesWithoutError()
        {
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall if needed:"));
            
            var options = new PaywallOptions("test_offering", true);
            var result = await uiBehaviour.PresentPaywallIfNeeded("premium", options);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public void UIBehaviour_WhenCreated_HasValidGameObjectName()
        {
            Assert.That(uiBehaviour.gameObject.name, Is.EqualTo("TestUIBehaviour"));
        }
    }
}
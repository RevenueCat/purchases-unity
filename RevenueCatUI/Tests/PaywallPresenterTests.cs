using System.Threading.Tasks;
using NUnit.Framework;
using RevenueCat.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace RevenueCat.Tests
{
    public class PaywallPresenterTests
    {
        [Test]
        public void PaywallPresenter_Instance_IsNotNull()
        {
            var instance = PaywallPresenter.Instance;
            
            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void PaywallPresenter_Instance_ReturnsSameInstance()
        {
            var instance1 = PaywallPresenter.Instance;
            var instance2 = PaywallPresenter.Instance;
            
            Assert.That(instance1, Is.SameAs(instance2));
        }

        [Test]
        public void PaywallPresenter_Instance_ImplementsIPaywallPresenter()
        {
            var instance = PaywallPresenter.Instance;
            
            Assert.That(instance, Is.InstanceOf<IPaywallPresenter>());
        }
    }

    public class UnsupportedPaywallPresenterTests
    {
        private UnsupportedPaywallPresenter presenter;

        [SetUp]
        public void SetUp()
        {
            presenter = new UnsupportedPaywallPresenter();
        }

        [Test]
        public void IsSupported_ReturnsalwaysFalse()
        {
            var result = presenter.IsSupported();
            
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task PresentPaywallAsync_ReturnsError()
        {
            LogAssert.Expect(LogType.Warning, "[RevenueCatUI] Paywall presentation is not supported on this platform.");
            
            var options = new PaywallOptions();
            var result = await presenter.PresentPaywallAsync("testObject", options);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeededAsync_ReturnsError()
        {
            LogAssert.Expect(LogType.Warning, "[RevenueCatUI] Paywall presentation is not supported on this platform.");
            
            var options = new PaywallOptions();
            var result = await presenter.PresentPaywallIfNeededAsync("testObject", "premium", options);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallAsync_WithNullOptions_ReturnsError()
        {
            LogAssert.Expect(LogType.Warning, "[RevenueCatUI] Paywall presentation is not supported on this platform.");
            
            var result = await presenter.PresentPaywallAsync("testObject", null);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeededAsync_WithNullOptions_ReturnsError()
        {
            LogAssert.Expect(LogType.Warning, "[RevenueCatUI] Paywall presentation is not supported on this platform.");
            
            var result = await presenter.PresentPaywallIfNeededAsync("testObject", "premium", null);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallAsync_WithNullGameObjectName_ReturnsError()
        {
            LogAssert.Expect(LogType.Warning, "[RevenueCatUI] Paywall presentation is not supported on this platform.");
            
            var options = new PaywallOptions();
            var result = await presenter.PresentPaywallAsync(null, options);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task PresentPaywallIfNeededAsync_WithNullGameObjectName_ReturnsError()
        {
            LogAssert.Expect(LogType.Warning, "[RevenueCatUI] Paywall presentation is not supported on this platform.");
            
            var options = new PaywallOptions();
            var result = await presenter.PresentPaywallIfNeededAsync(null, "premium", options);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }
    }
}
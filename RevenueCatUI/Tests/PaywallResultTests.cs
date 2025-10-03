using NUnit.Framework;
using RevenueCat.UI;

namespace RevenueCat.Tests
{
    public class PaywallResultTests
    {
        [Test]
        public void PaywallResult_Constructor_SetsResult()
        {
            var result = new PaywallResult(PaywallResultType.Purchased);
            
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Purchased));
        }

        [Test]
        public void PaywallResult_ToString_ReturnsFormattedString()
        {
            var result = new PaywallResult(PaywallResultType.Cancelled);
            
            Assert.That(result.ToString(), Is.EqualTo("PaywallResult(Cancelled)"));
        }

        [Test]
        public void PaywallResultType_ToNativeString_ReturnsCorrectValues()
        {
            Assert.That(PaywallResultType.NotPresented.ToNativeString(), Is.EqualTo("NOT_PRESENTED"));
            Assert.That(PaywallResultType.Cancelled.ToNativeString(), Is.EqualTo("CANCELLED"));
            Assert.That(PaywallResultType.Error.ToNativeString(), Is.EqualTo("ERROR"));
            Assert.That(PaywallResultType.Purchased.ToNativeString(), Is.EqualTo("PURCHASED"));
            Assert.That(PaywallResultType.Restored.ToNativeString(), Is.EqualTo("RESTORED"));
        }

        [Test]
        public void PaywallResultType_FromNativeString_ReturnsCorrectValues()
        {
            Assert.That(PaywallResultTypeExtensions.FromNativeString("NOT_PRESENTED"), Is.EqualTo(PaywallResultType.NotPresented));
            Assert.That(PaywallResultTypeExtensions.FromNativeString("CANCELLED"), Is.EqualTo(PaywallResultType.Cancelled));
            Assert.That(PaywallResultTypeExtensions.FromNativeString("ERROR"), Is.EqualTo(PaywallResultType.Error));
            Assert.That(PaywallResultTypeExtensions.FromNativeString("PURCHASED"), Is.EqualTo(PaywallResultType.Purchased));
            Assert.That(PaywallResultTypeExtensions.FromNativeString("RESTORED"), Is.EqualTo(PaywallResultType.Restored));
        }

        [Test]
        public void PaywallResultType_FromNativeString_WithUnknownValue_ReturnsError()
        {
            Assert.That(PaywallResultTypeExtensions.FromNativeString("UNKNOWN"), Is.EqualTo(PaywallResultType.Error));
            Assert.That(PaywallResultTypeExtensions.FromNativeString(""), Is.EqualTo(PaywallResultType.Error));
            Assert.That(PaywallResultTypeExtensions.FromNativeString(null), Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public void PaywallResultType_ToNativeString_RoundTrip_PreservesValue()
        {
            var allTypes = new[]
            {
                PaywallResultType.NotPresented,
                PaywallResultType.Cancelled,
                PaywallResultType.Error,
                PaywallResultType.Purchased,
                PaywallResultType.Restored
            };

            foreach (var type in allTypes)
            {
                var nativeString = type.ToNativeString();
                var roundTrip = PaywallResultTypeExtensions.FromNativeString(nativeString);
                
                Assert.That(roundTrip, Is.EqualTo(type), $"Round trip failed for {type}");
            }
        }

        [Test]
        public void PaywallResult_StaticProperties_ReturnCorrectTypes()
        {
            Assert.That(PaywallResult.NotPresented.Result, Is.EqualTo(PaywallResultType.NotPresented));
            Assert.That(PaywallResult.Cancelled.Result, Is.EqualTo(PaywallResultType.Cancelled));
            Assert.That(PaywallResult.Error.Result, Is.EqualTo(PaywallResultType.Error));
            Assert.That(PaywallResult.Purchased.Result, Is.EqualTo(PaywallResultType.Purchased));
            Assert.That(PaywallResult.Restored.Result, Is.EqualTo(PaywallResultType.Restored));
        }
    }
}
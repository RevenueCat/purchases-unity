using NUnit.Framework;
using RevenueCatUI;

namespace RevenueCatUI.Tests
{
    public class PaywallResultAPITests
    {
        [Test]
        public void PaywallResultType_EnumValues_AreCorrect()
        {
            // Test that all expected enum values exist
            Assert.IsTrue(System.Enum.IsDefined(typeof(PaywallResultType), PaywallResultType.NotPresented));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PaywallResultType), PaywallResultType.Cancelled));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PaywallResultType), PaywallResultType.Error));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PaywallResultType), PaywallResultType.Purchased));
            Assert.IsTrue(System.Enum.IsDefined(typeof(PaywallResultType), PaywallResultType.Restored));
        }

        [Test]
        public void PaywallResult_Constructor_SetsResultType()
        {
            var result = new PaywallResult(PaywallResultType.Purchased);
            
            Assert.IsNotNull(result);
            Assert.AreEqual(PaywallResultType.Purchased, result.Result);
        }

        [Test]
        public void PaywallResult_Constructor_WithAllResultTypes_WorksCorrectly()
        {
            // Test creating PaywallResult with each enum value
            var notPresentedResult = new PaywallResult(PaywallResultType.NotPresented);
            Assert.AreEqual(PaywallResultType.NotPresented, notPresentedResult.Result);
            
            var cancelledResult = new PaywallResult(PaywallResultType.Cancelled);
            Assert.AreEqual(PaywallResultType.Cancelled, cancelledResult.Result);
            
            var errorResult = new PaywallResult(PaywallResultType.Error);
            Assert.AreEqual(PaywallResultType.Error, errorResult.Result);
            
            var purchasedResult = new PaywallResult(PaywallResultType.Purchased);
            Assert.AreEqual(PaywallResultType.Purchased, purchasedResult.Result);
            
            var restoredResult = new PaywallResult(PaywallResultType.Restored);
            Assert.AreEqual(PaywallResultType.Restored, restoredResult.Result);
        }

        [Test]
        public void PaywallResultTypeExtensions_ToNativeString_ReturnsValidStrings()
        {
            Assert.IsNotNull(PaywallResultTypeExtensions.ToNativeString(PaywallResultType.NotPresented));
            Assert.IsNotNull(PaywallResultTypeExtensions.ToNativeString(PaywallResultType.Cancelled));
            Assert.IsNotNull(PaywallResultTypeExtensions.ToNativeString(PaywallResultType.Error));
            Assert.IsNotNull(PaywallResultTypeExtensions.ToNativeString(PaywallResultType.Purchased));
            Assert.IsNotNull(PaywallResultTypeExtensions.ToNativeString(PaywallResultType.Restored));
        }

        [Test]
        public void PaywallResultTypeExtensions_FromNativeString_HandlesValidStrings()
        {
            // Test roundtrip conversion for each enum value
            foreach (PaywallResultType enumValue in System.Enum.GetValues(typeof(PaywallResultType)))
            {
                var nativeString = PaywallResultTypeExtensions.ToNativeString(enumValue);
                var convertedBack = PaywallResultTypeExtensions.FromNativeString(nativeString);
                Assert.AreEqual(enumValue, convertedBack);
            }
        }
    }
}
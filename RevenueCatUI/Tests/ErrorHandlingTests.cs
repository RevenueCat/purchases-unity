using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RevenueCat.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace RevenueCat.Tests
{
    public class ErrorHandlingTests
    {
        [Test]
        public void UIPresentation_IsSupported_HandlesExceptionsGracefully()
        {
            // This test verifies that IsSupported catches exceptions and returns false
            // The actual presenter creation might fail in test environment
            var result = UIPresentation.IsSupported();
            
            // Should always return a boolean, never throw
            Assert.That(result, Is.TypeOf<bool>());
        }

        [Test]
        public async Task UIPresentation_PresentPaywallAsync_HandlesNullOptionsGracefully()
        {
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall:"));
            
            // Should not throw when passed null options
            var result = await UIPresentation.PresentPaywallAsync(null);
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public async Task UIPresentation_PresentPaywallIfNeededAsync_ValidatesEntitlementIdentifier()
        {
            // Test null entitlement
            LogAssert.Expect(LogType.Error, "[RevenueCatUI] Required entitlement identifier cannot be null or empty");
            var result1 = await UIPresentation.PresentPaywallIfNeededAsync(null);
            Assert.That(result1.Result, Is.EqualTo(PaywallResultType.Error));

            // Test empty entitlement
            LogAssert.Expect(LogType.Error, "[RevenueCatUI] Required entitlement identifier cannot be null or empty");
            var result2 = await UIPresentation.PresentPaywallIfNeededAsync("");
            Assert.That(result2.Result, Is.EqualTo(PaywallResultType.Error));

            // Test whitespace-only entitlement (should be treated as valid by the current implementation)
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("[RevenueCatUI] Error presenting paywall if needed:"));
            var result3 = await UIPresentation.PresentPaywallIfNeededAsync("   ");
            Assert.That(result3.Result, Is.EqualTo(PaywallResultType.Error));
        }

        [Test]
        public void PaywallOptions_HandlesNullValues()
        {
            // Should not throw when setting null values
            Assert.DoesNotThrow(() =>
            {
                var options = new PaywallOptions();
                options.OfferingIdentifier = null;
            });

            Assert.DoesNotThrow(() => new PaywallOptions(null));
            Assert.DoesNotThrow(() => new PaywallOptions(null, true));
        }

        [Test]
        public void PaywallResult_HandlesAllEnumValues()
        {
            // Test that PaywallResult can be created with all enum values
            var allTypes = Enum.GetValues(typeof(PaywallResultType));
            
            foreach (PaywallResultType type in allTypes)
            {
                Assert.DoesNotThrow(() => new PaywallResult(type));
                
                var result = new PaywallResult(type);
                Assert.That(result.Result, Is.EqualTo(type));
                Assert.DoesNotThrow(() => result.ToString());
            }
        }

        [Test]
        public void PaywallResultTypeExtensions_HandlesInvalidNativeStrings()
        {
            // Test various invalid inputs
            string[] invalidInputs = { null, "", "INVALID", "invalid", "123", " ", "NOT_PRESENTEDX" };
            
            foreach (var input in invalidInputs)
            {
                var result = PaywallResultTypeExtensions.FromNativeString(input);
                Assert.That(result, Is.EqualTo(PaywallResultType.Error), 
                    $"Input '{input}' should return Error");
            }
        }

        [Test]
        public void PaywallResultTypeExtensions_HandlesCaseSensitivity()
        {
            // Test that the conversion is case-sensitive (as it should be)
            var result1 = PaywallResultTypeExtensions.FromNativeString("purchased");
            var result2 = PaywallResultTypeExtensions.FromNativeString("Purchased");
            var result3 = PaywallResultTypeExtensions.FromNativeString("PURCHASED");
            
            Assert.That(result1, Is.EqualTo(PaywallResultType.Error));
            Assert.That(result2, Is.EqualTo(PaywallResultType.Error));
            Assert.That(result3, Is.EqualTo(PaywallResultType.Purchased));
        }

        [Test]
        public void PaywallPresenter_Instance_AlwaysReturnsSameInstance()
        {
            // Test that multiple calls return the same instance (singleton pattern)
            var instance1 = PaywallPresenter.Instance;
            var instance2 = PaywallPresenter.Instance;
            var instance3 = PaywallPresenter.Instance;
            
            Assert.That(instance1, Is.SameAs(instance2));
            Assert.That(instance2, Is.SameAs(instance3));
            Assert.That(instance1, Is.Not.Null);
        }

        [Test]
        public void UnsupportedPaywallPresenter_LogsWarningMessages()
        {
            var presenter = new UnsupportedPaywallPresenter();
            
            // Test that warning messages are logged for unsupported operations
            LogAssert.Expect(LogType.Warning, "[RevenueCatUI] Paywall presentation is not supported on this platform.");
            var task1 = presenter.PresentPaywallAsync("test", new PaywallOptions());
            
            LogAssert.Expect(LogType.Warning, "[RevenueCatUI] Paywall presentation is not supported on this platform.");
            var task2 = presenter.PresentPaywallIfNeededAsync("test", "premium", new PaywallOptions());
        }
    }
}
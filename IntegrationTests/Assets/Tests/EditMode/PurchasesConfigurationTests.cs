using NUnit.Framework;

namespace RevenueCat.Tests
{
    public class PurchasesConfigurationTests
    {
        /// <remarks>
        /// This locks in what the builder does today, which is not what the SDK documents.
        /// The builder leaves StoreKitVersion, ShouldShowInAppMessagesAutomatically and
        /// EntitlementVerificationMode at their C# zero values, so the runtime-setup path defaults to
        /// StoreKit 1, in-app messages disabled and verification disabled, while the inspector fields on
        /// Purchases default to StoreKitVersion.Default, in-app messages enabled and Informational.
        /// Those divergences look unintentional, but aligning the builder changes runtime behavior for
        /// apps using runtime setup, so it is handled separately from these tests.
        /// </remarks>
        [Test]
        public void BuildUsesBuilderDefaults()
        {
            var configuration = Purchases.PurchasesConfiguration.Builder
                .Init("test_api_key")
                .Build();

            Assert.That(configuration.ApiKey, Is.EqualTo("test_api_key"));
            Assert.That(configuration.AppUserId, Is.Null);
            Assert.That(configuration.PurchasesAreCompletedBy, Is.EqualTo(Purchases.PurchasesAreCompletedBy.RevenueCat));
            Assert.That(configuration.UserDefaultsSuiteName, Is.Null);
            Assert.That(configuration.UseAmazon, Is.False);
            Assert.That(configuration.DangerousSettings.AutoSyncPurchases, Is.True);
            // Diverges from the Purchases inspector default (StoreKitVersion.Default).
            Assert.That(configuration.StoreKitVersion, Is.EqualTo(Purchases.StoreKitVersion.StoreKit1));
            // Diverges from the documented default (in-app messages are shown automatically).
            Assert.That(configuration.ShouldShowInAppMessagesAutomatically, Is.False);
            // Diverges from the Purchases inspector default (EntitlementVerificationMode.Informational).
            Assert.That(configuration.EntitlementVerificationMode, Is.EqualTo(Purchases.EntitlementVerificationMode.Disabled));
            Assert.That(configuration.PendingTransactionsForPrepaidPlansEnabled, Is.False);
            Assert.That(configuration.DiagnosticsEnabled, Is.False);
            Assert.That(configuration.AutomaticDeviceIdentifierCollectionEnabled, Is.True);
            Assert.That(configuration.PreferredUILocaleOverride, Is.Null);
            Assert.That(configuration.ProxyURL, Is.Null);
        }

        [Test]
        public void BuildUsesConfiguredValues()
        {
            var dangerousSettings = new Purchases.DangerousSettings(false);

            var configuration = Purchases.PurchasesConfiguration.Builder
                .Init("test_api_key")
                .SetAppUserId("app_user_id")
                .SetPurchasesAreCompletedBy(Purchases.PurchasesAreCompletedBy.MyApp, Purchases.StoreKitVersion.StoreKit2)
                .SetUserDefaultsSuiteName("suite_name")
                .SetUseAmazon(true)
                .SetDangerousSettings(dangerousSettings)
                .SetShouldShowInAppMessagesAutomatically(true)
                .SetEntitlementVerificationMode(Purchases.EntitlementVerificationMode.Informational)
                .SetPendingTransactionsForPrepaidPlansEnabled(true)
                .SetDiagnosticsEnabled(true)
                .SetAutomaticDeviceIdentifierCollectionEnabled(false)
                .SetPreferredUILocaleOverride("de_DE")
                .SetProxyURL("https://proxy.revenuecat.com")
                .Build();

            Assert.That(configuration.AppUserId, Is.EqualTo("app_user_id"));
            Assert.That(configuration.PurchasesAreCompletedBy, Is.EqualTo(Purchases.PurchasesAreCompletedBy.MyApp));
            Assert.That(configuration.StoreKitVersion, Is.EqualTo(Purchases.StoreKitVersion.StoreKit2));
            Assert.That(configuration.UserDefaultsSuiteName, Is.EqualTo("suite_name"));
            Assert.That(configuration.UseAmazon, Is.True);
            Assert.That(configuration.DangerousSettings, Is.SameAs(dangerousSettings));
            Assert.That(configuration.ShouldShowInAppMessagesAutomatically, Is.True);
            Assert.That(configuration.EntitlementVerificationMode, Is.EqualTo(Purchases.EntitlementVerificationMode.Informational));
            Assert.That(configuration.PendingTransactionsForPrepaidPlansEnabled, Is.True);
            Assert.That(configuration.DiagnosticsEnabled, Is.True);
            Assert.That(configuration.AutomaticDeviceIdentifierCollectionEnabled, Is.False);
            Assert.That(configuration.PreferredUILocaleOverride, Is.EqualTo("de_DE"));
            Assert.That(configuration.ProxyURL, Is.EqualTo("https://proxy.revenuecat.com"));
        }

        [Test]
        public void BuildRestoresDefaultDangerousSettingsWhenSetToNull()
        {
            var configuration = Purchases.PurchasesConfiguration.Builder
                .Init("test_api_key")
                .SetDangerousSettings(null)
                .Build();

            Assert.That(configuration.DangerousSettings, Is.Not.Null);
            Assert.That(configuration.DangerousSettings.AutoSyncPurchases, Is.True);
        }
    }
}

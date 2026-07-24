using NUnit.Framework;

namespace RevenueCat.Tests
{
    public class PurchasesConfigurationTests
    {
        [Test]
        public void BuildUsesDocumentedDefaults()
        {
            var configuration = Purchases.PurchasesConfiguration.Builder
                .Init("test_api_key")
                .Build();

            Assert.That(configuration.ApiKey, Is.EqualTo("test_api_key"));
            Assert.That(configuration.AppUserId, Is.Null);
            Assert.That(configuration.PurchasesAreCompletedBy, Is.EqualTo(default(Purchases.PurchasesAreCompletedBy)));
            Assert.That(configuration.UserDefaultsSuiteName, Is.Null);
            Assert.That(configuration.UseAmazon, Is.False);
            Assert.That(configuration.DangerousSettings.AutoSyncPurchases, Is.True);
            Assert.That(configuration.StoreKitVersion, Is.EqualTo(default(Purchases.StoreKitVersion)));
            Assert.That(configuration.ShouldShowInAppMessagesAutomatically, Is.False);
            Assert.That(configuration.EntitlementVerificationMode, Is.EqualTo(default(Purchases.EntitlementVerificationMode)));
            Assert.That(configuration.PendingTransactionsForPrepaidPlansEnabled, Is.False);
            Assert.That(configuration.DiagnosticsEnabled, Is.False);
            Assert.That(configuration.AutomaticDeviceIdentifierCollectionEnabled, Is.True);
            Assert.That(configuration.PreferredUILocaleOverride, Is.Null);
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

using NUnit.Framework;
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class PurchasesCallTests
    {
        private GameObject _gameObject;
        private Purchases _purchases;
        private PurchasesWrapperSpy _wrapper;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("RevenueCatTests");
            _purchases = _gameObject.AddComponent<Purchases>();
            _wrapper = new PurchasesWrapperSpy();
            _purchases.SetWrapper(_wrapper);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void ConfigureForwardsEveryConfigurationValue()
        {
            var configuration = Purchases.PurchasesConfiguration.Builder
                .Init("test_api_key")
                .SetAppUserId("app_user_id")
                .SetPurchasesAreCompletedBy(Purchases.PurchasesAreCompletedBy.MyApp,
                    Purchases.StoreKitVersion.StoreKit2)
                .SetUserDefaultsSuiteName("suite_name")
                .SetUseAmazon(true)
                .SetDangerousSettings(new Purchases.DangerousSettings(false))
                .SetShouldShowInAppMessagesAutomatically(true)
                .SetEntitlementVerificationMode(Purchases.EntitlementVerificationMode.Informational)
                .SetPendingTransactionsForPrepaidPlansEnabled(true)
                .SetDiagnosticsEnabled(true)
                .SetAutomaticDeviceIdentifierCollectionEnabled(false)
                .SetPreferredUILocaleOverride("de_DE")
                .Build();

            _purchases.Configure(configuration);

            var invocation = AssertOnlyInvocation(nameof(IPurchasesWrapper.Setup), 14);
            Assert.That(invocation.Arguments[0], Is.EqualTo(_gameObject.name));
            Assert.That(invocation.Arguments[1], Is.EqualTo("test_api_key"));
            Assert.That(invocation.Arguments[2], Is.EqualTo("app_user_id"));
            Assert.That(invocation.Arguments[3], Is.EqualTo(Purchases.PurchasesAreCompletedBy.MyApp));
            Assert.That(invocation.Arguments[4], Is.EqualTo(Purchases.StoreKitVersion.StoreKit2));
            Assert.That(invocation.Arguments[5], Is.EqualTo("suite_name"));
            Assert.That(invocation.Arguments[6], Is.True);
            var dangerousSettings = JSONNode.Parse((string)invocation.Arguments[7]);
            Assert.That(dangerousSettings["AutoSyncPurchases"].AsBool, Is.False);
            Assert.That(dangerousSettings.HasKey("UseWorkflows"), Is.False);
            Assert.That(invocation.Arguments[8], Is.True);
            Assert.That(invocation.Arguments[9], Is.EqualTo(Purchases.EntitlementVerificationMode.Informational));
            Assert.That(invocation.Arguments[10], Is.True);
            Assert.That(invocation.Arguments[11], Is.True);
            Assert.That(invocation.Arguments[12], Is.False);
            Assert.That(invocation.Arguments[13], Is.EqualTo("de_DE"));
        }

        /// <remarks>
        /// The proxy URL has to reach the native SDK before it is configured, so this pins the order
        /// rather than just the fact that both calls happen.
        /// </remarks>
        [Test]
        public void ConfigureSetsProxyUrlBeforeSettingUp()
        {
            var configuration = Purchases.PurchasesConfiguration.Builder
                .Init("test_api_key")
                .SetProxyURL("https://proxy.revenuecat.com")
                .Build();

            _purchases.Configure(configuration);

            Assert.That(_wrapper.Invocations, Has.Count.EqualTo(2));
            Assert.That(_wrapper.Invocations[0].Method, Is.EqualTo(nameof(IPurchasesWrapper.SetProxyURL)));
            Assert.That(_wrapper.Invocations[0].Arguments[0], Is.EqualTo("https://proxy.revenuecat.com"));
            Assert.That(_wrapper.Invocations[1].Method, Is.EqualTo(nameof(IPurchasesWrapper.Setup)));
        }

        /// <remarks>
        /// A configuration that carries no proxy URL must not call the wrapper at all, otherwise it would
        /// clear a proxy URL already set through the inspector field on Purchases.
        /// </remarks>
        [Test]
        public void ConfigureWithoutProxyUrlDoesNotCallSetProxyUrl()
        {
            var configuration = Purchases.PurchasesConfiguration.Builder
                .Init("test_api_key")
                .Build();

            _purchases.Configure(configuration);

            AssertOnlyInvocation(nameof(IPurchasesWrapper.Setup), 14);
        }

        private PurchasesWrapperSpy.Invocation AssertOnlyInvocation(string method, int argumentCount)
        {
            Assert.That(_wrapper.Invocations, Has.Count.EqualTo(1));

            var invocation = _wrapper.Invocations[0];
            Assert.That(invocation.Method, Is.EqualTo(method));
            Assert.That(invocation.Arguments, Has.Length.EqualTo(argumentCount));
            return invocation;
        }
    }
}

using System.Collections.Generic;
using NUnit.Framework;
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class WrapperPassthroughTests
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
        public void SetAttributesSerializesStringsAndNulls()
        {
            _purchases.SetAttributes(new Dictionary<string, string>
            {
                ["plan"] = "premium",
                ["nickname"] = null
            });

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SetAttributes), 1);
            var attributes = JSONNode.Parse((string)invocation.Arguments[0]);
            Assert.That(attributes["plan"].Value, Is.EqualTo("premium"));
            Assert.That(attributes["nickname"].IsNull, Is.True);
        }

        [Test]
        public void ShowInAppMessagesForwardsMessageTypes()
        {
            var messageTypes = new[] { Purchases.InAppMessageType.BillingIssue };

            _purchases.ShowInAppMessages(messageTypes);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.ShowInAppMessages), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(messageTypes));
        }

        [Test]
        public void ShowInAppMessagesForwardsNullWhenNotSpecified()
        {
            _purchases.ShowInAppMessages();

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.ShowInAppMessages), 1);
            Assert.That(invocation.Arguments[0], Is.Null);
        }

        [Test]
        public void OverridePreferredUILocaleForwardsLocale()
        {
            _purchases.OverridePreferredUILocale("de_DE");

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.OverridePreferredUILocale), 1);
            Assert.That(invocation.Arguments[0], Is.EqualTo("de_DE"));
        }

        [Test]
        public void InvalidateCustomerInfoCacheCallsWrapper()
        {
            _purchases.InvalidateCustomerInfoCache();

            AssertLastInvocation(nameof(IPurchasesWrapper.InvalidateCustomerInfoCache), 0);
        }

        [Test]
        public void InvalidateVirtualCurrenciesCacheCallsWrapper()
        {
            _purchases.InvalidateVirtualCurrenciesCache();

            AssertLastInvocation(nameof(IPurchasesWrapper.InvalidateVirtualCurrenciesCache), 0);
        }

        [Test]
        public void SetAppsFlyerConversionDataSerializesNestedValues()
        {
            _purchases.SetAppsFlyerConversionData(new Dictionary<string, object>
            {
                ["af_status"] = "Organic",
                ["media_source"] = null,
                ["click_count"] = 3,
                ["nested"] = new Dictionary<string, object> { ["key"] = "value" },
                ["list"] = new List<object> { "a", "b" }
            });

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SetAppsFlyerConversionData), 1);
            var conversionData = JSONNode.Parse((string)invocation.Arguments[0]);
            Assert.That(conversionData["af_status"].Value, Is.EqualTo("Organic"));
            Assert.That(conversionData["media_source"].IsNull, Is.True);
            Assert.That(conversionData["click_count"].AsInt, Is.EqualTo(3));
            Assert.That(conversionData["nested"]["key"].Value, Is.EqualTo("value"));
            Assert.That(conversionData["list"][0].Value, Is.EqualTo("a"));
            Assert.That(conversionData["list"][1].Value, Is.EqualTo("b"));
        }

        [Test]
        public void SyncAmazonPurchaseForwardsArgumentsInOrder()
        {
            _purchases.SyncAmazonPurchase("product_1", "receipt_1", "amazon_user_1", "USD", 9.99);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SyncAmazonPurchase), 5);
            Assert.That(invocation.Arguments[0], Is.EqualTo("product_1"));
            Assert.That(invocation.Arguments[1], Is.EqualTo("receipt_1"));
            Assert.That(invocation.Arguments[2], Is.EqualTo("amazon_user_1"));
            Assert.That(invocation.Arguments[3], Is.EqualTo("USD"));
            Assert.That(invocation.Arguments[4], Is.EqualTo(9.99));
        }

        [Test]
        public void CollectDeviceIdentifiersCallsWrapper()
        {
            _purchases.CollectDeviceIdentifiers();

            AssertLastInvocation(nameof(IPurchasesWrapper.CollectDeviceIdentifiers), 0);
        }

        [Test]
        public void SetSimulatesAskToBuyInSandboxForwardsFlag()
        {
            _purchases.SetSimulatesAskToBuyInSandbox(true);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SetSimulatesAskToBuyInSandbox), 1);
            Assert.That(invocation.Arguments[0], Is.True);
        }

        [Test]
        public void SetAdjustIdForwardsValue()
        {
            _purchases.SetAdjustID("adjust_id_1");

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SetAdjustID), 1);
            Assert.That(invocation.Arguments[0], Is.EqualTo("adjust_id_1"));
        }

        private PurchasesWrapperSpy.Invocation AssertLastInvocation(string method, int argumentCount)
        {
            Assert.That(_wrapper.Invocations, Has.Count.EqualTo(1));
            Assert.That(_wrapper.LastInvocation.Method, Is.EqualTo(method));
            Assert.That(_wrapper.LastInvocation.Arguments, Has.Length.EqualTo(argumentCount));
            return _wrapper.LastInvocation;
        }
    }
}

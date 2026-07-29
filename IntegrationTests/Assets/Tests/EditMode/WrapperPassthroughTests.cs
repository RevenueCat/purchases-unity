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
                ["list"] = new List<object> { "a", "b" },
                ["is_first_launch"] = true,
                ["is_retargeting"] = false,
                ["install_time_ms"] = 1717171717171L,
                ["revenue_float"] = 9.99f,
                ["revenue_double"] = 12.34d
            });

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SetAppsFlyerConversionData), 1);
            var conversionDataJson = (string)invocation.Arguments[0];
            var conversionData = JSONNode.Parse(conversionDataJson);
            Assert.That(conversionData["af_status"].Value, Is.EqualTo("Organic"));
            Assert.That(conversionData["media_source"].IsNull, Is.True);
            Assert.That(conversionData["click_count"].IsNumber, Is.True);
            Assert.That(conversionData["click_count"].AsInt, Is.EqualTo(3));
            Assert.That(conversionData["nested"]["key"].Value, Is.EqualTo("value"));
            Assert.That(conversionData["list"][0].Value, Is.EqualTo("a"));
            Assert.That(conversionData["list"][1].Value, Is.EqualTo("b"));
            // The IsBoolean/IsNumber checks carry the weight here. AsBool and AsFloat fall back to
            // parsing the node's string value, so a number or bool that regressed into being
            // serialized as a quoted string still satisfies the value assertion on its own.
            Assert.That(conversionData["is_first_launch"].IsBoolean, Is.True);
            Assert.That(conversionData["is_first_launch"].AsBool, Is.True);
            Assert.That(conversionData["is_retargeting"].IsBoolean, Is.True);
            Assert.That(conversionData["is_retargeting"].AsBool, Is.False);
            Assert.That(conversionData["install_time_ms"].IsNumber, Is.True);
            Assert.That(conversionData["install_time_ms"].AsLong, Is.EqualTo(1717171717171L));
            Assert.That(conversionData["revenue_float"].IsNumber, Is.True);
            Assert.That(conversionData["revenue_float"].AsFloat, Is.EqualTo(9.99f).Within(0.0001f));
            Assert.That(conversionData["revenue_double"].IsNumber, Is.True);
            Assert.That(conversionData["revenue_double"].AsDouble, Is.EqualTo(12.34d).Within(0.0001d));
            Assert.That(conversionDataJson, Does.Contain("\"is_first_launch\":true"));
            Assert.That(conversionDataJson, Does.Contain("\"is_retargeting\":false"));
            Assert.That(conversionDataJson, Does.Contain("\"install_time_ms\":1717171717171"));
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
        public void SetLogLevelForwardsLevel()
        {
            _purchases.SetLogLevel(Purchases.LogLevel.Warn);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SetLogLevel), 1);
            Assert.That(invocation.Arguments[0], Is.EqualTo(Purchases.LogLevel.Warn));
        }

        [Test]
        public void SetDebugLogsEnabledForwardsFlag()
        {
#pragma warning disable 618
            _purchases.SetDebugLogsEnabled(true);
#pragma warning restore 618

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SetDebugLogsEnabled), 1);
            Assert.That(invocation.Arguments[0], Is.True);
        }

        [Test]
        public void SetAllowSharingStoreAccountForwardsFlag()
        {
#pragma warning disable 618
            _purchases.SetAllowSharingStoreAccount(true);
#pragma warning restore 618

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SetAllowSharingStoreAccount), 1);
            Assert.That(invocation.Arguments[0], Is.True);
        }

        [Test]
        public void SyncPurchasesCallsWrapper()
        {
            _purchases.SyncPurchases();

            AssertLastInvocation(nameof(IPurchasesWrapper.SyncPurchases), 0);
        }

        [Test]
        public void SyncPurchasesWithCallbackCallsWrapper()
        {
            _purchases.SyncPurchases((customerInfo, error) => { });

            AssertLastInvocation(nameof(IPurchasesWrapper.SyncPurchases), 0);
        }

        [Test]
        public void SyncAttributesAndOfferingsIfNeededCallsWrapper()
        {
            _purchases.SyncAttributesAndOfferingsIfNeeded((offerings, error) => { });

            AssertLastInvocation(nameof(IPurchasesWrapper.SyncAttributesAndOfferingsIfNeeded), 0);
        }

        [Test]
        public void SetLogHandlerCallsWrapper()
        {
            _purchases.SetLogHandler((logLevel, message) => { });

            AssertLastInvocation(nameof(IPurchasesWrapper.SetLogHandler), 0);
        }

        [Test]
        public void PresentCodeRedemptionSheetCallsWrapper()
        {
            _purchases.PresentCodeRedemptionSheet();

            AssertLastInvocation(nameof(IPurchasesWrapper.PresentCodeRedemptionSheet), 0);
        }

        [Test]
        public void EnableAdServicesAttributionTokenCollectionCallsWrapper()
        {
            _purchases.EnableAdServicesAttributionTokenCollection();

            AssertLastInvocation(nameof(IPurchasesWrapper.EnableAdServicesAttributionTokenCollection), 0);
        }

        [Test]
        public void GetAppUserIdCallsWrapper()
        {
            _purchases.GetAppUserId();

            AssertLastInvocation(nameof(IPurchasesWrapper.GetAppUserId), 0);
        }

        [Test]
        public void IsAnonymousCallsWrapper()
        {
            _purchases.IsAnonymous();

            AssertLastInvocation(nameof(IPurchasesWrapper.IsAnonymous), 0);
        }

        [Test]
        public void IsConfiguredCallsWrapper()
        {
            _purchases.IsConfigured();

            AssertLastInvocation(nameof(IPurchasesWrapper.IsConfigured), 0);
        }

        [Test]
        public void GetCachedVirtualCurrenciesReturnsNullWhenWrapperHasNothingCached()
        {
            var virtualCurrencies = _purchases.GetCachedVirtualCurrencies();

            AssertLastInvocation(nameof(IPurchasesWrapper.GetCachedVirtualCurrencies), 0);
            Assert.That(virtualCurrencies, Is.Null);
        }

        [Test]
        public void TrackCustomPaywallImpressionForwardsParameters()
        {
            var parameters = new Purchases.CustomPaywallImpressionParams("paywall_1");

            _purchases.TrackCustomPaywallImpression(parameters);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackCustomPaywallImpression), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(parameters));
        }

        [Test]
        public void TrackCustomPaywallImpressionWithoutParametersForwardsDefaultParameters()
        {
            _purchases.TrackCustomPaywallImpression();

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackCustomPaywallImpression), 1);
            var parameters = invocation.Arguments[0] as Purchases.CustomPaywallImpressionParams;
            Assert.That(parameters, Is.Not.Null);
            Assert.That(parameters.PaywallId, Is.Null);
            Assert.That(parameters.OfferingId, Is.Null);
        }

        [Test]
        public void TrackAdDisplayedForwardsData()
        {
            var data = new AdDisplayedData(AdTracker.MediatorName.AdMob, AdTracker.Format.Interstitial,
                "ad_unit_1", "impression_1");

            _purchases.AdTracker.TrackAdDisplayed(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdDisplayed), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
        }

        [Test]
        public void TrackAdOpenedForwardsData()
        {
            var data = new AdOpenedData(AdTracker.MediatorName.AppLovin, AdTracker.Format.Banner,
                "ad_unit_1", "impression_1");

            _purchases.AdTracker.TrackAdOpened(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdOpened), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
        }

        [Test]
        public void TrackAdRevenueForwardsData()
        {
            var data = new AdRevenueData(AdTracker.MediatorName.AdMob, AdTracker.Format.Rewarded,
                "ad_unit_1", "impression_1", 1234, "USD", AdTracker.Precision.Exact);

            _purchases.AdTracker.TrackAdRevenue(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdRevenue), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
        }

        [Test]
        public void TrackAdLoadedForwardsData()
        {
            var data = new AdLoadedData(AdTracker.MediatorName.AdMob, AdTracker.Format.Native,
                "ad_unit_1", "impression_1");

            _purchases.AdTracker.TrackAdLoaded(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdLoaded), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
        }

        [Test]
        public void TrackAdFailedToLoadForwardsData()
        {
            var data = new AdFailedToLoadData(AdTracker.MediatorName.AppLovin, AdTracker.Format.AppOpen,
                "ad_unit_1", "placement_1", 42);

            _purchases.AdTracker.TrackAdFailedToLoad(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdFailedToLoad), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
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

using NUnit.Framework;
using RevenueCat;
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class TrackingTests
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
        public void TrackCustomPaywallImpressionWithoutParametersCreatesEmptyParameters()
        {
            _purchases.TrackCustomPaywallImpression();

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackCustomPaywallImpression), 1);
            var parameters = (Purchases.CustomPaywallImpressionParams)invocation.Arguments[0];
            Assert.That(parameters.PaywallId, Is.Null);
            Assert.That(parameters.OfferingId, Is.Null);
            Assert.That(parameters.Offering, Is.Null);
        }

        [Test]
        public void TrackCustomPaywallImpressionWithPaywallIdOnlySetsPaywallIdOnly()
        {
            _purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams("paywall_1"));

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackCustomPaywallImpression), 1);
            var parameters = (Purchases.CustomPaywallImpressionParams)invocation.Arguments[0];
            Assert.That(parameters.PaywallId, Is.EqualTo("paywall_1"));
            Assert.That(parameters.OfferingId, Is.Null);
            Assert.That(parameters.Offering, Is.Null);
        }

        [Test]
        public void TrackCustomPaywallImpressionWithOfferingResolvesOfferingIdAndContext()
        {
            var offering = CreateOfferingWithPackage();

            _purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams(offering));

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackCustomPaywallImpression), 1);
            var parameters = (Purchases.CustomPaywallImpressionParams)invocation.Arguments[0];
            Assert.That(parameters.PaywallId, Is.Null);
            Assert.That(parameters.Offering, Is.SameAs(offering));
            Assert.That(parameters.OfferingId, Is.EqualTo(offering.Identifier));
        }

        [Test]
        public void CustomPaywallImpressionParamsResolvesPresentedOfferingContextFromFirstPackage()
        {
            var offering = CreateOfferingWithPackage();

            var parameters = new Purchases.CustomPaywallImpressionParams(offering);

            Assert.That(parameters.PresentedOfferingContext, Is.Not.Null);
            Assert.That(parameters.PresentedOfferingContext.OfferingIdentifier, Is.EqualTo("default"));
        }

        [Test]
        public void CustomPaywallImpressionParamsHasNullPresentedOfferingContextWhenOfferingHasNoPackages()
        {
            var offering = new Purchases.Offering(JSONNode.Parse(
                "{\"identifier\":\"default\",\"serverDescription\":\"desc\",\"availablePackages\":[]}"));

            var parameters = new Purchases.CustomPaywallImpressionParams(offering);

            Assert.That(parameters.PresentedOfferingContext, Is.Null);
        }

        [Test]
        public void TrackCustomPaywallImpressionWithPaywallIdAndOfferingSetsBoth()
        {
            var offering = CreateOfferingWithPackage();

            _purchases.TrackCustomPaywallImpression(new Purchases.CustomPaywallImpressionParams("paywall_1", offering));

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackCustomPaywallImpression), 1);
            var parameters = (Purchases.CustomPaywallImpressionParams)invocation.Arguments[0];
            Assert.That(parameters.PaywallId, Is.EqualTo("paywall_1"));
            Assert.That(parameters.OfferingId, Is.EqualTo(offering.Identifier));
        }

        [Test]
        [System.Obsolete]
        public void TrackCustomPaywallImpressionDeprecatedCtorUsesExplicitOfferingId()
        {
            _purchases.TrackCustomPaywallImpression(
                new Purchases.CustomPaywallImpressionParams("paywall_1", "offering_1"));

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackCustomPaywallImpression), 1);
            var parameters = (Purchases.CustomPaywallImpressionParams)invocation.Arguments[0];
            Assert.That(parameters.PaywallId, Is.EqualTo("paywall_1"));
            Assert.That(parameters.OfferingId, Is.EqualTo("offering_1"));
            Assert.That(parameters.Offering, Is.Null);
        }

        [Test]
        public void TrackAdDisplayedForwardsDataToWrapper()
        {
            var data = new AdDisplayedData(AdTracker.MediatorName.AdMob, AdTracker.Format.Banner, "unit_1", "impression_1");

            _purchases.AdTracker.TrackAdDisplayed(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdDisplayed), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
        }

        [Test]
        public void TrackAdOpenedForwardsDataToWrapper()
        {
            var data = new AdOpenedData(AdTracker.MediatorName.AppLovin, AdTracker.Format.Interstitial, "unit_1", "impression_1");

            _purchases.AdTracker.TrackAdOpened(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdOpened), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
        }

        [Test]
        public void TrackAdRevenueForwardsDataToWrapper()
        {
            var data = new AdRevenueData(AdTracker.MediatorName.AdMob, AdTracker.Format.Rewarded, "unit_1",
                "impression_1", 1500000L, "USD", AdTracker.Precision.Exact);

            _purchases.AdTracker.TrackAdRevenue(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdRevenue), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
        }

        [Test]
        public void TrackAdLoadedForwardsDataToWrapper()
        {
            var data = new AdLoadedData(AdTracker.MediatorName.AdMob, AdTracker.Format.Native, "unit_1", "impression_1");

            _purchases.AdTracker.TrackAdLoaded(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdLoaded), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
        }

        [Test]
        public void TrackAdFailedToLoadForwardsDataToWrapper()
        {
            var data = new AdFailedToLoadData(AdTracker.MediatorName.AdMob, AdTracker.Format.Banner, "unit_1");

            _purchases.AdTracker.TrackAdFailedToLoad(data);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackAdFailedToLoad), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(data));
        }

        [Test]
        public void AdDisplayedDataToJsonStringOmitsOptionalFieldsWhenNull()
        {
            var data = new AdDisplayedData(AdTracker.MediatorName.AdMob, AdTracker.Format.Banner, "unit_1", "impression_1");

            var json = JSONNode.Parse(data.ToJsonString());

            Assert.That(json["mediatorName"].Value, Is.EqualTo("AdMob"));
            Assert.That(json["adFormat"].Value, Is.EqualTo("banner"));
            Assert.That(json["adUnitId"].Value, Is.EqualTo("unit_1"));
            Assert.That(json["impressionId"].Value, Is.EqualTo("impression_1"));
            Assert.That(json.HasKey("networkName"), Is.False);
            Assert.That(json.HasKey("placement"), Is.False);
        }

        [Test]
        public void AdDisplayedDataToJsonStringIncludesOptionalFieldsWhenPresent()
        {
            var data = new AdDisplayedData(AdTracker.MediatorName.AppLovin, AdTracker.Format.Interstitial, "unit_1",
                "impression_1", "network_1", "placement_1");

            var json = JSONNode.Parse(data.ToJsonString());

            Assert.That(json["networkName"].Value, Is.EqualTo("network_1"));
            Assert.That(json["placement"].Value, Is.EqualTo("placement_1"));
        }

        [Test]
        public void AdRevenueDataToJsonStringIncludesRevenueFields()
        {
            var data = new AdRevenueData(AdTracker.MediatorName.AdMob, AdTracker.Format.Rewarded, "unit_1",
                "impression_1", 1500000L, "USD", AdTracker.Precision.Exact);

            var json = JSONNode.Parse(data.ToJsonString());

            Assert.That(json["revenueMicros"].AsLong, Is.EqualTo(1500000L));
            Assert.That(json["currency"].Value, Is.EqualTo("USD"));
            Assert.That(json["precision"].Value, Is.EqualTo("exact"));
            Assert.That(json.HasKey("networkName"), Is.False);
        }

        [Test]
        public void AdFailedToLoadDataToJsonStringOmitsOptionalFieldsWhenAbsent()
        {
            var data = new AdFailedToLoadData(AdTracker.MediatorName.AdMob, AdTracker.Format.Banner, "unit_1");

            var json = JSONNode.Parse(data.ToJsonString());

            Assert.That(json["mediatorName"].Value, Is.EqualTo("AdMob"));
            Assert.That(json["adFormat"].Value, Is.EqualTo("banner"));
            Assert.That(json["adUnitId"].Value, Is.EqualTo("unit_1"));
            Assert.That(json.HasKey("impressionId"), Is.False);
            Assert.That(json.HasKey("placement"), Is.False);
            Assert.That(json.HasKey("mediatorErrorCode"), Is.False);
        }

        [Test]
        public void AdFailedToLoadDataToJsonStringIncludesOptionalFieldsWhenPresent()
        {
            var data = new AdFailedToLoadData(AdTracker.MediatorName.AdMob, AdTracker.Format.Banner, "unit_1",
                "placement_1", 42);

            var json = JSONNode.Parse(data.ToJsonString());

            Assert.That(json["placement"].Value, Is.EqualTo("placement_1"));
            Assert.That(json["mediatorErrorCode"].AsInt, Is.EqualTo(42));
        }

        private PurchasesWrapperSpy.Invocation AssertLastInvocation(string method, int argumentCount)
        {
            Assert.That(_wrapper.Invocations, Has.Count.EqualTo(1));
            Assert.That(_wrapper.LastInvocation.Method, Is.EqualTo(method));
            Assert.That(_wrapper.LastInvocation.Arguments, Has.Length.EqualTo(argumentCount));
            return _wrapper.LastInvocation;
        }

        private static Purchases.Offering CreateOfferingWithPackage()
        {
            return new Purchases.Offering(JSONNode.Parse(
                "{\"identifier\":\"default\",\"serverDescription\":\"desc\",\"availablePackages\":[" +
                "{\"identifier\":\"$rc_monthly\",\"packageType\":\"MONTHLY\"," +
                "\"product\":{\"title\":\"Monthly\",\"identifier\":\"monthly\"," +
                "\"description\":\"Monthly access\",\"price\":9.99,\"priceString\":\"$9.99\"," +
                "\"currencyCode\":\"USD\",\"productCategory\":\"SUBSCRIPTION\"}," +
                "\"presentedOfferingContext\":{\"offeringIdentifier\":\"default\"}}" +
                "]}"
            ));
        }
    }
}

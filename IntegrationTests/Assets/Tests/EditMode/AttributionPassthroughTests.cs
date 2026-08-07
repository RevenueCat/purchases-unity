using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class AttributionPassthroughTests
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
            // Qualified because `using System` above makes a bare `Object` ambiguous.
            UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        /// <summary>
        /// Every string attribution setter on <see cref="Purchases"/>. Each case carries the wrapper method it is
        /// expected to reach, the call to make, and the value that must arrive unchanged.
        /// </summary>
        private static IEnumerable StringSetters()
        {
            yield return Setter(nameof(IPurchasesWrapper.SetEmail),
                (purchases, value) => purchases.SetEmail(value), "support@revenuecat.com");
            yield return Setter(nameof(IPurchasesWrapper.SetPhoneNumber),
                (purchases, value) => purchases.SetPhoneNumber(value), "+15551234567");
            yield return Setter(nameof(IPurchasesWrapper.SetDisplayName),
                (purchases, value) => purchases.SetDisplayName(value), "Ada Lovelace");
            yield return Setter(nameof(IPurchasesWrapper.SetPushToken),
                (purchases, value) => purchases.SetPushToken(value), "push_token_1");
            yield return Setter(nameof(IPurchasesWrapper.SetAdjustID),
                (purchases, value) => purchases.SetAdjustID(value), "adjust_id_1");
            yield return Setter(nameof(IPurchasesWrapper.SetAppsflyerID),
                (purchases, value) => purchases.SetAppsflyerID(value), "appsflyer_id_1");
            yield return Setter(nameof(IPurchasesWrapper.SetFBAnonymousID),
                (purchases, value) => purchases.SetFBAnonymousID(value), "fb_anonymous_id_1");
            yield return Setter(nameof(IPurchasesWrapper.SetMparticleID),
                (purchases, value) => purchases.SetMparticleID(value), "mparticle_id_1");
            yield return Setter(nameof(IPurchasesWrapper.SetOnesignalID),
                (purchases, value) => purchases.SetOnesignalID(value), "onesignal_id_1");
            yield return Setter(nameof(IPurchasesWrapper.SetOnesignalUserID),
                (purchases, value) => purchases.SetOnesignalUserID(value), "onesignal_user_id_1");
            yield return Setter(nameof(IPurchasesWrapper.SetAirshipChannelID),
                (purchases, value) => purchases.SetAirshipChannelID(value), "airship_channel_1");
            yield return Setter(nameof(IPurchasesWrapper.SetCleverTapID),
                (purchases, value) => purchases.SetCleverTapID(value), "clever_tap_id_1");
            yield return Setter(nameof(IPurchasesWrapper.SetMixpanelDistinctID),
                (purchases, value) => purchases.SetMixpanelDistinctID(value), "mixpanel_distinct_id_1");
            yield return Setter(nameof(IPurchasesWrapper.SetFirebaseAppInstanceID),
                (purchases, value) => purchases.SetFirebaseAppInstanceID(value), "firebase_app_instance_id_1");
            yield return Setter(nameof(IPurchasesWrapper.SetMediaSource),
                (purchases, value) => purchases.SetMediaSource(value), "media_source_1");
            yield return Setter(nameof(IPurchasesWrapper.SetCampaign),
                (purchases, value) => purchases.SetCampaign(value), "campaign_1");
            yield return Setter(nameof(IPurchasesWrapper.SetAdGroup),
                (purchases, value) => purchases.SetAdGroup(value), "ad_group_1");
            yield return Setter(nameof(IPurchasesWrapper.SetAd),
                (purchases, value) => purchases.SetAd(value), "ad_1");
            yield return Setter(nameof(IPurchasesWrapper.SetKeyword),
                (purchases, value) => purchases.SetKeyword(value), "keyword_1");
            yield return Setter(nameof(IPurchasesWrapper.SetCreative),
                (purchases, value) => purchases.SetCreative(value), "creative_1");
        }

        private static TestCaseData Setter(string method, Action<Purchases, string> invoke, string value)
        {
            return new TestCaseData(method, invoke, value)
                .SetName($"{nameof(ForwardsValueToWrapper)}({method})");
        }

        [TestCaseSource(nameof(StringSetters))]
        public void ForwardsValueToWrapper(string method, Action<Purchases, string> invoke, string value)
        {
            invoke(_purchases, value);

            var invocation = AssertOnlyInvocation(method, 1);
            Assert.That(invocation.Arguments[0], Is.EqualTo(value));
        }

        /// <summary>
        /// A null value means "delete this subscriber attribute", so the facade must not filter it out before it
        /// reaches the wrapper. All of the setters above share the same single-statement body, so one representative
        /// case is enough to catch a null guard being introduced.
        /// </summary>
        [Test]
        public void ForwardsNullToWrapper()
        {
            _purchases.SetEmail(null);

            var invocation = AssertOnlyInvocation(nameof(IPurchasesWrapper.SetEmail), 1);
            Assert.That(invocation.Arguments[0], Is.Null);
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

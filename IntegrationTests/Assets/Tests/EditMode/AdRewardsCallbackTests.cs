using System;
using System.Reflection;
using NUnit.Framework;
using RevenueCat.SimpleJSON;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RevenueCat.Tests
{
    public class AdRewardsCallbackTests
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
        public void TearDown() => Object.DestroyImmediate(_gameObject);

        [Test]
        public void OverlappingCallsPreserveExactIdentifiersAndCompleteOutOfOrderOnlyOnce()
        {
            var firstCalls = 0;
            var secondCalls = 0;
            _purchases.GetAdRewards(" Ad-Unit-ID ", (rewards, error) =>
            {
                firstCalls++;
                Assert.That(rewards, Is.Null);
                Assert.That(error, Is.Null);
            });
            _purchases.GetAdRewards(" ad-unit-id ", (rewards, error) =>
            {
                secondCalls++;
                Assert.That(rewards, Is.SameAs(Purchases.ConfiguredAdRewards.NoConfiguredReward));
                Assert.That(error, Is.Null);
            });

            Assert.That(_wrapper.Invocations, Has.Count.EqualTo(2));
            Assert.That(_wrapper.Invocations[0].Method, Is.EqualTo("GetAdRewards"));
            Assert.That(_wrapper.Invocations[0].Arguments[0], Is.EqualTo(" Ad-Unit-ID "));
            Assert.That(_wrapper.Invocations[1].Arguments[0], Is.EqualTo(" ad-unit-id "));
            var firstId = (string)_wrapper.Invocations[0].Arguments[1];
            var secondId = (string)_wrapper.Invocations[1].Arguments[1];
            Assert.That(firstId, Is.Not.EqualTo(secondId));

            Respond(secondId, "{\"reward\":null,\"moreRewards\":[]}");
            Assert.That(firstCalls, Is.Zero);
            Respond(firstId, "null");
            Respond(firstId, "null");
            Respond(secondId, "{}");
            Respond("unrecognized-request", "null");

            Assert.That(firstCalls, Is.EqualTo(1));
            Assert.That(secondCalls, Is.EqualTo(1));
        }

        [Test]
        public void SameAdUnitCanHaveIndependentCallsAndDoesNotReuseThePreviousResult()
        {
            Purchases.ConfiguredAdRewards first = null;
            Purchases.ConfiguredAdRewards second = null;
            _purchases.GetAdRewards("unit", (rewards, error) => first = rewards);
            _purchases.GetAdRewards("unit", (rewards, error) => second = rewards);

            Respond((string)_wrapper.Invocations[1].Arguments[1], "{}");
            Respond((string)_wrapper.Invocations[0].Arguments[1],
                "{\"reward\":{\"type\":\"virtual_currency\",\"code\":\"coins\",\"amount\":10}}");

            Assert.That(first.Reward, Is.TypeOf<Purchases.AdRewardConfiguration.VirtualCurrency>());
            Assert.That(second.Reward, Is.Null);

            var calls = 0;
            _purchases.GetAdRewards("unit", (rewards, error) =>
            {
                calls++;
                Assert.That(rewards, Is.Null);
            });
            Assert.That(calls, Is.Zero);
            Respond((string)_wrapper.LastInvocation.Arguments[1], "null");
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void NativeBridgeErrorsKeepTheirRequestCorrelation()
        {
            var calls = 0;
            _purchases.GetAdRewards("unit", (rewards, error) =>
            {
                calls++;
                Assert.That(rewards, Is.Null);
                Assert.That(error.Code, Is.EqualTo(23));
                Assert.That(error.Message, Is.EqualTo("Unsupported platform"));
            });
            var requestId = (string)_wrapper.LastInvocation.Arguments[1];
            var response = new JSONObject();
            response["requestId"] = requestId;
            response["error"] = JSONNode.Parse("{\"code\":23,\"message\":\"Unsupported platform\"}");
            Send(response.ToString());
            Send(response.ToString());

            Assert.That(calls, Is.EqualTo(1));
        }

        [TestCase(null)]
        [TestCase("")]
        public void EmptyIdentifiersCompleteAsUnavailableWithoutCallingNative(string adUnitId)
        {
            LogAssert.Expect(LogType.Warning, "[Purchases] GetAdRewards requires a non-empty ad unit identifier.");
            var calls = 0;
            _purchases.GetAdRewards(adUnitId, (rewards, error) =>
            {
                calls++;
                Assert.That(rewards, Is.Null);
                Assert.That(error, Is.Null);
            });

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(_wrapper.Invocations, Is.Empty);
        }

        [Test]
        public void NullCallbackIsRejectedBeforeMakingANativeRequest()
        {
            Assert.Throws<ArgumentNullException>(() => _purchases.GetAdRewards("unit", null));
            Assert.That(_wrapper.Invocations, Is.Empty);
        }

        [Test]
        public void WrapperWithoutTheOptionalCapabilityCompletesAsUnavailable()
        {
            var type = typeof(Purchases).GetNestedType("PurchasesWrapperNoop", BindingFlags.NonPublic);
            _purchases.SetWrapper((IPurchasesWrapper)Activator.CreateInstance(type, true));
            var calls = 0;
            _purchases.GetAdRewards("unit", (rewards, error) =>
            {
                calls++;
                Assert.That(rewards, Is.Null);
                Assert.That(error, Is.Null);
            });

            Assert.That(calls, Is.EqualTo(1));
        }

        private void Respond(string requestId, string rewards)
        {
            var response = new JSONObject();
            response["requestId"] = requestId;
            response["rewards"] = JSONNode.Parse(rewards);
            Send(response.ToString());
        }

        private void Send(string json) => typeof(Purchases)
            .GetMethod("_getAdRewards", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(_purchases, new object[] { json });
    }
}

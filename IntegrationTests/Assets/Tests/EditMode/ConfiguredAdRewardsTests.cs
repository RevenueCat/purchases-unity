using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class ConfiguredAdRewardsTests
    {
        private const string Coins = "{\"type\":\"virtual_currency\",\"code\":\"coins\",\"amount\":10}";
        private const string Entitlement =
            "{\"type\":\"entitlement\",\"identifier\":\"pro\",\"durationSeconds\":1800}";

        [Test]
        public void MapsPrimaryAndOrderedAdditionalRewardsFromHybridCommon()
        {
            // Captured from the published Hybrid Common 20.8.0 mapper using Android 11.2.1's
            // ConfiguredAdRewards(rewards) constructor, including its derived primary/extras.
            var result = LoadReleasedFixture("configured-ad-rewards.json");

            var currency = (Purchases.AdRewardConfiguration.VirtualCurrency)result.Reward;
            Assert.That(currency.Code, Is.EqualTo("coins"));
            Assert.That(currency.Amount, Is.EqualTo(10));
            Assert.That(result.MoreRewards, Has.Count.EqualTo(3));
            var entitlement = (Purchases.AdRewardConfiguration.Entitlement)result.MoreRewards[0];
            Assert.That(entitlement.Identifier, Is.EqualTo("pro"));
            Assert.That(entitlement.Duration, Is.EqualTo(TimeSpan.FromMinutes(30)));
            Assert.That(result.MoreRewards[1], Is.SameAs(Purchases.AdRewardConfiguration.Unsupported.Instance));
            Assert.That(result.MoreRewards[2], Is.TypeOf<Purchases.AdRewardConfiguration.VirtualCurrency>());
        }

        [Test]
        public void EmptyNativeRewardsListMapsToResolvedAbsence()
        {
            var result = LoadReleasedFixture("configured-ad-rewards-none.json");

            Assert.That(result, Is.SameAs(Purchases.ConfiguredAdRewards.NoConfiguredReward));
            Assert.That(result.Reward, Is.Null);
            Assert.That(result.MoreRewards, Is.Empty);
        }

        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("42")]
        [TestCase("{\"reward\":null,\"moreRewards\":42}")]
        [TestCase("{\"reward\":null,\"moreRewards\":{}}")]
        public void UnavailableOrMalformedResultsRemainUnavailable(string json)
        {
            Assert.That(Map(json), Is.Null);
            Assert.That(Purchases.ConfiguredAdRewards.FromJson(null), Is.Null);
        }

        [TestCase("{}")]
        [TestCase("{\"reward\":null}")]
        [TestCase("{\"reward\":{\"type\":\"no_reward\"},\"moreRewards\":[]}")]
        [TestCase("{\"reward\":null,\"moreRewards\":null}")]
        [TestCase("{\"moreRewards\":[null,{\"type\":\"no_reward\"}]}")]
        public void ResolvedAbsenceIsDifferentFromUnavailableConfiguration(string json)
        {
            var result = Map(json);

            Assert.That(result, Is.SameAs(Purchases.ConfiguredAdRewards.NoConfiguredReward));
            Assert.That(result.Reward, Is.Null);
            Assert.That(result.MoreRewards, Is.Empty);
        }

        [TestCase("")]
        [TestCase("\"reward\":null,")]
        [TestCase("\"reward\":{\"type\":\"no_reward\"},")]
        public void AdditionalRewardsRequireAnActualPrimaryReward(string primary)
        {
            Assert.That(Map("{" + primary + "\"moreRewards\":[" + Coins + "]}"), Is.Null);
            Assert.That(Map("{" + primary + "\"moreRewards\":[{\"type\":\"future_reward\"}]}"), Is.Null);
        }

        [Test]
        public void NullAndNoRewardAdditionalEntriesDoNotChangeRewardOrder()
        {
            var result = Map("{\"reward\":" + Coins + ",\"moreRewards\":[null," + Entitlement +
                             ",{\"type\":\"no_reward\"}," + Coins + "]}");

            Assert.That(result.MoreRewards, Has.Count.EqualTo(2));
            Assert.That(result.MoreRewards[0], Is.TypeOf<Purchases.AdRewardConfiguration.Entitlement>());
            Assert.That(result.MoreRewards[1], Is.TypeOf<Purchases.AdRewardConfiguration.VirtualCurrency>());
        }

        [TestCase("{\"type\":\"unsupported_reward\"}")]
        [TestCase("{\"type\":\"future_reward\",\"newField\":true}")]
        [TestCase("{\"type\":\"virtual_currency\",\"code\":\"\",\"amount\":10}")]
        [TestCase("{\"type\":\"virtual_currency\",\"code\":\"coins\",\"amount\":0}")]
        [TestCase("{\"type\":\"virtual_currency\",\"code\":\"coins\",\"amount\":-1}")]
        [TestCase("{\"type\":\"virtual_currency\",\"code\":\"coins\",\"amount\":1.5}")]
        [TestCase("{\"type\":\"virtual_currency\",\"code\":\"coins\",\"amount\":2147483648}")]
        [TestCase("{\"type\":\"virtual_currency\",\"code\":\"coins\",\"amount\":\"10\"}")]
        [TestCase("{\"type\":\"entitlement\",\"identifier\":\"pro\",\"durationSeconds\":0}")]
        [TestCase("{\"type\":\"entitlement\",\"identifier\":\"pro\",\"durationSeconds\":-1}")]
        [TestCase("{\"type\":\"entitlement\",\"identifier\":\"pro\",\"durationSeconds\":1e20}")]
        [TestCase("{\"type\":\"entitlement\",\"identifier\":\"pro\",\"durationSeconds\":\"1800\"}")]
        [TestCase("{\"type\":\"entitlement\",\"identifier\":\"\",\"durationSeconds\":1800}")]
        [TestCase("{}")]
        [TestCase("42")]
        public void UnknownAndUnrepresentableRewardsStayInTheirOriginalPosition(string reward)
        {
            var result = Map("{\"reward\":" + reward + ",\"moreRewards\":[" + Coins + "]}");

            Assert.That(result.Reward, Is.SameAs(Purchases.AdRewardConfiguration.Unsupported.Instance));
            Assert.That(result.MoreRewards, Has.Count.EqualTo(1));
        }

        [Test]
        public void EntitlementDurationUsesTimeSpanAndRetainsFractionalSecondsFromTheBridge()
        {
            var result = Map("{\"reward\":{\"type\":\"entitlement\",\"identifier\":\"pro\",\"durationSeconds\":1.5}}");

            Assert.That(((Purchases.AdRewardConfiguration.Entitlement)result.Reward).Duration,
                Is.EqualTo(TimeSpan.FromMilliseconds(1500)));
        }

        [Test]
        public void AdditionalRewardsCannotBeMutated()
        {
            var result = Purchases.ConfiguredAdRewards.NoConfiguredReward;

            Assert.Throws<NotSupportedException>(() =>
                ((IList<Purchases.AdRewardConfiguration>)result.MoreRewards)
                .Add(Purchases.AdRewardConfiguration.Unsupported.Instance));
            Assert.That(result.MoreRewards, Is.Empty);
        }

        private static Purchases.ConfiguredAdRewards Map(string json) =>
            Purchases.ConfiguredAdRewards.FromJson(JSONNode.Parse(json));

        private static Purchases.ConfiguredAdRewards LoadReleasedFixture(string filename) =>
            Map(File.ReadAllText(Path.Combine(Application.dataPath, "Tests", "EditMode", "JsonModelFixtures", filename)));
    }
}

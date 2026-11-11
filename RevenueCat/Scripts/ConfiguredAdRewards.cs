using System.Collections.Generic;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// Rewards configured for an ad unit. A null result from <see cref="GetAdRewards"/> means
    /// configuration is unavailable. A non-null result with a null <see cref="Reward"/> means
    /// configuration loaded successfully but no reward is configured for that ad unit.
    /// </summary>
    public sealed class ConfiguredAdRewards
    {
        /// <summary> No reward is configured for this ad unit. </summary>
        public static ConfiguredAdRewards NoConfiguredReward { get; } =
            new ConfiguredAdRewards(null, new List<AdRewardConfiguration>());

        /// <summary> The primary configured reward, or null when no reward is configured. </summary>
        public readonly AdRewardConfiguration Reward;

        /// <summary> Additional rewards in configured order. Empty when <see cref="Reward"/> is null. </summary>
        public readonly IReadOnlyList<AdRewardConfiguration> MoreRewards;

        private ConfiguredAdRewards(AdRewardConfiguration reward, List<AdRewardConfiguration> moreRewards)
        {
            Reward = reward;
            MoreRewards = moreRewards.AsReadOnly();
        }

        /// <summary>
        /// Maps a resolved purchases-hybrid-common result. Returns null for unavailable or malformed data.
        /// This does not parse remote configuration; resolution and validation belong to the native SDK.
        /// </summary>
        public static ConfiguredAdRewards FromJson(JSONNode response)
        {
            if (response == null || !response.IsObject) return null;

            var additional = response["moreRewards"];
            if (additional != null && !additional.IsNull && !additional.IsArray) return null;

            var moreRewards = new List<AdRewardConfiguration>();
            foreach (JSONNode node in additional)
            {
                var reward = AdRewardConfiguration.FromJson(node);
                if (reward != null) moreRewards.Add(reward);
            }

            var primary = AdRewardConfiguration.FromJson(response["reward"]);
            if (primary == null) return moreRewards.Count == 0 ? NoConfiguredReward : null;

            return new ConfiguredAdRewards(primary, moreRewards);
        }

        public override string ToString() =>
            $"{nameof(Reward)}: {Reward?.ToString() ?? "null"}, " +
            $"{nameof(MoreRewards)}: [{string.Join(", ", MoreRewards)}]";
    }
}

using System.Collections.Generic;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;

public partial class Purchases
{
    /// <summary>
    /// Result of polling for reward verification. A single ad can grant multiple rewards.
    /// </summary>
    /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
    public class RewardVerificationResult
    {
        /// <summary>
        /// The primary reward when verification succeeded; null on failure.
        /// </summary>
        public readonly VerifiedReward Reward;

        /// <summary>
        /// Additional rewards granted alongside the primary; never repeats it; empty on failure.
        /// </summary>
        public readonly List<VerifiedReward> MoreRewards;

        /// <summary>
        /// True when verification did not complete (rejected / timeout / network).
        /// </summary>
        public readonly bool Failed;

        public RewardVerificationResult(JSONNode response)
        {
            Failed = response["failed"].AsBool;
            MoreRewards = new List<VerifiedReward>();

            if (Failed) return;

            if (response["reward"] != null && !response["reward"].IsNull)
            {
                Reward = new VerifiedReward(response["reward"]);
            }

            foreach (JSONNode rewardNode in response["moreRewards"].AsArray)
            {
                MoreRewards.Add(new VerifiedReward(rewardNode));
            }
        }

        public override string ToString()
        {
            return $"{nameof(Failed)}: {Failed}, " +
                   $"{nameof(Reward)}: {(Reward == null ? "null" : Reward.ToString())}, " +
                   $"{nameof(MoreRewards)}:\n{ListToString(MoreRewards)}";
        }
    }
}

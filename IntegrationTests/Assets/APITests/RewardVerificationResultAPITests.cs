using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class RewardVerificationResultAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.RewardVerificationResult result = new Purchases.RewardVerificationResult(null);
            bool failed = result.Failed;
            Purchases.VerifiedReward reward = result.Reward;
            List<Purchases.VerifiedReward> moreRewards = result.MoreRewards;
        }
    }
}

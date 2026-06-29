using UnityEngine;

namespace DefaultNamespace
{
    public class VerifiedRewardAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.VerifiedReward reward = new Purchases.VerifiedReward(null);
            Purchases.VerifiedReward.RewardType type = reward.Type;
            string code = reward.Code;
            int amount = reward.Amount;
            string identifier = reward.Identifier;
            string expiresAt = reward.ExpiresAt;
            long expiresAtMillis = reward.ExpiresAtMillis;

            switch (type)
            {
                case Purchases.VerifiedReward.RewardType.VirtualCurrency:
                case Purchases.VerifiedReward.RewardType.Entitlement:
                case Purchases.VerifiedReward.RewardType.NoReward:
                case Purchases.VerifiedReward.RewardType.Unsupported:
                    break;
            }
        }
    }
}

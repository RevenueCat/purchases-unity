using UnityEngine;

namespace DefaultNamespace
{
    public class VerifiedRewardAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.VerifiedReward reward = Purchases.VerifiedReward.FromJson(null);

            switch (reward)
            {
                case Purchases.VerifiedReward.VirtualCurrency virtualCurrency:
                    string code = virtualCurrency.Code;
                    int amount = virtualCurrency.Amount;
                    break;
                case Purchases.VerifiedReward.Entitlement entitlement:
                    string identifier = entitlement.Identifier;
                    string expiresAt = entitlement.ExpiresAt;
                    long expiresAtMillis = entitlement.ExpiresAtMillis;
                    break;
                case Purchases.VerifiedReward.NoReward noReward:
                    break;
                case Purchases.VerifiedReward.Unsupported unsupported:
                    break;
            }
        }
    }
}

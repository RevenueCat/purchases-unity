using UnityEngine;

namespace DefaultNamespace
{
    public class RewardVerificationTokenAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.RewardVerificationToken token = new Purchases.RewardVerificationToken(null);
            string customData = token.CustomData;
            string clientTransactionId = token.ClientTransactionId;
            string appUserID = token.AppUserID;
        }
    }
}

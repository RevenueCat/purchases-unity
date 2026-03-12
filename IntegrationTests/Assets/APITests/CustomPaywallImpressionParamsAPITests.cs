using UnityEngine;

namespace DefaultNamespace
{
    public class CustomPaywallImpressionParamsAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.CustomPaywallImpressionParams paramsWithId =
                new Purchases.CustomPaywallImpressionParams("my_paywall_id");
            string paywallId = paramsWithId.PaywallId;

            Purchases.CustomPaywallImpressionParams paramsWithoutId =
                new Purchases.CustomPaywallImpressionParams();
            string nullPaywallId = paramsWithoutId.PaywallId;
        }
    }
}

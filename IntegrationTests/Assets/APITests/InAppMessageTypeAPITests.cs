using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class InAppMessageTypeAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.InAppMessageType inAppMessageType = Purchases.InAppMessageType.Generic;
            switch (inAppMessageType)
            {
                case Purchases.InAppMessageType.BillingIssue:
                case Purchases.InAppMessageType.PriceIncreaseConsent:
                case Purchases.InAppMessageType.Generic:
                case Purchases.InAppMessageType.WinBackOffer:
                    break;
            }
        }
    }
}

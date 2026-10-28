using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class BillingPlanTypeAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.BillingPlanType billingPlanType = Purchases.BillingPlanType.UP_FRONT;

            switch (billingPlanType)
            {
                case Purchases.BillingPlanType.UP_FRONT:
                case Purchases.BillingPlanType.MONTHLY:
                case Purchases.BillingPlanType.UNKNOWN:
                    break;
            }
        }
    }
}

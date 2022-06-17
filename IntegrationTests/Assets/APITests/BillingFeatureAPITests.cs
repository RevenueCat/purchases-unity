using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class BillingFeatureAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.BillingFeature feature = Purchases.BillingFeature.Subscriptions;

            switch (feature)
            {
                case Purchases.BillingFeature.Subscriptions:
                case Purchases.BillingFeature.SubscriptionsUpdate:
                case Purchases.BillingFeature.PriceChangeConfirmation:
                case Purchases.BillingFeature.SubscriptionsOnVR:
                case Purchases.BillingFeature.InAppItemsOnVR:
                    break;
            }
        }
    }
}
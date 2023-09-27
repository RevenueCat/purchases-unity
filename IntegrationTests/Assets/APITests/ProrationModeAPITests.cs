using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class ProrationModeAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.ProrationMode prorationMode = Purchases.ProrationMode.ImmediateWithoutProration;
            switch (prorationMode)
            {
                case Purchases.ProrationMode.ImmediateWithoutProration:
                case Purchases.ProrationMode.ImmediateWithTimeProration:
                case Purchases.ProrationMode.ImmediateAndChargeProratedPrice:
                case Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy:
                case Purchases.ProrationMode.ImmediateAndChargeFullPrice:
                    break;
            }
        }
    }
}

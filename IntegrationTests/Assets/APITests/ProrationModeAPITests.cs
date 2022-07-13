using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class ProrationModeAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.ProrationMode prorationMode = Purchases.ProrationMode.Deferred;
            switch (prorationMode)
            {
                case Purchases.ProrationMode.Deferred:
                case Purchases.ProrationMode.ImmediateWithoutProration:
                case Purchases.ProrationMode.ImmediateWithTimeProration:
                case Purchases.ProrationMode.ImmediateAndChargeProratedPrice:
                case Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy:
                    break;
            }
        }
    }
}
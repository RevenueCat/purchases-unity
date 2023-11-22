using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class EntitlementVerificationModeAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.EntitlementVerificationMode entitlementVerificationMode = Purchases.EntitlementVerificationMode.Disabled;
            switch (entitlementVerificationMode)
            {
                case Purchases.EntitlementVerificationMode.Disabled:
                case Purchases.EntitlementVerificationMode.Informational:
                    break;
            }
        }
    }
}

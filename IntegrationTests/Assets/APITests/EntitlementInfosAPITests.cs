using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class EntitlementInfosAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.EntitlementInfos entitlementInfos = new Purchases.EntitlementInfos(null);
            
            Dictionary<string, Purchases.EntitlementInfo> all = entitlementInfos.All;
            Dictionary<string, Purchases.EntitlementInfo> active = entitlementInfos.Active;
            VerificationResult verification = entitlementInfos.Verification;
        }
    }
}
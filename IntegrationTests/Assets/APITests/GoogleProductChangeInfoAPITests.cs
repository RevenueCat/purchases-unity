using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class GoogleProductChangeInfoAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.GoogleProductChangeInfo googleProductChangeInfo = new Purchases.GoogleProductChangeInfo("aaa", Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy);
            string OldProductIdentifier = googleProductChangeInfo.OldProductIdentifier;
            Purchases.ProrationMode ProrationMode = googleProductChangeInfo.ProrationMode;
        }
    }
}
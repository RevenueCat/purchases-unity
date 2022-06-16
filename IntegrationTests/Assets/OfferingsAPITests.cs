using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class OfferingsAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.Offerings offerings = new Purchases.Offerings(null);
            Dictionary<string, Purchases.Offering> all = offerings.All;
            Purchases.Offering current = offerings.Current;
        }
    }
}
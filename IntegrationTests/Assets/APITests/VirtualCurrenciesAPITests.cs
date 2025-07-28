using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class VirtualCurrenciesAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.VirtualCurrencies virtualCurrencies = new Purchases.VirtualCurrencies(null);
            Dictionary<string, Purchases.VirtualCurrency> all = virtualCurrencies.All;
        }
    }
}

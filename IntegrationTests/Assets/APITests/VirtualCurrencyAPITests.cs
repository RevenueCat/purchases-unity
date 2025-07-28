using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class VirtualCurrencyAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.VirtualCurrency virtualCurrency = new Purchases.VirtualCurrency(null);
            int balance = virtualCurrency.Balance;
            string name = virtualCurrency.Name;
            string code = virtualCurrency.Code;
            string? serverDescription = virtualCurrency.ServerDescription;
        }
    }
}

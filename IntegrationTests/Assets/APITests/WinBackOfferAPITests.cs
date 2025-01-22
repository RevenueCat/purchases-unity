using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class WinBackOfferAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.WinBackOffer winBackOffer = new Purchases.WinBackOffer(null);
            string identifier = winBackOffer.Identifier;
            float price = winBackOffer.Price;
            string priceString = winBackOffer.PriceString;
            int cycles = winBackOffer.Cycles;
            string period = winBackOffer.Period;
            string periodUnit = winBackOffer.PeriodUnit;
            int periodNumberOfUnits = winBackOffer.PeriodNumberOfUnits;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class OfferingAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.Offering offering = new Purchases.Offering(null);
            string Identifier = offering.Identifier;
            string ServerDescription = offering.ServerDescription;
            List<Purchases.Package> AvailablePackages = offering.AvailablePackages;
            Purchases.Package Lifetime = offering.Lifetime;
            Purchases.Package Annual = offering.Annual;
            Purchases.Package SixMonth = offering.SixMonth;
            Purchases.Package ThreeMonth = offering.ThreeMonth;
            Purchases.Package TwoMonth = offering.TwoMonth;
            Purchases.Package Monthly = offering.Monthly;
            Purchases.Package Weekly = offering.Weekly;
            string WebCheckoutUrl = offering.WebCheckoutUrl;
        }
    }
}
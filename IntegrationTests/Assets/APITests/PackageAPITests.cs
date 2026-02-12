using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class PackageAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.Package package = new Purchases.Package(null);
            string identifier = package.Identifier;
            string packageType = package.PackageType;
            Purchases.StoreProduct storeProduct = package.StoreProduct;
            string offeringIdentifier = package.OfferingIdentifier;
            string webCheckoutUrl = package.WebCheckoutUrl;
        }
    }
}
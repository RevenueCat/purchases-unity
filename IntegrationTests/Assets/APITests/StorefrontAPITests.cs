using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class StorefrontAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.Storefront storefront = new Purchases.Storefront(null);
            string countryCode = storefront.CountryCode;
        }
    }
}

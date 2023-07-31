using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class ProductCategoryAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.ProductCategory productCategory = Purchases.ProductCategory.NON_SUBSCRIPTION;

            switch (productCategory)
            {
                case Purchases.ProductCategory.NON_SUBSCRIPTION:
                case Purchases.ProductCategory.SUBSCRIPTION:
                case Purchases.ProductCategory.UNKNOWN:
                    break;
            }
        }
    }
}
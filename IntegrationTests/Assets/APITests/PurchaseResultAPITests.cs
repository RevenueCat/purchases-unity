using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class PurchaseResultAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.PurchaseResult purchaseResult = new Purchases.PurchaseResult(null);
            Purchases.StoreTransaction storeTransaction = purchaseResult.StoreTransaction;
            Purchases.CustomerInfo customerInfo = purchaseResult.CustomerInfo;
            bool userCancelled = purchaseResult.UserCancelled;
            Purchases.Error error = purchaseResult.Error;
            string productIdentifier = purchaseResult.ProductIdentifier;
        }
    }
}
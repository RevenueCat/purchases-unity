using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class StoreTransactionAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.StoreTransaction storeTransaction = new Purchases.StoreTransaction(null);
            string transactionIdentifier = storeTransaction.TransactionIdentifier;
            string revenueCatId = storeTransaction.RevenueCatId;
            string productIdentifier = storeTransaction.ProductIdentifier;
            string productId = storeTransaction.ProductId;
            DateTime purchaseDate = storeTransaction.PurchaseDate;
        }
    }
}
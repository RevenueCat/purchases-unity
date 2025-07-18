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
            string productIdentifier = storeTransaction.ProductIdentifier;
            DateTime purchaseDate = storeTransaction.PurchaseDate;
        }
    }
}
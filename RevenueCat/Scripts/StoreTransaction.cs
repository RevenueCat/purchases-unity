using System;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;


public partial class Purchases
{
    /// <summary>
    /// Abstract class that provides access to properties of a transaction. StoreTransactions can represent
    /// transactions from StoreKit 1, StoreKit 2 or transactions made from other places,
    /// like Stripe, Google Play or Amazon Store.
    /// </summary>
    public class StoreTransaction
    {
        /**
         * <summary>
         * Id associated with the transaction in RevenueCat.
         * </summary>
         */
        public readonly string TransactionIdentifier;

        /**
         * <summary>
         * Product Id associated with the transaction.
         * </summary>
         */
        public readonly string ProductIdentifier;

        /**
         * <summary>
         * Purchase date of the transaction in UTC, be sure to compare them with DateTime.UtcNow
         * </summary>
         */
        public readonly DateTime PurchaseDate;

        public StoreTransaction(JSONNode response)
        {
            TransactionIdentifier = response["transactionIdentifier"];
            ProductIdentifier = response["productIdentifier"];
            PurchaseDate = FromUnixTimeInMilliseconds(response["purchaseDateMillis"].AsLong);
        }

        public override string ToString()
        {
            return $"{nameof(TransactionIdentifier)}: {TransactionIdentifier}\n" +
                   $"{nameof(ProductIdentifier)}: {ProductIdentifier}\n" +
                   $"{nameof(PurchaseDate)}: {PurchaseDate}";
        }
    }
}
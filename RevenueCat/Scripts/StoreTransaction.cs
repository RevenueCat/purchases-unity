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

        /**
         * <summary>
         * The original purchase JSON as a string. Android only, null on iOS and some stores.
         * </summary>
         */
        public readonly string OriginalJson;

        /**
         * <summary>
         * The purchase signature. Google Play only, null on iOS and other stores.
         * </summary>
         */
        public readonly string Signature;

        public StoreTransaction(JSONNode response)
        {
            TransactionIdentifier = response["transactionIdentifier"];
            ProductIdentifier = response["productIdentifier"];
            PurchaseDate = FromUnixTimeInMilliseconds(response["purchaseDateMillis"].AsLong);
            OriginalJson = response["originalJson"];
            Signature = response["signature"];
        }

        public override string ToString()
        {
            return $"{nameof(TransactionIdentifier)}: {TransactionIdentifier}\n" +
                   $"{nameof(ProductIdentifier)}: {ProductIdentifier}\n" +
                   $"{nameof(PurchaseDate)}: {PurchaseDate}\n" +
                   $"{nameof(OriginalJson)}: {OriginalJson}\n" +
                   $"{nameof(Signature)}: {Signature}";
        }
    }
}
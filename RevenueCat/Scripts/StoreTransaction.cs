using Newtonsoft.Json;
using System;
using static RevenueCat.Utilities;

namespace RevenueCat
{
    /// <summary>
    /// Abstract class that provides access to properties of a transaction. StoreTransactions can represent
    /// transactions from StoreKit 1, StoreKit 2 or transactions made from other places,
    /// like Stripe, Google Play or Amazon Store.
    /// </summary>
    public class StoreTransaction
    {
        /// <summary>
        /// Id associated with the transaction in RevenueCat.
        /// </summary>
        [JsonProperty("transactionIdentifier")]
        public string TransactionIdentifier { get; }

        /// <summary>
        /// Product Id associated with the transaction.
        /// </summary>
        [JsonProperty("productIdentifier")]
        public string ProductIdentifier { get; }

        [JsonProperty("purchaseDate")]
        public long PurchaseDateUnixTimeMilliseconds { get; }

        /// <summary>
        /// Purchase date of the transaction in UTC, be sure to compare them with DateTime.UtcNow
        /// </summary>
        [JsonIgnore]
        public DateTime PurchaseDate { get; }

        [JsonConstructor]
        internal StoreTransaction(
            [JsonProperty("transactionIdentifier")] string transactionIdentifier,
            [JsonProperty("productIdentifier")] string productIdentifier,
            [JsonProperty("purchaseDate")] long purchaseDate)
        {
            TransactionIdentifier = transactionIdentifier;
            ProductIdentifier = productIdentifier;
            PurchaseDateUnixTimeMilliseconds = purchaseDate;
            PurchaseDate = FromUnixTimeInMilliseconds(purchaseDate);
        }

        public override string ToString()
        {
            return $"{nameof(TransactionIdentifier)}: {TransactionIdentifier}\n" +
                   $"{nameof(ProductIdentifier)}: {ProductIdentifier}\n" +
                   $"{nameof(PurchaseDate)}: {PurchaseDate}";
        }
    }
}
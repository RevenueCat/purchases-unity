using JetBrains.Annotations;
using Newtonsoft.Json;

namespace RevenueCat
{
    /// <summary>
    /// Class containing the data of the result of a purchase.
    /// </summary>
    public sealed class PurchaseResult
    {
        /// <summary>
        /// The updated <see cref="CustomerInfo"/> object after the successful purchase.
        /// </summary>
        [JsonProperty("customerInfo")]
        public  CustomerInfo CustomerInfo;

        /// <summary>
        /// The product identifier for which the purchase was attempted.
        /// </summary>
        [JsonIgnore]
        public  string ProductIdentifier;

        /// <summary>
        /// A boolean that indicates whether the purchase was cancelled by the user.
        /// </summary>
        public  bool UserCancelled;

        /// <summary>
        /// An error, if one occurred.Null if the purchase was successful.
        /// </summary>
        [CanBeNull]
        public  Error Error;

        /// <summary>
        /// The<see cref = "StoreTransaction" /> object for the purchase that just happened.Null if the purchase was not successful.
        /// </summary>
        public  StoreTransaction StoreTransaction;

        [JsonConstructor]
        internal PurchaseResult(CustomerInfo customerInfo, bool userCancelled, Error error, StoreTransaction storeTransaction)
        {
            CustomerInfo = customerInfo;
            StoreTransaction = storeTransaction;
            ProductIdentifier = storeTransaction.ProductIdentifier;
            UserCancelled = userCancelled;
            Error = error;
        }

        public override string ToString()
        {
            return $"{nameof(CustomerInfo)}: {CustomerInfo}\n" +
                   $"{nameof(ProductIdentifier)}: {ProductIdentifier}\n" +
                   $"{nameof(UserCancelled)}: {UserCancelled}\n" +
                   $"{nameof(Error)}: {Error}\n" +
                   $"{nameof(StoreTransaction)}: {StoreTransaction}";
        }
    }
}
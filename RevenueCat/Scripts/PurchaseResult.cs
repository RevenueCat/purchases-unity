using System;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;


public partial class Purchases
{
    /// <summary>
    /// Class containing the data of the result of a purchase.
    /// </summary>
    public class PurchaseResult
    {
        /**
         * <summary>
         * The updated <see cref="CustomerInfo"/> object after the successful purchase.
         * </summary>
         */
        public readonly CustomerInfo CustomerInfo;

        /**
         * <summary>
         * The product identifier for which the purchase was attempted.
         * </summary>
         */
        public readonly string ProductIdentifier;

        /**
         * <summary>
         * A boolean that indicates whether the purchase was cancelled by the user.
         * </summary>
         */
        public readonly bool UserCancelled;

        /**
         * <summary>
         * An error, if one occurred. Null if the purchase was successful.
         * </summary>
         */
        public readonly Error Error;

        /**
         * <summary>
         * The <see cref="StoreTransaction"/> object for the purchase that just happened. Null if the purchase was not successful.
         * </summary>
         */
        public readonly StoreTransaction StoreTransaction;

        public PurchaseResult(JSONNode response)
        {
            if (response["customerInfo"] != null)
            {
                CustomerInfo = new CustomerInfo(response["customerInfo"]);
            }

            if (response["transaction"] != null)
            {
                StoreTransaction = new StoreTransaction(response["transaction"]);
                ProductIdentifier = StoreTransaction.ProductIdentifier;
            }

            if (response["userCancelled"] != null)
            {
                UserCancelled = response["userCancelled"].AsBool;
            }

            if (response["error"] != null)
            {
                Error = new Error(response["error"]);
            }
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
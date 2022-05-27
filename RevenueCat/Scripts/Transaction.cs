using System;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;


public partial class Purchases
{    
    public class StoreTransaction
    {
        /**
         * <summary>
         * Id associated with the transaction in RevenueCat.
         * </summary>
         */
        public string RevenueCatId;
        /**
         * <summary>
         * Product Id associated with the transaction.
         * </summary>
         */
        public string ProductId;
        /**
         * <summary>
         * Purchase date of the transaction in UTC, be sure to compare them with DateTime.UtcNow
         * </summary>
         */
        public DateTime PurchaseDate;

        public StoreTransaction(JSONNode response)
        {
            RevenueCatId = response["revenueCatId"];
            ProductId = response["productId"];
            PurchaseDate = FromUnixTimeInMilliseconds(response["purchaseDateMillis"].AsLong);
        } 
        
        public override string ToString()
        {
            return $"{nameof(RevenueCatId)}: {RevenueCatId}, " +
                   $"{nameof(ProductId)}: {ProductId}, " +
                   $"{nameof(PurchaseDate)}: {PurchaseDate}";
        }
    }
}
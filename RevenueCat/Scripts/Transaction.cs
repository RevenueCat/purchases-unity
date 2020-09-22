using System;
using RevenueCat.SimpleJSON;

public partial class Purchases
{    
    public class Transaction
    {
        /*
         * RevenueCat Id associated to the transaction.
         */
        public string RevenueCatId;
        /*
         * Product Id associated with the transaction.
         */
        public string ProductId;
        /*
         * Purchase date of the transaction in UTC, be sure to compare them with DateTime.UtcNow
         */
        public DateTime PurchaseDate;

        public Transaction(JSONNode response)
        {
            RevenueCatId = response["revenuecatId"];
            ProductId = response["productId"];
            PurchaseDate = Utilities.FromUnixTime(response["purchaseDateMillis"].AsLong);
        } 
        
        public override string ToString()
        {
            return $"{nameof(RevenueCatId)}: {RevenueCatId}, " +
                   $"{nameof(ProductId)}: {ProductId}, " +
                   $"{nameof(PurchaseDate)}: {PurchaseDate}";
        }
    }
}
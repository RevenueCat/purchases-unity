using System.Collections.Generic;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class Package
    {
        public readonly string Identifier;
        public readonly string PackageType;
        public readonly StoreProduct StoreProduct;
        public readonly string OfferingIdentifier;

        public Package(JSONNode response)
        {
            Identifier = response["identifier"];
            PackageType = response["packageType"];
            StoreProduct = new StoreProduct(response["product"]);
            OfferingIdentifier = response["offeringIdentifier"];
        }

        public override string ToString()
        {
            return $"{nameof(Identifier)}: {Identifier}, " +
                   $"{nameof(PackageType)}: {PackageType}, " +
                   $"{nameof(StoreProduct)}: {StoreProduct}, " +
                   $"{nameof(OfferingIdentifier)}: {OfferingIdentifier}";
        }
    }
}
using System.Collections.Generic;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class Package
    {
        public readonly string Identifier;
        public readonly string PackageType;
        public readonly Product Product;
        public readonly string OfferingIdentifier;

        public Package(JSONNode response)
        {
            Identifier = response["identifier"];
            PackageType = response["packageType"];
            Product = new Product(response["product"]);
            OfferingIdentifier = response["offeringIdentifier"];
        }

        public override string ToString()
        {
            return $"{nameof(Identifier)}: {Identifier}, " +
                   $"{nameof(PackageType)}: {PackageType}, " +
                   $"{nameof(Product)}: {Product}, " +
                   $"{nameof(OfferingIdentifier)}: {OfferingIdentifier}";
        }
    }
}
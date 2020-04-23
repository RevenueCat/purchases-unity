public partial class Purchases
{
    public class Package
    {
        public readonly string Identifier;
        public readonly string PackageType;
        public readonly Product Product;
        public readonly string OfferingIdentifier;

        public Package(PackageResponse response)
        {
            Identifier = response.identifier;
            PackageType = response.packageType;
            Product = new Product(response.product);
            OfferingIdentifier = response.offeringIdentifier;
        }
    }
}
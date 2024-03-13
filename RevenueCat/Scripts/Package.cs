using System;
using System.Collections.Generic;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// Packages help abstract platform-specific products by grouping equivalent products across iOS, Android, and web.
    /// A package is made up of three parts: identifier, packageType, and underlying StoreProduct.
    /// </summary>
    public class Package
    {
        public readonly string Identifier;
        public readonly string PackageType;
        public readonly StoreProduct StoreProduct;
        public readonly PresentedOfferingContext PresentedOfferingContext;

        [Obsolete("Deprecated, use PresentedOfferingContext instead.", false)]
        public readonly string OfferingIdentifier;

        public Package(JSONNode response)
        {
            Identifier = response["identifier"];
            PackageType = response["packageType"];
            StoreProduct = new StoreProduct(response["product"]);
            PresentedOfferingContext = new PresentedOfferingContext(response["presentedOfferingContext"]);
            OfferingIdentifier = PresentedOfferingContext.OfferingIdentifier;
        }

        public override string ToString()
        {
            return $"{nameof(Identifier)}: {Identifier}\n" +
                   $"{nameof(PackageType)}: {PackageType}\n" +
                   $"{nameof(StoreProduct)}: {StoreProduct}\n" +
                   $"{nameof(PresentedOfferingContext)}: {PresentedOfferingContext}";
        }
    }
}
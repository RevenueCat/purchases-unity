using Newtonsoft.Json;
using System;

namespace RevenueCat
{
    /// <summary>
    /// Packages help abstract platform-specific products by grouping equivalent products across iOS, Android, and web.
    /// A package is made up of three parts: identifier, packageType, and underlying StoreProduct.
    /// </summary>
    public class Package
    {
        public string Identifier { get; }
        public string PackageType { get; }
        public StoreProduct StoreProduct { get; }
        public PresentedOfferingContext PresentedOfferingContext { get; }

        [Obsolete("Deprecated, use PresentedOfferingContext instead.", false)]
        public string OfferingIdentifier => PresentedOfferingContext?.OfferingIdentifier;

        [JsonConstructor]
        internal Package(
            [JsonProperty("identifier")] string identifier,
            [JsonProperty("packageType")] string packageType,
            [JsonProperty("product")] StoreProduct storeProduct,
            [JsonProperty("presentedOfferingContext")] PresentedOfferingContext presentedOfferingContext)
        {
            Identifier = identifier;
            PackageType = packageType;
            StoreProduct = storeProduct;
            PresentedOfferingContext = presentedOfferingContext;
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
using System.Collections.Generic;
using RevenueCat.SimpleJSON;

namespace RevenueCatUI
{
    /// <summary>The result of passing a checkpoint.</summary>
    public class CheckpointResult
    {
        /// <summary>Null when the checkpoint had nothing to present.</summary>
        public FlowResult Flow { get; }

        /// <summary>Customer information after passing the checkpoint.</summary>
        public Purchases.CustomerInfo Customer { get; }

        internal CheckpointResult(JSONNode response)
        {
            Customer = new Purchases.CustomerInfo(response["customer"]);
            var flow = response["flow"];
            Flow = flow == null || flow.IsNull ? null : new FlowResult(flow);
        }
    }

    /// <summary>What the customer obtained during the checkpoint's flow.</summary>
    public class FlowResult
    {
        public IReadOnlyList<ObtainedEntitlement> ObtainedEntitlements { get; }
        public IReadOnlyList<ObtainedProduct> ObtainedProducts { get; }

        internal FlowResult(JSONNode response)
        {
            var entitlements = new List<ObtainedEntitlement>();
            foreach (JSONNode entry in response["obtainedEntitlements"].AsArray)
                entitlements.Add(new ObtainedEntitlement(entry));
            ObtainedEntitlements = entitlements.AsReadOnly();

            var products = new List<ObtainedProduct>();
            foreach (JSONNode entry in response["obtainedProducts"].AsArray)
                products.Add(new ObtainedProduct(entry));
            ObtainedProducts = products.AsReadOnly();
        }
    }

    public class ObtainedEntitlement
    {
        public Purchases.EntitlementInfo EntitlementInfo { get; }

        internal ObtainedEntitlement(JSONNode response)
        {
            EntitlementInfo = new Purchases.EntitlementInfo(response["entitlementInfo"]);
        }
    }

    public class ObtainedProduct
    {
        public Purchases.StoreProduct Product { get; }

        internal ObtainedProduct(JSONNode response)
        {
            Product = new Purchases.StoreProduct(response["product"]);
        }
    }
}

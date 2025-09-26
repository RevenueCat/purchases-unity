using Newtonsoft.Json;

namespace RevenueCat
{
    /// <summary>
    /// Represents a web redemption link, that can be redeemed using `Purchases.redeemWebPurchase`.
    /// </summary>
    public sealed class WebPurchaseRedemption
    {
        /// <summary>
        /// Actual Redemption Link used.
        /// </summary>
        public string RedemptionLink { get; }

        [JsonConstructor]
        internal WebPurchaseRedemption([JsonProperty("redemption_link")] string redemptionLink)
        {
            RedemptionLink = redemptionLink;
        }

        public override string ToString()
        {
            return $"{nameof(RedemptionLink)}: {RedemptionLink}";
        }
    }
}
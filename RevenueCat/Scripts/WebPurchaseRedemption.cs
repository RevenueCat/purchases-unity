using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// Represents a web redemption link, that can be redeemed using `Purchases.redeemWebPurchase`.
    /// </summary>
    public class WebPurchaseRedemption
    {
        /// <summary>
        /// Actual Redemption Link used.
        /// </summary>
        public readonly string RedemptionLink;

        public WebPurchaseRedemption(string redemptionLink)
        {
            RedemptionLink = redemptionLink;
        }

        public override string ToString()
        {
            return $"{nameof(RedemptionLink)}: {RedemptionLink}";
        }
    }
}
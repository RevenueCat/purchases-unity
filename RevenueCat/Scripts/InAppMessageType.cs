public partial class Purchases
{
    /// Enum for in-app message types.
    /// This can be used if you disable automatic in-app message from showing automatically.
    /// Then, you can pass what type of messages you want to show in the `showInAppMessages`
    /// method in Purchases.
    public enum InAppMessageType
    {
        /// In-app messages to indicate there has been a billing issue charging the user.
        BillingIssue = 0,

        /// iOS-only. This message will show if you increase the price of a subscription and
        /// the user needs to opt-in to the increase.
        PriceIncreaseConsent = 1,

        /// iOS-only. StoreKit generic messages.
        Generic = 2,
    }
}

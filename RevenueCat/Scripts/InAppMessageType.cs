namespace RevenueCat
{
    /// <summary>
    /// Enum for in-app message types.
    /// This can be used if you disable automatic in-app message from showing automatically.
    /// Then, you can pass what type of messages you want to show in the `showInAppMessages`
    /// method in Purchases.
    /// </summary>
    public enum InAppMessageType
    {
        /// <summary>
        /// In-app messages to indicate there has been a billing issue charging the user.
        /// </summary>
        BillingIssue = 0,
        /// <summary>
        /// iOS-only. This message will show if you increase the price of a subscription and
        /// the user needs to opt-in to the increase.
        /// </summary>
        PriceIncreaseConsent = 1,
        /// <summary>
        /// iOS-only. StoreKit generic messages.
        /// </summary>
        Generic = 2,
        /// <summary>
        /// iOS-only. This message will show if the subscriber is eligible for an iOS win-back
        /// offer and will allow the subscriber to redeem the offer.
        /// </summary>
        WinBackOffer = 3,
    }
}

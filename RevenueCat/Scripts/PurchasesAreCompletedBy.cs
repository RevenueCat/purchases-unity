namespace RevenueCat
{
    public enum PurchasesAreCompletedBy
    {
        /// RevenueCat will automatically acknowledge verified purchases. No action is required by you.
        REVENUECAT,
        /// RevenueCat will **not** automatically acknowledge any purchases. You will have to do so manually.
        /// **Note:** failing to acknowledge a purchase within 3 days will lead to Google Play automatically issuing a
        /// refund to the user.
        /// For more info, see [revenuecat.com](https://docs.revenuecat.com/docs/observer-mode#option-2-client-side).
        MY_APP,
    }
}

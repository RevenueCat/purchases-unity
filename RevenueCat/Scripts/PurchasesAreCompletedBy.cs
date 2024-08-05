using System;
using System.ComponentModel;

public partial class Purchases
{
    public enum PurchasesAreCompletedBy
    {
        /// RevenueCat will automatically acknowledge verified purchases. No action is required by you.
        [Description("REVENUECAT")]
        RevenueCat,

        /// RevenueCat will **not** automatically acknowledge any purchases. You will have to do so manually.
        /// **Note:** failing to acknowledge a purchase within 3 days will lead to Google Play automatically issuing a
        /// refund to the user.
        /// For more info, see [revenuecat.com](https://docs.revenuecat.com/docs/observer-mode#option-2-client-side).
        [Description("MY_APP")]
        MyApp,
    }
}

internal static class PurchasesAreCompletedByExtensions
{
    internal static string Name(this Purchases.PurchasesAreCompletedBy purchasesAreCompletedBy)
    {
        var type = purchasesAreCompletedBy.GetType();
        var memInfo = type.GetMember(purchasesAreCompletedBy.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        var stringValue = ((DescriptionAttribute) attributes[0]).Description;
        return stringValue;
    }
}

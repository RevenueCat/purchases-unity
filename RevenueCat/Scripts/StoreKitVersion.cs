using System.ComponentModel;

namespace RevenueCat
{
    public enum StoreKitVersion
    {
        /// Always use StoreKit 1.
        [Description("STOREKIT_1")]
        StoreKit1,

        /// Always use StoreKit 2 (StoreKit 1 will be used if StoreKit 2 is not available in the current device.)
        /// - Warning: Make sure you have an In-App Purchase Key configured in your app.
        /// Please see https://rev.cat/in-app-purchase-key-configuration for more info.
        [Description("STOREKIT_2")]
        StoreKit2,

        /// Let RevenueCat use the most appropiate version of StoreKit.
        [Description("DEFAULT")]
        Default,
    }

    internal static class StoreKitVersionExtensions
    {
        internal static string Name(this StoreKitVersion storeKitVersion)
        {
            var type = storeKitVersion.GetType();
            var memInfo = type.GetMember(storeKitVersion.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            var stringValue = ((DescriptionAttribute)attributes[0]).Description;
            return stringValue;
        }
    }
}

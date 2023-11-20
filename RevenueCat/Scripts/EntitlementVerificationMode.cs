using System;
using System.ComponentModel;

public partial class Purchases
{
    /// <summary>
    /// Enum of entitlement verification modes.
    /// </summary>
    public enum EntitlementVerificationMode
    {
        /// <summary>
        /// The SDK will not perform any entitlement verification.
        /// </summary>
        [Description("DISABLED")]
        Disabled,

        /// <summary>
        /// Enable entitlement verification.
        ///
        /// If verification fails, this will be indicated with [VerificationResult.FAILED] in 
        /// the [EntitlementInfos.verification] and [EntitlementInfo.verification] properties but parsing will not fail 
        /// (i.e. Entitlements will still be granted).
        ///
        /// This can be useful if you want to handle verification failures to display an error/warning to the user
        /// or to track this situation but still grant access.
        /// </summary>
        [Description("INFORMATIONAL")]
        Informational,

        // WIP: Add Enforced mode once its ready.
        // Enforced
    }
}

internal static class EntitlementVerificationModeExtensions
{
    internal static string Name(this Purchases.EntitlementVerificationMode verificationMode)
    {
        var type = verificationMode.GetType();
        var memInfo = type.GetMember(verificationMode.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        var stringValue = ((DescriptionAttribute) attributes[0]).Description;
        return stringValue;
    }
}

using System;
using System.ComponentModel;

public partial class Purchases
{
    /// <summary>
    /// The result of the verification process. For more details check: http://rev.cat/trusted-entitlements
    ///
    /// This is accomplished by preventing MiTM attacks between the SDK and the RevenueCat server.
    /// With verification enabled, the SDK ensures that the response created by the server was not
    /// modified by a third-party, and the response received is exactly what was sent.
    /// - Note: Verification is only performed if enabled using PurchasesConfiguration's
    /// entitlementVerificationMode property. This is disabled by default.
    /// </summary>
    public enum VerificationResult
    {
        /// <summary>
        /// No verification was done. This value is returned when verification is not enabled in PurchasesConfiguration
        /// </summary>
        [Description("NOT_REQUESTED")]
        NotRequested,

        /// <summary>
        /// Verification with our server was performed successfully.
        /// </summary>
        [Description("VERIFIED")]
        Verified,

        /// <summary>
        /// Verification failed, possibly due to a MiTM attack.
        /// </summary>
        [Description("FAILED")]
        Failed,
        
        /// <summary>
        /// Verification was performed on device.
        /// </summary>
        [Description("VERIFIED_ON_DEVICE")]
        VerifiedOnDevice,
    }
}

internal static class VerificationResultExtensions
{
    internal static Purchases.VerificationResult ParseVerificationResultByName(string name)
    {
        foreach (var field in typeof(Purchases.VerificationResult).GetFields())
        {
            if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == name) return (Purchases.VerificationResult) field.GetValue(null);
            }
            else
            {
                if (field.Name == name) return (Purchases.VerificationResult) field.GetValue(null);
            }
        }

        return Purchases.VerificationResult.NotRequested;
    }
}
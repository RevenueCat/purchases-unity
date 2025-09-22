using System.Diagnostics.CodeAnalysis;

namespace RevenueCat
{
    /// <summary>
    /// The result of the verification process. For more details check: http://rev.cat/trusted-entitlements
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
        NOT_REQUESTED,
        /// <summary>
        /// Verification with our server was performed successfully.
        /// </summary>
        VERIFIED,
        /// <summary>
        /// Verification failed, possibly due to a MiTM attack.
        /// </summary>
        FAILED,
        /// <summary>
        /// Verification was performed on device.
        /// </summary>
        VERIFIED_ON_DEVICE,
    }
}

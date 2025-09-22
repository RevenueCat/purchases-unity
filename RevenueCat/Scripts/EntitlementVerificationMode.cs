namespace RevenueCat
{
    /// <summary>
    /// Enum of entitlement verification modes.
    /// </summary>
    public enum EntitlementVerificationMode
    {
        /// <summary>
        /// The SDK will not perform any entitlement verification.
        /// </summary>
        DISABLED,
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
        INFORMATIONAL,
        // WIP: Add Enforced mode once its ready.
        // ENFORCED
    }
}

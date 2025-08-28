public partial class Purchases
{
    /// <summary>
    /// Possible values for the result of a paywall presentation.
    /// Matches Flutter's PaywallResult enum for consistency across platforms.
    /// </summary>
    public enum PaywallResult
    {
        /// <summary>
        /// Only returned when using presentPaywallIfNeeded. Returned if the paywall was not presented.
        /// </summary>
        NotPresented,
        
        /// <summary>
        /// Returned when the paywall was presented and the user cancelled without executing an action.
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// Returned when the paywall was presented and an error occurred performing an operation.
        /// </summary>
        Error,
        
        /// <summary>
        /// Returned when the paywall was presented and the user successfully purchased.
        /// </summary>
        Purchased,
        
        /// <summary>
        /// Returned when the paywall was presented and the user successfully restored.
        /// </summary>
        Restored
    }

    /// <summary>
    /// Helper methods for PaywallResult enum.
    /// </summary>
    public static class PaywallResultExtensions
    {
        /// <summary>
        /// Creates a PaywallResult enum value from a string result name.
        /// </summary>
        /// <param name="resultName">The string result name from the platform layer</param>
        /// <returns>The corresponding PaywallResult enum value</returns>
        public static PaywallResult FromString(string resultName)
        {
            switch (resultName?.ToLower())
            {
                case "purchased":
                    return PaywallResult.Purchased;
                case "restored":
                    return PaywallResult.Restored;
                case "cancelled":
                    return PaywallResult.Cancelled;
                case "error":
                    return PaywallResult.Error;
                case "notpresented":
                    return PaywallResult.NotPresented;
                default:
                    return PaywallResult.Cancelled; // fallback
            }
        }
    }
}

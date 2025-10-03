using System;

namespace RevenueCatUI
{
    /// <summary>
    /// Represents the result of a paywall presentation.
    /// </summary>
    [Serializable]
    public class PaywallResult
    {
        /// <summary>
        /// The type of result from the paywall presentation.
        /// </summary>
        public PaywallResultType Result { get; }

        /// <summary>
        /// Creates a new PaywallResult.
        /// </summary>
        /// <param name="result">The result type</param>
        public PaywallResult(PaywallResultType result)
        {
            Result = result;
        }

        // Convenient static properties for common results
        internal static PaywallResult NotPresented => new PaywallResult(PaywallResultType.NotPresented);
        internal static PaywallResult Cancelled => new PaywallResult(PaywallResultType.Cancelled);
        internal static PaywallResult Error => new PaywallResult(PaywallResultType.Error);
        internal static PaywallResult Purchased => new PaywallResult(PaywallResultType.Purchased);
        internal static PaywallResult Restored => new PaywallResult(PaywallResultType.Restored);

        public override string ToString()
        {
            return $"PaywallResult({Result})";
        }
    }

    /// <summary>
    /// Enum representing the possible results of a paywall presentation.
    /// </summary>
    public enum PaywallResultType
    {
        /// <summary>
        /// The paywall was not presented (e.g., user already has the required entitlement).
        /// </summary>
        NotPresented,

        /// <summary>
        /// The user cancelled the paywall presentation.
        /// </summary>
        Cancelled,

        /// <summary>
        /// An error occurred during paywall presentation.
        /// </summary>
        Error,

        /// <summary>
        /// The user completed a purchase.
        /// </summary>
        Purchased,

        /// <summary>
        /// The user restored their purchases.
        /// </summary>
        Restored
    }

    /// <summary>
    /// Extension methods for PaywallResultType.
    /// </summary>
    public static class PaywallResultTypeExtensions
    {
        /// <summary>
        /// Converts a PaywallResultType to its string representation used by the native SDKs.
        /// </summary>
        /// <param name="resultType">The result type to convert</param>
        /// <returns>String representation matching the native SDK format</returns>
        public static string ToNativeString(this PaywallResultType resultType)
        {
            return resultType switch
            {
                PaywallResultType.NotPresented => "NOT_PRESENTED",
                PaywallResultType.Cancelled => "CANCELLED", 
                PaywallResultType.Error => "ERROR",
                PaywallResultType.Purchased => "PURCHASED",
                PaywallResultType.Restored => "RESTORED",
                _ => "ERROR"
            };
        }

        /// <summary>
        /// Parses a native string result to PaywallResultType.
        /// </summary>
        /// <param name="nativeResult">The native result string</param>
        /// <returns>The corresponding PaywallResultType</returns>
        public static PaywallResultType FromNativeString(string nativeResult)
        {
            return nativeResult switch
            {
                "NOT_PRESENTED" => PaywallResultType.NotPresented,
                "CANCELLED" => PaywallResultType.Cancelled,
                "ERROR" => PaywallResultType.Error,
                "PURCHASED" => PaywallResultType.Purchased,
                "RESTORED" => PaywallResultType.Restored,
                _ => PaywallResultType.Error
            };
        }
    }
} 

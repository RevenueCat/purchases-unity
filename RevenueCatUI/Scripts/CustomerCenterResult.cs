using System;

namespace RevenueCatUI
{
    /// <summary>
    /// Represents the outcome of a Customer Center presentation.
    /// </summary>
    [Serializable]
    public class CustomerCenterResult
    {
        /// <summary>
        /// The result type returned by the native SDKs.
        /// </summary>
        public CustomerCenterResultType Result { get; }

        /// <summary>
        /// Creates a new CustomerCenterResult.
        /// </summary>
        /// <param name="result">The result type.</param>
        public CustomerCenterResult(CustomerCenterResultType result)
        {
            Result = result;
        }

        internal static CustomerCenterResult Dismissed => new CustomerCenterResult(CustomerCenterResultType.Dismissed);
        internal static CustomerCenterResult NotPresented => new CustomerCenterResult(CustomerCenterResultType.NotPresented);
        internal static CustomerCenterResult Error => new CustomerCenterResult(CustomerCenterResultType.Error);

        public override string ToString()
        {
            return $"CustomerCenterResult({Result})";
        }
    }

    /// <summary>
    /// Enum describing the possible outcomes of a Customer Center presentation.
    /// </summary>
    public enum CustomerCenterResultType
    {
        /// <summary>
        /// The Customer Center was presented and then dismissed.
        /// </summary>
        Dismissed,

        /// <summary>
        /// The Customer Center could not be presented (e.g., unavailable on platform).
        /// </summary>
        NotPresented,

        /// <summary>
        /// An error occurred while attempting to present the Customer Center.
        /// </summary>
        Error
    }

    /// <summary>
    /// Helpers to convert CustomerCenterResultType values to the native string representations.
    /// </summary>
    public static class CustomerCenterResultTypeExtensions
    {
        public static string ToNativeString(this CustomerCenterResultType resultType)
        {
            return resultType switch
            {
                CustomerCenterResultType.Dismissed => "DISMISSED",
                CustomerCenterResultType.NotPresented => "NOT_PRESENTED",
                CustomerCenterResultType.Error => "ERROR",
                _ => "ERROR"
            };
        }

        public static CustomerCenterResultType FromNativeString(string nativeResult)
        {
            return nativeResult switch
            {
                "DISMISSED" => CustomerCenterResultType.Dismissed,
                "NOT_PRESENTED" => CustomerCenterResultType.NotPresented,
                "ERROR" => CustomerCenterResultType.Error,
                _ => CustomerCenterResultType.Error
            };
        }
    }
}

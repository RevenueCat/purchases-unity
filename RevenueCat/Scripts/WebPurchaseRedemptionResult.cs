using Newtonsoft.Json.Linq;
using System;

namespace RevenueCat
{
    /// Represents the result of a web purchase redemption process.
    public abstract class WebPurchaseRedemptionResult
    {
        private WebPurchaseRedemptionResult() { }

        /// <summary>
        /// Indicates that the web purchase was redeemed successfully.
        /// </summary>
        public sealed class Success : WebPurchaseRedemptionResult
        {
            public CustomerInfo CustomerInfo;

            public Success(CustomerInfo customerInfo) => CustomerInfo = customerInfo;
        }

        /// <summary>
        /// Indicates that an unknown error occurred during the redemption.
        /// </summary>
        public sealed class RedemptionError : WebPurchaseRedemptionResult
        {
            public Error Error;

            public RedemptionError(Error error) => Error = error;
        }

        /// <summary>
        /// Indicates that the redemption token is invalid.
        /// </summary>
        public sealed class InvalidToken : WebPurchaseRedemptionResult
        {
            private InvalidToken() { }
            public static InvalidToken Instance { get; } = new InvalidToken();
        }

        /// <summary>
        /// Indicates that the redemption token has expired.
        /// An email with a new redemption token might be sent if a new one wasn't already sent recently.
        /// The email where it will be sent is indicated by the <see cref="ObfuscatedEmail"/>.
        /// </summary>
        public sealed class Expired : WebPurchaseRedemptionResult
        {
            public string ObfuscatedEmail { get; }

            public Expired(string obfuscatedEmail) => ObfuscatedEmail = obfuscatedEmail;
        }

        /// <summary>
        /// Indicates that the redemption couldn't be performed because the purchase belongs to a different user.
        /// </summary>
        public sealed class PurchaseBelongsToOtherUser : WebPurchaseRedemptionResult
        {
            private PurchaseBelongsToOtherUser() { }
            public static PurchaseBelongsToOtherUser Instance { get; } = new PurchaseBelongsToOtherUser();
        }

        public static WebPurchaseRedemptionResult FromJson(JObject response)
        {
            var resultType = response["result"].Value<string>();
            switch (resultType)
            {
                case "SUCCESS":
                    var customerInfo = response["customerInfo"].ToObject<CustomerInfo>();
                    return new Success(customerInfo);
                case "ERROR":
                    var errorDetails = response["error"].ToObject<Error>();
                    return new RedemptionError(errorDetails);
                case "INVALID_TOKEN":
                    return InvalidToken.Instance;
                case "EXPIRED":
                    var obfuscatedEmail = response["obfuscatedEmail"].Value<string>();
                    return new Expired(obfuscatedEmail);
                case "PURCHASE_BELONGS_TO_OTHER_USER":
                    return PurchaseBelongsToOtherUser.Instance;
                default:
                    throw new ArgumentException($"Invalid result type: {resultType}");
            }
        }
    }
}
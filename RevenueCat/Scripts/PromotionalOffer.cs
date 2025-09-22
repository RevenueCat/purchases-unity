using Newtonsoft.Json;
using System;

namespace RevenueCat
{
    /// <summary>
    /// Represents a <see cref="Discount"/> that has been validated and is ready to be used for a purchase.
    /// </summary>
    public class PromotionalOffer
    {
        /// <summary>
        /// Identifier of the PromotionalOffer.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        ///  A string that identifies the key used to generate the signature.
        /// </summary>
        public string KeyIdentifier { get; }

        /// <summary>
        /// A universally unique ID (UUID) value that you define.
        /// </summary>
        public string Nonce { get; }

        /// <summary>
        /// A string representing the properties of a specific promotional offer, cryptographically signed.
        /// </summary>
        public string Signature { get; }

        /// <summary>
        /// The date and time of the signature's creation.
        /// </summary>
        public DateTime Timestamp { get; }

        [JsonConstructor]
        internal PromotionalOffer(
            [JsonProperty("identifier")] string identifier,
            [JsonProperty("keyIdentifier")] string keyIdentifier,
            [JsonProperty("nonce")] string nonce,
            [JsonProperty("signature")] string signature,
            [JsonProperty("timestamp")] long timestamp)
        {
            Identifier = identifier;
            KeyIdentifier = keyIdentifier;
            Nonce = nonce;
            Signature = signature;
            Timestamp = Utilities.FromUnixTimeInMilliseconds(timestamp);
        }

        public override string ToString()
        {
            return $"{nameof(Identifier)}: {Identifier}\n" +
                   $"{nameof(KeyIdentifier)}: {KeyIdentifier}\n" +
                   $"{nameof(Nonce)}: {Nonce}\n" +
                   $"{nameof(Signature)}: {Signature}\n" +
                   $"{nameof(Timestamp)}: {Timestamp}";
        }
    }
}
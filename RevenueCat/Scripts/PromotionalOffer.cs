using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// Represents a <see cref="Discount"/> that has been validated and is ready to be used for a purchase.
    /// </summary>
    public class PromotionalOffer
    {
        /// <summary>
        /// Identifier of the PromotionalOffer.
        /// </summary>
        public readonly string Identifier;
        
        /// <summary>
        ///  A string that identifies the key used to generate the signature.
        /// </summary>
        public readonly string KeyIdentifier;
        
        /// <summary>
        /// A universally unique ID (UUID) value that you define.
        /// </summary>
        public readonly string Nonce;
        
        /// <summary>
        /// A string representing the properties of a specific promotional offer, cryptographically signed.
        /// </summary>
        public readonly string Signature;
        
        /// <summary>
        /// The date and time of the signature's creation in milliseconds, formatted in Unix epoch time.
        /// </summary>
        public readonly long Timestamp;

        public PromotionalOffer(JSONNode response)
        {
            Identifier = response["identifier"];
            KeyIdentifier = response["keyIdentifier"];
            Nonce = response["nonce"];
            Signature = response["signature"];
            Timestamp = response["timestamp"];
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
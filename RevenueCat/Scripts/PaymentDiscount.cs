using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class PromotionalOffer
    {
        /// <summary>
        /// Identifier of the PromotionalOffer.
        /// </summary>
        public string identifier;
        
        /// <summary>
        ///  A string that identifies the key used to generate the signature.
        /// </summary>
        public string keyIdentifier;
        
        /// <summary>
        /// A universally unique ID (UUID) value that you define.
        /// </summary>
        public string nonce;
        
        /// <summary>
        /// A string representing the properties of a specific promotional offer, cryptographically signed.
        /// </summary>
        public string signature;
        
        /// <summary>
        /// The date and time of the signature's creation in milliseconds, formatted in Unix epoch time.
        /// </summary>
        public long timestamp;

        public PromotionalOffer(JSONNode response)
        {
            identifier = response["identifier"];
            keyIdentifier = response["keyIdentifier"];
            nonce = response["nonce"];
            signature = response["signature"];
            timestamp = response["timestamp"];
        }

        public override string ToString()
        {
            return $"{nameof(identifier)}: {identifier}, " +
                   $"{nameof(keyIdentifier)}: {keyIdentifier}, " +
                   $"{nameof(nonce)}: {nonce}, " +
                   $"{nameof(signature)}: {signature}, " +
                   $"{nameof(timestamp)}: {timestamp}";
        }
    }
}
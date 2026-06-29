using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// A reward granted after a verified rewarded ad. Inspect <see cref="Type"/> to
    /// determine which fields are populated.
    /// </summary>
    /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
    public class VerifiedReward
    {
        /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
        public enum RewardType
        {
            /// A virtual currency reward. <see cref="Code"/> and <see cref="Amount"/> are populated.
            VirtualCurrency,
            /// An entitlement reward. <see cref="Identifier"/>, <see cref="ExpiresAt"/> and
            /// <see cref="ExpiresAtMillis"/> are populated.
            Entitlement,
            /// Verification completed but nothing was granted.
            NoReward,
            /// Verification completed but the reward type isn't modeled by this SDK version.
            Unsupported
        }

        /// <summary>
        /// The kind of reward. Determines which of the fields below are populated.
        /// </summary>
        public readonly RewardType Type;

        /// <summary>
        /// The virtual currency code. Populated when <see cref="Type"/> is
        /// <see cref="RewardType.VirtualCurrency"/>.
        /// </summary>
        public readonly string Code;

        /// <summary>
        /// The virtual currency amount granted. Populated when <see cref="Type"/> is
        /// <see cref="RewardType.VirtualCurrency"/>.
        /// </summary>
        public readonly int Amount;

        /// <summary>
        /// The entitlement identifier. Populated when <see cref="Type"/> is
        /// <see cref="RewardType.Entitlement"/>.
        /// </summary>
        public readonly string Identifier;

        /// <summary>
        /// ISO 8601 expiration date string. Populated when <see cref="Type"/> is
        /// <see cref="RewardType.Entitlement"/>.
        /// </summary>
        public readonly string ExpiresAt;

        /// <summary>
        /// Expiration date in milliseconds since epoch. Populated when <see cref="Type"/> is
        /// <see cref="RewardType.Entitlement"/>.
        /// </summary>
        public readonly long ExpiresAtMillis;

        public VerifiedReward(JSONNode response)
        {
            switch ((string) response["type"])
            {
                case "virtual_currency":
                    Type = RewardType.VirtualCurrency;
                    Code = response["code"];
                    Amount = response["amount"];
                    break;
                case "entitlement":
                    Type = RewardType.Entitlement;
                    Identifier = response["identifier"];
                    ExpiresAt = response["expiresAt"];
                    ExpiresAtMillis = response["expiresAtMillis"].AsLong;
                    break;
                case "no_reward":
                    Type = RewardType.NoReward;
                    break;
                default:
                    Type = RewardType.Unsupported;
                    break;
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case RewardType.VirtualCurrency:
                    return $"{nameof(Type)}: {Type}, {nameof(Code)}: {Code}, {nameof(Amount)}: {Amount}";
                case RewardType.Entitlement:
                    return $"{nameof(Type)}: {Type}, {nameof(Identifier)}: {Identifier}, " +
                           $"{nameof(ExpiresAt)}: {ExpiresAt}, {nameof(ExpiresAtMillis)}: {ExpiresAtMillis}";
                default:
                    return $"{nameof(Type)}: {Type}";
            }
        }
    }
}

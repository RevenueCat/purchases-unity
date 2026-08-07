using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// A reward granted after a verified rewarded ad. Switch on the concrete subtype to read the
    /// fields relevant to that reward.
    /// </summary>
    /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
    public abstract class VerifiedReward
    {
        private VerifiedReward() { }

        /// <summary>
        /// A virtual currency reward.
        /// </summary>
        public sealed class VirtualCurrency : VerifiedReward
        {
            /// The virtual currency code.
            public readonly string Code;

            /// The virtual currency amount granted.
            public readonly int Amount;

            public VirtualCurrency(string code, int amount)
            {
                Code = code;
                Amount = amount;
            }

            public override string ToString() =>
                $"{nameof(VirtualCurrency)}({nameof(Code)}: {Code}, {nameof(Amount)}: {Amount})";
        }

        /// <summary>
        /// An entitlement reward.
        /// </summary>
        public sealed class Entitlement : VerifiedReward
        {
            /// The entitlement identifier.
            public readonly string Identifier;

            /// ISO 8601 expiration date string.
            public readonly string ExpiresAt;

            /// Expiration date in milliseconds since epoch.
            public readonly long ExpiresAtMillis;

            public Entitlement(string identifier, string expiresAt, long expiresAtMillis)
            {
                Identifier = identifier;
                ExpiresAt = expiresAt;
                ExpiresAtMillis = expiresAtMillis;
            }

            public override string ToString() =>
                $"{nameof(Entitlement)}({nameof(Identifier)}: {Identifier}, " +
                $"{nameof(ExpiresAt)}: {ExpiresAt}, {nameof(ExpiresAtMillis)}: {ExpiresAtMillis})";
        }

        /// <summary>
        /// Verification completed but nothing was granted.
        /// </summary>
        public sealed class NoReward : VerifiedReward
        {
            private NoReward() { }
            public static NoReward Instance { get; } = new NoReward();

            public override string ToString() => nameof(NoReward);
        }

        /// <summary>
        /// Verification completed but the reward type isn't modeled by this SDK version.
        /// </summary>
        public sealed class Unsupported : VerifiedReward
        {
            private Unsupported() { }
            public static Unsupported Instance { get; } = new Unsupported();

            public override string ToString() => nameof(Unsupported);
        }

        public static VerifiedReward FromJson(JSONNode response)
        {
            switch ((string) response["type"])
            {
                case "virtual_currency":
                    return new VirtualCurrency(response["code"], response["amount"]);
                case "entitlement":
                    return new Entitlement(
                        response["identifier"],
                        response["expiresAt"],
                        response["expiresAtMillis"].AsLong);
                case "no_reward":
                    return NoReward.Instance;
                default:
                    return Unsupported.Instance;
            }
        }
    }
}

using System;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// A reward configured for an ad unit, before an ad is shown. Switch on the concrete subtype
    /// to read its fields. Use <see cref="VerifiedReward"/> for rewards actually granted after verification.
    /// </summary>
    public abstract class AdRewardConfiguration
    {
        private AdRewardConfiguration() { }

        /// <summary> A configured virtual currency reward. </summary>
        public sealed class VirtualCurrency : AdRewardConfiguration
        {
            /// The virtual currency code.
            public readonly string Code;

            /// The configured reward amount.
            public readonly int Amount;

            internal VirtualCurrency(string code, int amount)
            {
                Code = code;
                Amount = amount;
            }

            public override string ToString() =>
                $"{nameof(VirtualCurrency)}({nameof(Code)}: {Code}, {nameof(Amount)}: {Amount})";
        }

        /// <summary> A configured entitlement reward. </summary>
        public sealed class Entitlement : AdRewardConfiguration
        {
            /// The entitlement identifier.
            public readonly string Identifier;

            /// The duration for which access will be granted, rather than an expiration date.
            public readonly TimeSpan Duration;

            internal Entitlement(string identifier, TimeSpan duration)
            {
                Identifier = identifier;
                Duration = duration;
            }

            public override string ToString() =>
                $"{nameof(Entitlement)}({nameof(Identifier)}: {Identifier}, {nameof(Duration)}: {Duration})";
        }

        /// <summary> A configured reward that this SDK version cannot represent. </summary>
        public sealed class Unsupported : AdRewardConfiguration
        {
            private Unsupported() { }
            public static Unsupported Instance { get; } = new Unsupported();

            public override string ToString() => nameof(Unsupported);
        }

        /// <summary>
        /// Maps a reward from purchases-hybrid-common. Missing, null and no_reward values return null;
        /// unknown types and invalid reward values return <see cref="Unsupported"/>.
        /// </summary>
        public static AdRewardConfiguration FromJson(JSONNode response)
        {
            if (response == null || response.IsNull) return null;
            if (!response.IsObject || !response["type"].IsString) return Unsupported.Instance;

            switch ((string)response["type"])
            {
                case "no_reward":
                    return null;
                case "virtual_currency":
                    var amount = response["amount"];
                    if (!response["code"].IsString || string.IsNullOrEmpty(response["code"]) ||
                        !amount.IsNumber || amount.AsDouble <= 0 || amount.AsDouble > int.MaxValue ||
                        amount.AsDouble != Math.Truncate(amount.AsDouble)) return Unsupported.Instance;
                    return new VirtualCurrency(response["code"], amount.AsInt);
                case "entitlement":
                    var duration = response["durationSeconds"];
                    if (!response["identifier"].IsString || string.IsNullOrEmpty(response["identifier"]) ||
                        !duration.IsNumber || duration.AsDouble <= 0 ||
                        double.IsNaN(duration.AsDouble) ||
                        duration.AsDouble >= TimeSpan.MaxValue.TotalSeconds) return Unsupported.Instance;
                    return new Entitlement(response["identifier"], TimeSpan.FromSeconds(duration.AsDouble));
                default:
                    return Unsupported.Instance;
            }
        }
    }
}

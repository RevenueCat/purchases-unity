using Newtonsoft.Json;
using System.Collections.Generic;
using static RevenueCat.Utilities;

namespace RevenueCat
{
    /// <summary>
    /// This class contains all the entitlements associated to the user.
    /// </summary>
    public sealed class EntitlementInfos
    {
        [JsonProperty("all")]
        public IReadOnlyDictionary<string, EntitlementInfo> All { get; }

        [JsonProperty("active")]
        public IReadOnlyDictionary<string, EntitlementInfo> Active { get; }

        [JsonProperty("verification")]
        public VerificationResult Verification { get; }

        [JsonConstructor]
        internal EntitlementInfos(
            [JsonProperty("all")] Dictionary<string, EntitlementInfo> all,
            [JsonProperty("active")] Dictionary<string, EntitlementInfo> active,
            [JsonProperty("verification")] VerificationResult verification)
        {
            All = all;
            Active = active;
            Verification = verification;
        }

        public override string ToString()
        {
            return $"{nameof(All)}:\n{DictToString(All)}\n" +
                   $"{nameof(Active)}:\n{DictToString(Active)}\n" +
                   $"{nameof(Verification)}: {Verification}";
        }
    }
}
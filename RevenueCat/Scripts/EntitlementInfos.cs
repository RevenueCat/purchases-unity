using System.Collections.Generic;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;

public partial class Purchases
{
    /// <summary>
    /// This class contains all the entitlements associated to the user.
    /// </summary>
    public class EntitlementInfos
    {
        public readonly Dictionary<string, EntitlementInfo> All;
        public readonly Dictionary<string, EntitlementInfo> Active;
        public readonly VerificationResult Verification;

        public EntitlementInfos(JSONNode response)
        {
            All = new Dictionary<string, EntitlementInfo>();
            foreach (var keyValuePair in response["all"])
            {
                All.Add(keyValuePair.Key, new EntitlementInfo(keyValuePair.Value));
            }

            Active = new Dictionary<string, EntitlementInfo>();
            foreach (var keyValuePair in response["active"])
            {
                Active.Add(keyValuePair.Key, new EntitlementInfo(keyValuePair.Value));
            }

            Verification = VerificationResultExtensions.ParseVerificationResultByName(response["verification"]);
        }

        public override string ToString()
        {
            return $"{nameof(All)}:\n{DictToString(All)}\n" +
                   $"{nameof(Active)}:\n{DictToString(Active)}\n" +
                   $"{nameof(Verification)}: {Verification}";
        }
    }
}
using System.Collections.Generic;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class EntitlementInfos
    {
        public readonly Dictionary<string, EntitlementInfo> All;
        public readonly Dictionary<string, EntitlementInfo> Active;

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
        }

        public override string ToString()
        {
            return $"{nameof(All)}: {All}, {nameof(Active)}: {Active}";
        }
    }
}
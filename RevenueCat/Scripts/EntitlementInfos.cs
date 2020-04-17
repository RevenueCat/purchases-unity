using System.Collections.Generic;

public partial class Purchases
{
    public class EntitlementInfos
    {
        public readonly Dictionary<string, EntitlementInfo> All;
        public readonly Dictionary<string, EntitlementInfo> Active;

        public EntitlementInfos(EntitlementInfosResponse response)
        {
            All = new Dictionary<string, EntitlementInfo>();
            for (var i = 0; i < response.allKeys.Count; i++)
            {
                All[response.allKeys[i]] = new EntitlementInfo(response.allValues[i]);
            }
            Active = new Dictionary<string, EntitlementInfo>();
            for (var i = 0; i < response.activeKeys.Count; i++)
            {
                Active[response.activeKeys[i]] = new EntitlementInfo(response.activeValues[i]);
            }
        }

    }
}
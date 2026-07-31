using System;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// Advanced settings. Use only after contacting RevenueCat support and making sure you understand them.
    /// </summary>
    [Serializable]
    public class DangerousSettings
    {
        public readonly bool AutoSyncPurchases;

        public DangerousSettings(bool autoSyncPurchases)
        {
            AutoSyncPurchases = autoSyncPurchases;
        }

        public JSONNode Serialize()
        {
            var n = new JSONObject();
            n["AutoSyncPurchases"] = AutoSyncPurchases;
            return n;
        }

        public override string ToString()
        {
            return $"{nameof(AutoSyncPurchases)}: {AutoSyncPurchases}";
        }
    }
}

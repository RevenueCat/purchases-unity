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

        /// <summary>
        /// Enables RevenueCat Workflows (multipage paywalls). Internal RevenueCat use only;
        /// behavior may change without warning.
        /// </summary>
        internal readonly bool UseWorkflows;

        public DangerousSettings(bool autoSyncPurchases) : this(autoSyncPurchases, false)
        {
        }

        internal DangerousSettings(bool autoSyncPurchases, bool useWorkflows)
        {
            AutoSyncPurchases = autoSyncPurchases;
            UseWorkflows = useWorkflows;
        }

        public JSONNode Serialize()
        {
            var n = new JSONObject();
            n["AutoSyncPurchases"] = AutoSyncPurchases;
            n["UseWorkflows"] = UseWorkflows;
            return n;
        }

        public override string ToString()
        {
            return $"{nameof(AutoSyncPurchases)}: {AutoSyncPurchases}, {nameof(UseWorkflows)}: {UseWorkflows}";
        }
    }
}

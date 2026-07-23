using System;
using System.ComponentModel;
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
        /// Internal RevenueCat use only. Enables RevenueCat Workflows (multipage paywalls).
        /// This is unsupported API that will be removed in a future release. Do not use unless
        /// explicitly directed by RevenueCat; behavior may change without warning.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly bool UseWorkflows;

        public DangerousSettings(bool autoSyncPurchases) : this(autoSyncPurchases, false)
        {
        }

        /// <summary>
        /// Internal RevenueCat use only. Enables RevenueCat Workflows (multipage paywalls).
        /// This is unsupported API that will be removed in a future release. Do not use unless
        /// explicitly directed by RevenueCat; behavior may change without warning.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DangerousSettings(bool autoSyncPurchases, bool useWorkflows)
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

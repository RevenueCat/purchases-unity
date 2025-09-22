using Newtonsoft.Json;

namespace RevenueCat
{
    /// <summary>
    /// Advanced settings. Use only after contacting RevenueCat support and making sure you understand them.
    /// </summary>
    public sealed class DangerousSettings
    {
        [JsonProperty]
        public bool AutoSyncPurchases { get; }

        [JsonConstructor]
        public DangerousSettings(bool autoSyncPurchases = true)
        {
            AutoSyncPurchases = autoSyncPurchases;
        }

        public override string ToString()
        {
            return $"{nameof(AutoSyncPurchases)}: {AutoSyncPurchases}";
        }
    }
}

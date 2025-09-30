using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using static RevenueCat.Utilities;

namespace RevenueCat
{
    /// <summary>
    /// This class contains all the offerings configured in RevenueCat dashboard.
    /// Offerings let you control which products are shown to users without requiring an app update.
    /// </summary>
    ///  
    public class Offerings
    {
        public IReadOnlyDictionary<string, Offering> All { get; }

        [CanBeNull]
        public Offering Current { get; }

        [JsonConstructor]
        internal Offerings(
            [JsonProperty("all")] Dictionary<string, Offering> all,
            [JsonProperty("current")] Offering current = null)
        {
            All = all;
            Current = current;
        }

        public override string ToString()
        {
            var currentString = Current != null ? $"{nameof(Current)}: {Current}" : "current: <null>";
            return $"{currentString}\n{nameof(All)}: {DictToString(All)}";
        }
    }
}
using Newtonsoft.Json;
using System.Collections.Generic;
using static RevenueCat.Utilities;

namespace RevenueCat
{
    /// <summary>
    /// The VirtualCurrencies object contains all the virtual currencies associated to the user.
    /// </summary>
    public sealed class VirtualCurrencies
    {
        /// <summary>
        /// Map of all VirtualCurrency objects keyed by virtual currency code.
        /// </summary>
        public IReadOnlyDictionary<string, VirtualCurrency> All { get; }

        [JsonConstructor]
        internal VirtualCurrencies([JsonProperty("all")] Dictionary<string, VirtualCurrency> all)
        {
            All = all;
        }

        public override string ToString()
        {
            return $"{nameof(All)}:\n{DictToString(All)}";
        }
    }
}

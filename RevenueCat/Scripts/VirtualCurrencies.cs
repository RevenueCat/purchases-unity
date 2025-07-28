using System.Collections.Generic;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;

public partial class Purchases
{
    /// <summary>
    /// The VirtualCurrencies object contains all the virtual currencies associated to the user.
    /// </summary>
    public class VirtualCurrencies
    {
        /// <summary>
        /// Map of all VirtualCurrency objects keyed by virtual currency code.
        /// </summary>
        public readonly Dictionary<string, VirtualCurrency> All;

        public VirtualCurrencies(JSONNode response)
        {
            All = new Dictionary<string, VirtualCurrency>();
            foreach (var keyValuePair in response["all"])
            {
                All.Add(keyValuePair.Key, new VirtualCurrency(keyValuePair.Value));
            }
        }

        public override string ToString()
        {
            return $"{nameof(All)}:\n{DictToString(All)}";
        }
    }
}
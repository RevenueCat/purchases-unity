using System.Collections.Generic;
using JetBrains.Annotations;

public partial class Purchases
{
    public class Offerings
    {
        public readonly Dictionary<string, Offering> All;
        [CanBeNull] public readonly Offering Current;

        public Offerings(OfferingsResponse response)
        {
            All = new Dictionary<string, Offering>();
            for (var i = 0; i < response.allKeys.Count; i++)
            {
                All[response.allKeys[i]] = new Offering(response.allValues[i]);
            }
            if (response.current.identifier != null) {
                Current = new Offering(response.current);
            }
        }

    }
}
using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class Offerings
    {
        public readonly Dictionary<string, Offering> All;
        [CanBeNull] public readonly Offering Current;

        public Offerings(JSONNode response)
        {
            All = new Dictionary<string, Offering>();
            foreach (var keyValuePair in response["all"])
            {
                All.Add(keyValuePair.Key, new Offering(keyValuePair.Value));
            }

            var currentJsonNode = response["current"];
            if (currentJsonNode != null && !currentJsonNode.IsNull)
            {
                Current = new Offering(currentJsonNode);
            }
        }

        public override string ToString()
        {
            return $"{nameof(All)}: {All}, {nameof(Current)}: {Current}";
        }
    }
}
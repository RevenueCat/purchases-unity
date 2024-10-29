using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;

public partial class Purchases
{
    /// <summary>
    /// An offering is a collection of <see cref="Package"/>, and they let you control which products
    /// are shown to users without requiring an app update.
    /// </summary>
    public class Offering
    {
        public readonly string Identifier;
        public readonly string ServerDescription;
        public readonly List<Package> AvailablePackages;
        [CanBeNull] public readonly Dictionary<string, object> Metadata;
        [CanBeNull] public readonly Package Lifetime;
        [CanBeNull] public readonly Package Annual;
        [CanBeNull] public readonly Package SixMonth;
        [CanBeNull] public readonly Package ThreeMonth;
        [CanBeNull] public readonly Package TwoMonth;
        [CanBeNull] public readonly Package Monthly;
        [CanBeNull] public readonly Package Weekly;
        
        public Offering(JSONNode response)
        {
            Identifier = response["identifier"];
            ServerDescription = response["serverDescription"];
            AvailablePackages = new List<Package>();
            
            foreach (JSONNode packageResponse in response["availablePackages"])
            {
                AvailablePackages.Add(new Package(packageResponse));
            }

            if (response["lifetime"] != null && !response["lifetime"].IsNull)
            {
                Lifetime = new Package(response["lifetime"]);
            }

            if (response["annual"] != null && !response["annual"].IsNull)
            {
                Annual = new Package(response["annual"]);
            }

            if (response["sixMonth"] != null && !response["sixMonth"].IsNull)
            {
                SixMonth = new Package(response["sixMonth"]);
            }

            if (response["threeMonth"] != null && !response["threeMonth"].IsNull)
            {
                ThreeMonth = new Package(response["threeMonth"]);
            }

            if (response["twoMonth"] != null && !response["twoMonth"].IsNull)
            {
                TwoMonth = new Package(response["twoMonth"]);
            }

            if (response["monthly"] != null && !response["monthly"].IsNull)
            {
                Monthly = new Package(response["monthly"]);
            }
            if (response["weekly"] != null && !response["weekly"].IsNull)
            {
                Weekly = new Package(response["weekly"]);
            }
            Metadata = new Dictionary<string, object>();
            if (response["metadata"] != null && !response["metadata"].IsNull)
            {
                foreach(var metadataEntry in response["metadata"])
                {
                    object value = ParseJsonValue(metadataEntry.Value);
                    Metadata.Add(metadataEntry.Key, value);
                }
            }
        }

        public override string ToString()
        {
            return $"{nameof(Identifier)}: {Identifier}\n" +
                   $"{nameof(ServerDescription)}: {ServerDescription}\n" +
                   $"{nameof(AvailablePackages)}: {AvailablePackages}\n" +
                   $"{nameof(Metadata)}: {DictToString(Metadata)}\n" +
                   $"{nameof(Lifetime)}: {Lifetime}\n" +
                   $"{nameof(Annual)}: {Annual}\n" +
                   $"{nameof(SixMonth)}: {SixMonth}\n" +
                   $"{nameof(ThreeMonth)}: {ThreeMonth}\n" +
                   $"{nameof(TwoMonth)}: {TwoMonth}\n" +
                   $"{nameof(Monthly)}: {Monthly}\n" +
                   $"{nameof(Weekly)}: {Weekly}";
        }

        private object ParseJsonValue(JSONNode jsonValue)
        {
            if (jsonValue.IsString) return jsonValue.Value;
            if (jsonValue.IsNumber) return jsonValue.AsFloat;
            if (jsonValue.IsBoolean) return jsonValue.AsBool;
            
            if (jsonValue.IsObject)
            {
                var dict = new Dictionary<string, object>();
                foreach (var kvp in jsonValue.AsObject)
                {
                    dict[kvp.Key] = ParseJsonValue(kvp.Value);
                }
                return dict;
            }
            
            if (jsonValue.IsArray)
            {
                var list = new List<object>();
                foreach (var item in jsonValue.AsArray)
                {
                    list.Add(ParseJsonValue(item));
                }
                return list;
            }
            
            return null;
        }
    }
}
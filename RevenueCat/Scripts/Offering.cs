using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using static RevenueCat.Utilities;

namespace RevenueCat
{
    /// <summary>
    /// An offering is a collection of <see cref="Package"/>, and they let you control which products
    /// are shown to users without requiring an app update.
    /// </summary>
    public class Offering
    {
        public string Identifier { get; }

        public string ServerDescription { get; }

        public IReadOnlyList<Package> AvailablePackages { get; }

        [CanBeNull]
        public IReadOnlyDictionary<string, object> Metadata { get; }

        [CanBeNull]
        public Package Lifetime { get; }

        [CanBeNull]
        public Package Annual { get; }

        [CanBeNull]
        public Package SixMonth { get; }

        [CanBeNull]
        public Package ThreeMonth { get; }

        [CanBeNull]
        public Package TwoMonth { get; }
        [CanBeNull]
        public Package Monthly { get; }

        [CanBeNull]
        public Package Weekly { get; }

        [JsonConstructor]
        internal Offering(
            [JsonProperty("identifier")] string identifier,
            [JsonProperty("serverDescription")] string serverDescription,
            [JsonProperty("availablePackages")] List<Package> availablePackages,
            [JsonProperty("metadata")] Dictionary<string, object> metadata = null,
            [JsonProperty("lifetime")] Package lifetime = null,
            [JsonProperty("annual")] Package annual = null,
            [JsonProperty("sixMonth")] Package sixMonth = null,
            [JsonProperty("threeMonth")] Package threeMonth = null,
            [JsonProperty("twoMonth")] Package twoMonth = null,
            [JsonProperty("monthly")] Package monthly = null,
            [JsonProperty("weekly")] Package weekly = null)
        {
            Identifier = identifier;
            ServerDescription = serverDescription;
            AvailablePackages = availablePackages;
            Metadata = metadata;
            Lifetime = lifetime;
            Annual = annual;
            SixMonth = sixMonth;
            ThreeMonth = threeMonth;
            TwoMonth = twoMonth;
            Monthly = monthly;
            Weekly = weekly;
        }

        public override string ToString()
        {
            return $"{nameof(Identifier)}: {Identifier}\n" +
                   $"{nameof(ServerDescription)}: {ServerDescription}\n" +
                   $"{nameof(AvailablePackages)}: {string.Join(", ", AvailablePackages)}\n" +
                   $"{nameof(Metadata)}: {DictToString(Metadata)}\n" +
                   $"{nameof(Lifetime)}: {Lifetime}\n" +
                   $"{nameof(Annual)}: {Annual}\n" +
                   $"{nameof(SixMonth)}: {SixMonth}\n" +
                   $"{nameof(ThreeMonth)}: {ThreeMonth}\n" +
                   $"{nameof(TwoMonth)}: {TwoMonth}\n" +
                   $"{nameof(Monthly)}: {Monthly}\n" +
                   $"{nameof(Weekly)}: {Weekly}";
        }
    }
}
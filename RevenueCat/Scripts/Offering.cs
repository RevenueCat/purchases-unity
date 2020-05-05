using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class Offering
    {
        public readonly string Identifier;
        public readonly string ServerDescription;
        public readonly List<Package> AvailablePackages;
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
        }

        public override string ToString()
        {
            return $"{nameof(Identifier)}: {Identifier}, " +
                   $"{nameof(ServerDescription)}: {ServerDescription}, " +
                   $"{nameof(AvailablePackages)}: {AvailablePackages}, " +
                   $"{nameof(Lifetime)}: {Lifetime}, " +
                   $"{nameof(Annual)}: {Annual}, " +
                   $"{nameof(SixMonth)}: {SixMonth}, " +
                   $"{nameof(ThreeMonth)}: {ThreeMonth}, " +
                   $"{nameof(TwoMonth)}: {TwoMonth}, " +
                   $"{nameof(Monthly)}: {Monthly}, " +
                   $"{nameof(Weekly)}: {Weekly}";
        }
    }
}
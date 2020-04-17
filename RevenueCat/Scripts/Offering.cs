using System.Collections.Generic;
using JetBrains.Annotations;

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

        public Offering(OfferingResponse response)
        {
            Identifier = response.identifier;
            ServerDescription = response.serverDescription;
            AvailablePackages = new List<Package>();
            foreach (var packageResponse in response.availablePackages)
            {
                AvailablePackages.Add(new Package(packageResponse));
            }
            if (response.lifetime.identifier != null)
            {
                Lifetime = new Package(response.lifetime);
            }
            if (response.annual.identifier != null)
            {
                Annual = new Package(response.annual);
            }
            if (response.sixMonth.identifier != null)
            {
                SixMonth = new Package(response.sixMonth);
            }
            if (response.threeMonth.identifier != null)
            {
                ThreeMonth = new Package(response.threeMonth);
            }
            if (response.twoMonth.identifier != null)
            {
                TwoMonth = new Package(response.twoMonth);
            }
            if (response.monthly.identifier != null)
            {
                Monthly = new Package(response.monthly);
            }
            if (response.weekly.identifier != null)
            {
                Weekly = new Package(response.weekly);
            }
        }
    }
}
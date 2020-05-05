using System.Collections.Generic;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class IntroEligibility
    {
        /// The introductory price eligibility status
        public readonly IntroEligibilityStatus Status;

        /// Description of the status
        public readonly string Description;

        public IntroEligibility(JSONNode response)
        {
            Status = (IntroEligibilityStatus) response["status"].AsInt;
            Description = response["description"];
        }

        public override string ToString()
        {
            return $"{nameof(Status)}: {Status}, {nameof(Description)}: {Description}";
        }
    }
}
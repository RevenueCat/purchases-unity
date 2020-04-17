public partial class Purchases
{
    public class IntroEligibility
    {
        /// The introductory price eligibility status
        public readonly IntroEligibilityStatus Status;

        /// Description of the status
        public readonly string Description;

        public IntroEligibility(IntroEligibilityResponse response)
        {
            Status = (IntroEligibilityStatus)response.status;
            Description = response.description;
        }

        public override string ToString()
        {
            return "{ status:" + Status + "; description:" + Description + " }";
        }
    }
}
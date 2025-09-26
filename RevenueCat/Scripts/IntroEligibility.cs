using Newtonsoft.Json;

namespace RevenueCat
{
    public class IntroEligibility
    {
        /// <summary>
        /// The introductory price eligibility status
        /// </summary>
        public IntroEligibilityStatus Status { get; }

        /// <summary>
        /// Description of the status
        /// </summary>
        public string Description { get; }

        [JsonConstructor]
        internal IntroEligibility(
            [JsonProperty("status")] IntroEligibilityStatus status,
            [JsonProperty("description")] string description)
        {
            Status = status;
            Description = description;
        }

        public override string ToString()
        {
            return $"{nameof(Status)}: {Status}, {nameof(Description)}: {Description}";
        }
    }
}
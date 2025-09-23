using Newtonsoft.Json;

namespace RevenueCat
{
    public sealed class LoginResult
    {
        public CustomerInfo CustomerInfo { get; }

        public bool Created { get; }

        [JsonConstructor]
        internal LoginResult(
            [JsonProperty("customerInfo")] CustomerInfo customerInfo,
            [JsonProperty("created")] bool created)
        {
            CustomerInfo = customerInfo;
            Created = created;
        }
    }
}

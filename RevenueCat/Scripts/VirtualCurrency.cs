using JetBrains.Annotations;
using Newtonsoft.Json;

namespace RevenueCat
{
    /// <summary>
    /// The VirtualCurrency object represents information about a virtual currency in the app.
    /// Use this object to access information about a virtual currency, such as its current balance.
    /// </summary>
    public class VirtualCurrency
    {
        /// <summary>
        /// The virtual currency's balance.
        /// </summary>
        [JsonProperty("balance")]
        public int Balance { get; }

        /// <summary>
        /// The virtual currency's name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// The virtual currency's code.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; }

        /// <summary>
        /// The virtual currency's description defined in the RevenueCat dashboard.
        /// </summary>
        [CanBeNull]
        [JsonProperty("serverDescription")]
        public string ServerDescription { get; }

        [JsonConstructor]
        internal VirtualCurrency(
            [JsonProperty("balance")] int balance,
            [JsonProperty("name")] string name,
            [JsonProperty("code")] string code,
            [JsonProperty("serverDescription")] string serverDescription)
        {
            Balance = balance;
            Name = name;
            Code = code;
            ServerDescription = serverDescription;
        }

        public override string ToString()
        {
            return $"{nameof(Balance)}: {Balance}\n" +
                   $"{nameof(Name)}: {Name}\n" +
                   $"{nameof(Code)}: {Code}\n" +
                   $"{nameof(ServerDescription)}: {ServerDescription ?? "null"}";
        }
    }
}

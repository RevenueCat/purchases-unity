using RevenueCat.SimpleJSON;

public partial class Purchases
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
        public readonly int Balance;

        /// <summary>
        /// The virtual currency's name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The virtual currency's code.
        /// </summary>
        public readonly string Code;

        /// <summary>
        /// The virtual currency's description defined in the RevenueCat dashboard.
        /// </summary>
        public readonly string? ServerDescription;

        public VirtualCurrency(JSONNode response)
        {
            Balance = response["balance"];
            Name = response["name"];
            Code = response["code"];
            ServerDescription = response["serverDescription"];
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

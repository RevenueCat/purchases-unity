using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// Token generated for a rewarded ad impression. Pass <see cref="ClientTransactionId"/>
    /// to the ad network as server-side verification custom data, then to
    /// <see cref="Purchases.PollRewardVerification"/> to await the reward.
    /// </summary>
    /// <remarks>Experimental: this API is unstable and may change in a future release.</remarks>
    public class RewardVerificationToken
    {
        /// <summary>
        /// Custom data to forward to the ad network for server-side verification.
        /// </summary>
        public readonly string CustomData;

        /// <summary>
        /// The transaction identifier to poll with <see cref="Purchases.PollRewardVerification"/>.
        /// </summary>
        public readonly string ClientTransactionId;

        /// <summary>
        /// The app user ID the token was generated for.
        /// </summary>
        public readonly string AppUserID;

        public RewardVerificationToken(JSONNode response)
        {
            CustomData = response["customData"];
            ClientTransactionId = response["clientTransactionId"];
            AppUserID = response["appUserID"];
        }

        public override string ToString()
        {
            return $"{nameof(CustomData)}: {CustomData}, " +
                   $"{nameof(ClientTransactionId)}: {ClientTransactionId}, " +
                   $"{nameof(AppUserID)}: {AppUserID}";
        }
    }
}

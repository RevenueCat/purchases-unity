using Newtonsoft.Json;
using System;
using static RevenueCat.Utilities;

namespace RevenueCat
{
    /// <summary>
    /// The EntitlementInfo object gives you access to all of the information about the status of a user entitlement.
    /// </summary>
    public sealed class EntitlementInfo
    {
        public string Identifier { get; }

        public bool IsActive { get; }

        public bool WillRenew { get; }

        public string PeriodType { get; }

        public DateTime LatestPurchaseDate { get; }

        public DateTime OriginalPurchaseDate { get; }

        public DateTime? ExpirationDate { get; }

        public string Store { get; }

        public string ProductIdentifier { get; }

        public bool IsSandbox { get; }

        public DateTime? UnsubscribeDetectedAt { get; }

        public DateTime? BillingIssueDetectedAt { get; }

        public VerificationResult Verification { get; }

        [JsonConstructor]
        public EntitlementInfo(
            [JsonProperty("identifier")] string identifier,
            [JsonProperty("isActive")] bool isActive,
            [JsonProperty("willRenew")] bool willRenew,
            [JsonProperty("periodType")] string periodType,
            [JsonProperty("latestPurchaseDateMillis")] long latestPurchaseDateMillis,
            [JsonProperty("originalPurchaseDateMillis")] long originalPurchaseDateMillis,
            [JsonProperty("expirationDateMillis")] long? expirationDateMillis,
            [JsonProperty("store")] string store,
            [JsonProperty("productIdentifier")] string productIdentifier,
            [JsonProperty("isSandbox")] bool isSandbox,
            [JsonProperty("unsubscribeDetectedAtMillis")] long? unsubscribeDetectedAtMillis,
            [JsonProperty("billingIssueDetectedAtMillis")] long? billingIssueDetectedAtMillis,
            [JsonProperty("verification")] VerificationResult verification)
        {
            Identifier = identifier;
            IsActive = isActive;
            WillRenew = willRenew;
            PeriodType = periodType;
            LatestPurchaseDate = FromUnixTimeInMilliseconds(latestPurchaseDateMillis);
            OriginalPurchaseDate = FromUnixTimeInMilliseconds(originalPurchaseDateMillis);
            ExpirationDate = FromOptionalUnixTimeInMilliseconds(expirationDateMillis);
            Store = store;
            ProductIdentifier = productIdentifier;
            IsSandbox = isSandbox;
            UnsubscribeDetectedAt = FromOptionalUnixTimeInMilliseconds(unsubscribeDetectedAtMillis);
            BillingIssueDetectedAt = FromOptionalUnixTimeInMilliseconds(billingIssueDetectedAtMillis);
            Verification = verification;
        }

        public override string ToString()
        {
            return
                $"{nameof(Identifier)}: {Identifier}\n" +
                $"{nameof(IsActive)}: {IsActive}\n" +
                $"{nameof(WillRenew)}: {WillRenew}\n" +
                $"{nameof(PeriodType)}: {PeriodType}\n" +
                $"{nameof(LatestPurchaseDate)}: {LatestPurchaseDate}\n" +
                $"{nameof(OriginalPurchaseDate)}: {OriginalPurchaseDate}\n" +
                $"{nameof(ExpirationDate)}: {ExpirationDate}\n" +
                $"{nameof(Store)}: {Store}\n" +
                $"{nameof(ProductIdentifier)}: {ProductIdentifier}\n" +
                $"{nameof(IsSandbox)}: {IsSandbox}\n" +
                $"{nameof(UnsubscribeDetectedAt)}: {UnsubscribeDetectedAt}\n" +
                $"{nameof(BillingIssueDetectedAt)}: {BillingIssueDetectedAt}\n" +
                $"{nameof(Verification)}: {Verification}";
        }
    }
}
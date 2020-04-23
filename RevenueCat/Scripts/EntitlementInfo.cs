using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    public class EntitlementInfo
    {
        public readonly string Identifier;
        public readonly bool IsActive;
        public readonly bool WillRenew;
        public readonly string PeriodType;
        public readonly DateTime LatestPurchaseDate;
        public readonly DateTime OriginalPurchaseDate;
        public readonly DateTime? ExpirationDate;
        public readonly string Store;
        public readonly string ProductIdentifier;
        public readonly bool IsSandbox;
        public readonly DateTime? UnsubscribeDetectedAt;
        public readonly DateTime? BillingIssueDetectedAt;

        public EntitlementInfo(JSONNode response)
        {
            Identifier = response["identifier"];
            IsActive = response["isActive"].AsBool;
            WillRenew = response["willRenew"].AsBool;
            PeriodType = response["periodType"];
            LatestPurchaseDate = FromUnixTime(response["latestPurchaseDateMillis"].AsLong);
            OriginalPurchaseDate = FromUnixTime(response["originalPurchaseDateMillis"].AsLong);

            var expirationDateJson = response["expirationDateMillis"];
            var hasExpirationDate = expirationDateJson != null && !expirationDateJson.IsNull &&
                                    expirationDateJson.AsLong != 0L;
            if (hasExpirationDate)
            {
                ExpirationDate = FromUnixTime(expirationDateJson.AsLong);
            }

            Store = response["store"];
            ProductIdentifier = response["productIdentifier"];
            IsSandbox = response["isSandbox"].AsBool;

            var unsubscribeDetectedJson = response["unsubscribeDetectedAtMillis"];
            var hasUnsubscribeDetected = unsubscribeDetectedJson != null && !unsubscribeDetectedJson.IsNull &&
                                         unsubscribeDetectedJson.AsLong != 0L;
            if (hasUnsubscribeDetected)
            {
                UnsubscribeDetectedAt = FromUnixTime(unsubscribeDetectedJson.AsLong);
            }

            var billingIssueJson = response["billingIssueDetectedAtMillis"];
            var hasBillingIssue = billingIssueJson != null && !billingIssueJson.IsNull &&
                                  billingIssueJson.AsLong != 0L;
            if (hasBillingIssue)
            {
                BillingIssueDetectedAt = FromUnixTime(billingIssueJson.AsLong);
            }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override string ToString()
        {
            return
                $"{nameof(Identifier)}: {Identifier}, " +
                $"{nameof(IsActive)}: {IsActive}, " +
                $"{nameof(WillRenew)}: {WillRenew}, " +
                $"{nameof(PeriodType)}: {PeriodType}, " +
                $"{nameof(LatestPurchaseDate)}: {LatestPurchaseDate}, " +
                $"{nameof(OriginalPurchaseDate)}: {OriginalPurchaseDate}, " +
                $"{nameof(ExpirationDate)}: {ExpirationDate}, " +
                $"{nameof(Store)}: {Store}, " +
                $"{nameof(ProductIdentifier)}: {ProductIdentifier}, " +
                $"{nameof(IsSandbox)}: {IsSandbox}, " +
                $"{nameof(UnsubscribeDetectedAt)}: {UnsubscribeDetectedAt}, " +
                $"{nameof(BillingIssueDetectedAt)}: {BillingIssueDetectedAt}";
        }
    }
}
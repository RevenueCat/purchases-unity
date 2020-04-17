using System;
using JetBrains.Annotations;

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
        [CanBeNull] public readonly DateTime? UnsubscribeDetectedAt;
        [CanBeNull] public readonly DateTime? BillingIssueDetectedAt;

        public EntitlementInfo(EntitlementInfoResponse response)
        {
            Identifier = response.identifier;
            IsActive = response.isActive;
            WillRenew = response.willRenew;
            PeriodType = response.periodType;
            LatestPurchaseDate = FromUnixTime(response.latestPurchaseDateMillis);
            OriginalPurchaseDate = FromUnixTime(response.originalPurchaseDateMillis);
            if (response.expirationDateMillis != 0L)
            {
                ExpirationDate = FromUnixTime(response.expirationDateMillis);
            }
            Store = response.store;
            ProductIdentifier = response.productIdentifier;
            IsSandbox = response.isSandbox;
            if (response.unsubscribeDetectedAtMillis != 0L)
            {
                UnsubscribeDetectedAt = FromUnixTime(response.unsubscribeDetectedAtMillis);
            }
            if (response.billingIssueDetectedAtMillis != 0L)
            {
                BillingIssueDetectedAt = FromUnixTime(response.billingIssueDetectedAtMillis);
            }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    }
}
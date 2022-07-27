using System;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;


public partial class Purchases
{
    
    /// <summary>
    /// The EntitlementInfo object gives you access to all of the information about the status of a user entitlement.
    /// </summary>
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
            LatestPurchaseDate = FromUnixTimeInMilliseconds(response["latestPurchaseDateMillis"].AsLong);
            OriginalPurchaseDate = FromUnixTimeInMilliseconds(response["originalPurchaseDateMillis"].AsLong);

            var expirationDateJson = response["expirationDateMillis"];
            var hasExpirationDate = expirationDateJson != null && !expirationDateJson.IsNull &&
                                    expirationDateJson.AsLong != 0L;
            if (hasExpirationDate)
            {
                ExpirationDate = FromUnixTimeInMilliseconds(expirationDateJson.AsLong);
            }

            Store = response["store"];
            ProductIdentifier = response["productIdentifier"];
            IsSandbox = response["isSandbox"].AsBool;

            var unsubscribeDetectedJson = response["unsubscribeDetectedAtMillis"];
            var hasUnsubscribeDetected = unsubscribeDetectedJson != null && !unsubscribeDetectedJson.IsNull &&
                                         unsubscribeDetectedJson.AsLong != 0L;
            if (hasUnsubscribeDetected)
            {
                UnsubscribeDetectedAt = FromUnixTimeInMilliseconds(unsubscribeDetectedJson.AsLong);
            }

            var billingIssueJson = response["billingIssueDetectedAtMillis"];
            var hasBillingIssue = billingIssueJson != null && !billingIssueJson.IsNull &&
                                  billingIssueJson.AsLong != 0L;
            if (hasBillingIssue)
            {
                BillingIssueDetectedAt = FromUnixTimeInMilliseconds(billingIssueJson.AsLong);
            }
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
                $"{nameof(BillingIssueDetectedAt)}: {BillingIssueDetectedAt}";
        }
    }
}
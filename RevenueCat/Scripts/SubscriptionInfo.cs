using UnityEngine;
using System;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;


public partial class Purchases
{
    
    /// <summary>
    /// The SubscriptionInfo object gives you access to all of the information about the status of a subscription.
    /// </summary>
    public class SubscriptionInfo
    {
        public readonly string ProductIdentifier;
        public readonly DateTime PurchaseDate;
        public readonly DateTime? OriginalPurchaseDate;
        public readonly DateTime? ExpiresDate;
        public readonly string Store;
        public readonly DateTime? UnsubscribeDetectedAt;
        public readonly bool IsSandbox;
        public readonly DateTime? BillingIssuesDetectedAt;
        public readonly DateTime? GracePeriodExpiresDate;
        public readonly string OwnershipType;
        public readonly string PeriodType;
        public readonly DateTime? RefundedAt;
        public readonly string? StoreTransactionId;
        public readonly bool IsActive;
        public readonly bool WillRenew;

        public SubscriptionInfo(JSONNode response)
        {
            ProductIdentifier = response["productIdentifier"];
            var purchaseDateTime = FromISO8601(response["purchaseDate"]);
            if (purchaseDateTime == null) {
                Debug.LogError("Purchase date is null or has an invalid format. Defaulting to 1970-01-01.");
            }
            PurchaseDate = purchaseDateTime ?? new DateTime(1970, 1, 1);
            OriginalPurchaseDate = FromResponseISO8601String(response, "originalPurchaseDate");
            ExpiresDate = FromResponseISO8601String(response, "expiresDate");
            Store = response["store"];
            UnsubscribeDetectedAt = FromResponseISO8601String(response, "unsubscribeDetectedAt");
            IsSandbox = response["isSandbox"].AsBool;
            BillingIssuesDetectedAt = FromResponseISO8601String(response, "billingIssueDetectedAt");
            GracePeriodExpiresDate = FromResponseISO8601String(response, "gracePeriodExpiresDate");
            OwnershipType = response["ownershipType"];
            PeriodType = response["periodType"];
            RefundedAt = FromResponseISO8601String(response, "refundedAt");
            StoreTransactionId = response["storeTransactionId"];
            IsActive = response["isActive"].AsBool;
            WillRenew = response["willRenew"].AsBool;
        }

        public override string ToString()
        {
            return
                $"{nameof(ProductIdentifier)}: {ProductIdentifier}\n" +
                $"{nameof(PurchaseDate)}: {PurchaseDate}\n" +
                $"{nameof(OriginalPurchaseDate)}: {OriginalPurchaseDate}\n" +
                $"{nameof(ExpiresDate)}: {ExpiresDate}\n" +
                $"{nameof(Store)}: {Store}\n" +
                $"{nameof(UnsubscribeDetectedAt)}: {UnsubscribeDetectedAt}\n" +
                $"{nameof(IsSandbox)}: {IsSandbox}\n" +
                $"{nameof(BillingIssuesDetectedAt)}: {BillingIssuesDetectedAt}\n" +
                $"{nameof(GracePeriodExpiresDate)}: {GracePeriodExpiresDate}\n" +
                $"{nameof(OwnershipType)}: {OwnershipType}\n" +
                $"{nameof(PeriodType)}: {PeriodType}\n" +
                $"{nameof(RefundedAt)}: {RefundedAt}\n" +
                $"{nameof(StoreTransactionId)}: {StoreTransactionId}\n" +
                $"{nameof(IsActive)}: {IsActive}\n" +
                $"{nameof(WillRenew)}: {WillRenew}";
        }

        private DateTime? FromResponseISO8601String(JSONNode response, string key)
        {
            var dateJson = response[key];
            var hasDate = dateJson != null && !dateJson.IsNull;
            if (hasDate)
            {
                return FromISO8601(dateJson);
            }
            else
            {
                return null;
            }
        }
    }
}

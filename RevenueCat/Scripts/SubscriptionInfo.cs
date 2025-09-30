using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using System;
using static RevenueCat.Utilities;

namespace RevenueCat
{
    /// <summary>
    /// The SubscriptionInfo object gives you access to all of the information about the status of a subscription.
    /// </summary>
    public class SubscriptionInfo
    {
        public string ProductIdentifier { get; }

        public DateTime PurchaseDate { get; }

        public DateTime? OriginalPurchaseDate { get; }

        public DateTime? ExpiresDate { get; }

        public string Store { get; }

        public DateTime? UnsubscribeDetectedAt { get; }

        public bool IsSandbox { get; }

        public DateTime? BillingIssuesDetectedAt { get; }

        public DateTime? GracePeriodExpiresDate { get; }

        public string OwnershipType { get; }

        public string PeriodType { get; }

        public DateTime? RefundedAt { get; }

        [CanBeNull]
        public string StoreTransactionId { get; }

        public bool IsActive { get; }

        public bool WillRenew { get; }

        [JsonConstructor]
        internal SubscriptionInfo(
            [JsonProperty("productIdentifier")] string productIdentifier,
            [JsonProperty("purchaseDate")] string purchaseDate,
            [JsonProperty("originalPurchaseDate")] string originalPurchaseDate,
            [JsonProperty("expiresDate")] string expiresDate,
            [JsonProperty("store")] string store,
            [JsonProperty("unsubscribeDetectedAt")] string unsubscribeDetectedAt,
            [JsonProperty("isSandbox")] bool isSandbox,
            [JsonProperty("billingIssuesDetectedAt")] string billingIssuesDetectedAt,
            [JsonProperty("gracePeriodExpiresDate")] string gracePeriodExpiresDate,
            [JsonProperty("ownershipType")] string ownershipType,
            [JsonProperty("periodType")] string periodType,
            [JsonProperty("refundedAt")] string refundedAt,
            [JsonProperty("storeTransactionId")] string storeTransactionId,
            [JsonProperty("isActive")] bool isActive,
            [JsonProperty("willRenew")] bool willRenew)
        {
            ProductIdentifier = productIdentifier;
            var purchaseDateTime = FromISO8601(purchaseDate);

            if (purchaseDateTime == null)
            {
                Debug.LogError("Purchase date is null or has an invalid format. Defaulting to 1970-01-01.");
            }

            PurchaseDate = purchaseDateTime ?? new DateTime(1970, 1, 1);
            OriginalPurchaseDate = FromISO8601(originalPurchaseDate);
            ExpiresDate = FromISO8601(expiresDate);
            Store = store;
            UnsubscribeDetectedAt = FromISO8601(unsubscribeDetectedAt);
            IsSandbox = isSandbox;
            BillingIssuesDetectedAt = FromISO8601(billingIssuesDetectedAt);
            GracePeriodExpiresDate = FromISO8601(gracePeriodExpiresDate);
            OwnershipType = ownershipType;
            PeriodType = periodType;
            RefundedAt = FromISO8601(refundedAt);
            StoreTransactionId = storeTransactionId;
            IsActive = isActive;
            WillRenew = willRenew;
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
    }
}

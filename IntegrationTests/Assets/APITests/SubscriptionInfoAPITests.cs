using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class SubscriptionInfoAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.SubscriptionInfo subscriptionInfo = new Purchases.SubscriptionInfo(null);
            string productIdentifier = subscriptionInfo.ProductIdentifier;
            DateTime purchaseDate = subscriptionInfo.PurchaseDate;
            DateTime? originalPurchaseDate = subscriptionInfo.OriginalPurchaseDate;
            DateTime? expiresDate = subscriptionInfo.ExpiresDate;
            string store = subscriptionInfo.Store;
            DateTime? unsubscribeDetectedAt = subscriptionInfo.UnsubscribeDetectedAt;
            bool isSandbox = subscriptionInfo.IsSandbox;
            DateTime? billingIssuesDetectedAt = subscriptionInfo.BillingIssuesDetectedAt;
            DateTime? gracePeriodExpiresDate = subscriptionInfo.GracePeriodExpiresDate;
            string ownershipType = subscriptionInfo.OwnershipType;
            string periodType = subscriptionInfo.PeriodType;
            DateTime? refundedAt = subscriptionInfo.RefundedAt;
            string? storeTransactionId = subscriptionInfo.StoreTransactionId;
            bool isActive = subscriptionInfo.IsActive;
            bool willRenew = subscriptionInfo.WillRenew;
        }
    }
}
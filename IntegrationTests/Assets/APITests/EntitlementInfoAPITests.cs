using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class EntitlementInfoAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.EntitlementInfo entitlementInfo = new Purchases.EntitlementInfo(null);
            string identifier = entitlementInfo.Identifier;
            bool isActive = entitlementInfo.IsActive;
            bool willRenew = entitlementInfo.WillRenew;
            string periodType = entitlementInfo.PeriodType;
            DateTime latestPurchaseDate = entitlementInfo.LatestPurchaseDate;
            DateTime originalPurchaseDate = entitlementInfo.OriginalPurchaseDate;
            DateTime? expirationDate = entitlementInfo.ExpirationDate;
            string store = entitlementInfo.Store;
            string productIdentifier = entitlementInfo.ProductIdentifier;
            bool isSandbox = entitlementInfo.IsSandbox;
            DateTime? unsubscribeDetectedAt = entitlementInfo.UnsubscribeDetectedAt;
            DateTime? billingIssueDetectedAt = entitlementInfo.BillingIssueDetectedAt;
            Purchases.VerificationResult verification = entitlementInfo.Verification;
        }
    }
}
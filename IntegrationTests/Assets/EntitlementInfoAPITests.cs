using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class EntitlementInfoAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.EntitlementInfo entitlementInfo = new Purchases.EntitlementInfo(null);
            string Identifier = entitlementInfo.Identifier;
            bool IsActive = entitlementInfo.IsActive;
            bool WillRenew = entitlementInfo.WillRenew;
            string PeriodType = entitlementInfo.PeriodType;
            DateTime LatestPurchaseDate = entitlementInfo.LatestPurchaseDate;
            DateTime OriginalPurchaseDate = entitlementInfo.OriginalPurchaseDate;
            DateTime? ExpirationDate = entitlementInfo.ExpirationDate;
            string Store = entitlementInfo.Store;
            string ProductIdentifier = entitlementInfo.ProductIdentifier;
            bool IsSandbox = entitlementInfo.IsSandbox;
            DateTime? UnsubscribeDetectedAt = entitlementInfo.UnsubscribeDetectedAt;
            DateTime? BillingIssueDetectedAt = entitlementInfo.BillingIssueDetectedAt;
        }
    }
}
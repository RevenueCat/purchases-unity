using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class CustomerInfoAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.CustomerInfo customerInfo = new Purchases.CustomerInfo(null);
            Purchases.EntitlementInfos entitlements = customerInfo.Entitlements;
            List<string> activeSubscriptions = customerInfo.ActiveSubscriptions;
            List<string> allPurchasedProductIdentifiers = customerInfo.AllPurchasedProductIdentifiers;
            DateTime? latestExpirationDate = customerInfo.LatestExpirationDate;
            DateTime firstSeen = customerInfo.FirstSeen;
            string originalAppUserId = customerInfo.OriginalAppUserId;
            DateTime requestDate = customerInfo.RequestDate;
            DateTime? originalPurchaseDate = customerInfo.OriginalPurchaseDate;
            Dictionary<string, DateTime?> allExpirationDates = customerInfo.AllExpirationDates;
            Dictionary<string, DateTime?> allPurchaseDates = customerInfo.AllPurchaseDates;
            string originalApplicationVersion = customerInfo.OriginalApplicationVersion;
            string managementURL = customerInfo.ManagementURL;
            List<Purchases.StoreTransaction> nonSubscriptionTransactions = customerInfo.NonSubscriptionTransactions;
        }
    }
}
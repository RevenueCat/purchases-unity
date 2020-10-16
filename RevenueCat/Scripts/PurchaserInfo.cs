using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;

public partial class Purchases
{
    /*
     * PurchaserInfo encapsulate the current status of subscriber. 
     * Use it to determine which entitlements to unlock, typically by checking 
     * ActiveSubscriptions or via LatestExpirationDate. 
     * 
     * Note: All DateTimes are in UTC, be sure to compare them with 
     * DateTime.UtcNow
     */
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class PurchaserInfo
    {
        public EntitlementInfos Entitlements;
        public List<string> ActiveSubscriptions;
        public List<string> AllPurchasedProductIdentifiers;
        public DateTime? LatestExpirationDate;
        public DateTime FirstSeen;
        public string OriginalAppUserId;
        public DateTime RequestDate;
        public DateTime? OriginalPurchaseDate;
        public Dictionary<string, DateTime?> AllExpirationDates;
        public Dictionary<string, DateTime> AllPurchaseDates;
        [CanBeNull] public string OriginalApplicationVersion;
        [CanBeNull] public string ManagementURL;
        public List<Transaction> NonSubscriptionTransactions;

        public PurchaserInfo(JSONNode response)
        {
            Entitlements = new EntitlementInfos(response["entitlements"]);
            ActiveSubscriptions = new List<string>();
            foreach (JSONNode subscription in response["activeSubscriptions"])
            {
                ActiveSubscriptions.Add(subscription);
            }

            AllPurchasedProductIdentifiers = new List<string>();
            foreach (JSONNode productIdentifier in response["allPurchasedProductIdentifiers"])
            {
                AllPurchasedProductIdentifiers.Add(productIdentifier);
            }

            FirstSeen = FromUnixTime(response["firstSeenMillis"].AsLong);
            OriginalAppUserId = response["originalAppUserId"];
            RequestDate = FromUnixTime(response["requestDateMillis"].AsLong);
            OriginalPurchaseDate =
                FromOptionalUnixTime(response["originalPurchaseDateMillis"].AsLong);
            LatestExpirationDate =
                FromOptionalUnixTime(response["latestExpirationDateMillis"].AsLong);
            ManagementURL = response["managementURL"];
            AllExpirationDates = new Dictionary<string, DateTime?>();
            foreach (var keyValue in response["allExpirationDatesMillis"])
            {
                var productID = keyValue.Key;
                var expirationDateJSON = keyValue.Value;
                if (expirationDateJSON != null && !expirationDateJSON.IsNull && expirationDateJSON.AsLong != 0L)
                {
                    AllExpirationDates.Add(productID, FromUnixTime(expirationDateJSON.AsLong));
                }
                else
                {
                    AllExpirationDates.Add(productID, null);
                }
            }

            AllPurchaseDates = new Dictionary<string, DateTime>();
            foreach (var keyValue in response["allPurchaseDatesMillis"])
            {
                AllPurchaseDates.Add(keyValue.Key, FromUnixTime(keyValue.Value.AsLong));
            }

            OriginalApplicationVersion = response["originalApplicationVersion"];
            
            NonSubscriptionTransactions = new List<Transaction>();
            foreach (JSONNode transactionResponse in response["nonSubscriptionTransactions"])
            {
                NonSubscriptionTransactions.Add(new Transaction(transactionResponse));
            }
        }

        public override string ToString()
        {
            return $"{nameof(Entitlements)}: {Entitlements}, " +
                   $"{nameof(ActiveSubscriptions)}: {ActiveSubscriptions}, " +
                   $"{nameof(AllPurchasedProductIdentifiers)}: {AllPurchasedProductIdentifiers}, " +
                   $"{nameof(LatestExpirationDate)}: {LatestExpirationDate}, " +
                   $"{nameof(FirstSeen)}: {FirstSeen}, " +
                   $"{nameof(OriginalAppUserId)}: {OriginalAppUserId}, " +
                   $"{nameof(RequestDate)}: {RequestDate}, " +
                   $"{nameof(AllExpirationDates)}: {AllExpirationDates}, " +
                   $"{nameof(AllPurchaseDates)}: {AllPurchaseDates}, " +
                   $"{nameof(OriginalPurchaseDate)}: {OriginalPurchaseDate}, " +
                   $"{nameof(ManagementURL)}: {ManagementURL}, " +
                   $"{nameof(NonSubscriptionTransactions)}: {NonSubscriptionTransactions}, " +
                   $"{nameof(OriginalApplicationVersion)}: {OriginalApplicationVersion}";
        }

    }
}
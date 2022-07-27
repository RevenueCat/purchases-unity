using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;
using static RevenueCat.Utilities;

public partial class Purchases
{
    ///
    /// <summary>
    /// CustomerInfo encapsulates the current status of subscriber. 
    /// Use it to determine which entitlements to unlock, typically by checking 
    /// ActiveSubscriptions or via LatestExpirationDate.
    /// </summary> 
    /// 
    /// <remarks>
    /// All DateTimes are in UTC, be sure to compare them with <c>DateTime.UtcNow</c>
    /// </remarks>
    ///
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class CustomerInfo
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
        public List<StoreTransaction> NonSubscriptionTransactions;

        public CustomerInfo(JSONNode response)
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

            FirstSeen = FromUnixTimeInMilliseconds(response["firstSeenMillis"].AsLong);
            OriginalAppUserId = response["originalAppUserId"];
            RequestDate = FromUnixTimeInMilliseconds(response["requestDateMillis"].AsLong);
            OriginalPurchaseDate =
                FromOptionalUnixTimeInMilliseconds(response["originalPurchaseDateMillis"].AsLong);
            LatestExpirationDate =
                FromOptionalUnixTimeInMilliseconds(response["latestExpirationDateMillis"].AsLong);
            ManagementURL = response["managementURL"];
            AllExpirationDates = new Dictionary<string, DateTime?>();
            foreach (var keyValue in response["allExpirationDatesMillis"])
            {
                var productID = keyValue.Key;
                var expirationDateJSON = keyValue.Value;
                if (expirationDateJSON != null && !expirationDateJSON.IsNull && expirationDateJSON.AsLong != 0L)
                {
                    AllExpirationDates.Add(productID, FromUnixTimeInMilliseconds(expirationDateJSON.AsLong));
                }
                else
                {
                    AllExpirationDates.Add(productID, null);
                }
            }

            AllPurchaseDates = new Dictionary<string, DateTime>();
            foreach (var keyValue in response["allPurchaseDatesMillis"])
            {
                AllPurchaseDates.Add(keyValue.Key, FromUnixTimeInMilliseconds(keyValue.Value.AsLong));
            }

            OriginalApplicationVersion = response["originalApplicationVersion"];
            
            NonSubscriptionTransactions = new List<StoreTransaction>();
            foreach (JSONNode transactionResponse in response["nonSubscriptionTransactions"])
            {
                NonSubscriptionTransactions.Add(new StoreTransaction(transactionResponse));
            }
        }

        public override string ToString()
        {
            return $"{nameof(Entitlements)}: {Entitlements}\n" +
                   $"{nameof(ActiveSubscriptions)}: {ListToString(ActiveSubscriptions)}\n" +
                   $"{nameof(AllPurchasedProductIdentifiers)}: {ListToString(AllPurchasedProductIdentifiers)}\n" +
                   $"{nameof(LatestExpirationDate)}: {LatestExpirationDate}\n" +
                   $"{nameof(FirstSeen)}: {FirstSeen}\n" +
                   $"{nameof(OriginalAppUserId)}: {OriginalAppUserId}\n" +
                   $"{nameof(RequestDate)}: {RequestDate}\n" +
                   $"{nameof(AllExpirationDates)}: {DictToString(AllExpirationDates)}\n" +
                   $"{nameof(AllPurchaseDates)}: {DictToString(AllPurchaseDates)}\n" +
                   $"{nameof(OriginalPurchaseDate)}: {OriginalPurchaseDate}\n" +
                   $"{nameof(ManagementURL)}: {ManagementURL}\n" +
                   $"{nameof(NonSubscriptionTransactions)}: {ListToString(NonSubscriptionTransactions)}\n" +
                   $"{nameof(OriginalApplicationVersion)}: {OriginalApplicationVersion}";
        }

    }
}
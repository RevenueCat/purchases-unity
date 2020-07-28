using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

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
            OriginalPurchaseDate = FromOptionalUnixTime(response["originalPurchaseDateMillis"].AsLong);
            LatestExpirationDate = FromOptionalUnixTime(response["latestExpirationDateMillis"].AsLong);
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
        } 
        
        private static DateTime? FromOptionalUnixTime(long unixTime)
         {
            DateTime? value = null;
             if (unixTime != 0L) { 
                value = FromUnixTime(unixTime);
             }
             return value;
         }

        private static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
                   $"{nameof(OriginalApplicationVersion)}: {OriginalApplicationVersion}";
        }
    }
}
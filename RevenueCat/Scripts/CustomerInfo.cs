using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static RevenueCat.Utilities;

namespace RevenueCat
{
    /// <summary>
    /// CustomerInfo encapsulates the current status of subscriber. 
    /// Use it to determine which entitlements to unlock, typically by checking 
    /// ActiveSubscriptions or via LatestExpirationDate.
    /// </summary> 
    /// <remarks>
    /// All DateTimes are in UTC, be sure to compare them with <c>DateTime.UtcNow</c>
    /// </remarks>
    public sealed class CustomerInfo
    {
        [JsonProperty("entitlements")]
        public EntitlementInfos Entitlements { get; }

        [JsonProperty("activeSubscriptions")]
        public IReadOnlyList<string> ActiveSubscriptions { get; }

        [JsonProperty("allPurchasedProductIdentifiers")]
        public IReadOnlyList<string> AllPurchasedProductIdentifiers { get; }

        [JsonProperty("latestExpirationDateMillis")]
        public long? LatestExpirationDateMillis { get; }

        [JsonIgnore]
        public DateTime? LatestExpirationDate
            => FromOptionalUnixTimeInMilliseconds(LatestExpirationDateMillis);

        [JsonProperty("firstSeenMillis")]
        public long FirstSeenMillis { get; }

        [JsonIgnore]
        public DateTime FirstSeen
            => FromUnixTimeInMilliseconds(FirstSeenMillis);

        [JsonProperty("originalAppUserId")]
        public string OriginalAppUserId { get; }

        [JsonProperty("requestDateMillis")]
        public long RequestDateMillis { get; }

        [JsonIgnore]
        public DateTime RequestDate
            => FromUnixTimeInMilliseconds(RequestDateMillis);

        [JsonProperty("originalPurchaseDateMillis")]
        public long? OriginalPurchaseDateMillis { get; }

        [JsonIgnore]
        public DateTime? OriginalPurchaseDate
            => FromOptionalUnixTimeInMilliseconds(OriginalPurchaseDateMillis);

        [JsonProperty("allExpirationDatesMillis")]
        public IReadOnlyDictionary<string, long?> AllExpirationDatesMillis { get; }

        [JsonIgnore]
        public IReadOnlyDictionary<string, DateTime?> AllExpirationDates { get; }

        [JsonProperty("allPurchaseDatesMillis")]
        public IReadOnlyDictionary<string, long?> AllPurchaseDatesMillis { get; }

        [JsonIgnore]
        public IReadOnlyDictionary<string, DateTime?> AllPurchaseDates { get; }

        [CanBeNull]
        [JsonProperty("originalApplicationVersion")]
        public string OriginalApplicationVersion { get; }

        [CanBeNull]
        [JsonProperty("managementURL")]
        public string ManagementURL { get; }

        [JsonProperty("nonSubscriptionTransactions")]
        public IReadOnlyList<StoreTransaction> NonSubscriptionTransactions { get; }

        [JsonProperty("subscriptionsByProductIdentifier")]
        public IReadOnlyDictionary<string, SubscriptionInfo> SubscriptionsByProductIdentifier { get; }

        [JsonConstructor]
        internal CustomerInfo(
            [JsonProperty("entitlements")] EntitlementInfos entitlements,
            [JsonProperty("activeSubscriptions")] List<string> activeSubscriptions,
            [JsonProperty("allPurchasedProductIdentifiers")] List<string> allPurchasedProductIdentifiers,
            [JsonProperty("latestExpirationDateMillis")] long? latestExpirationDateMillis,
            [JsonProperty("firstSeenMillis")] long firstSeenMillis,
            [JsonProperty("originalAppUserId")] string originalAppUserId,
            [JsonProperty("requestDateMillis")] long requestDateMillis,
            [JsonProperty("originalPurchaseDateMillis")] long? originalPurchaseDateMillis,
            [JsonProperty("allExpirationDatesMillis")] Dictionary<string, long?> allExpirationDatesMillis,
            [JsonProperty("allPurchaseDatesMillis")] Dictionary<string, long?> allPurchaseDatesMillis,
            [JsonProperty("originalApplicationVersion")] string originalApplicationVersion,
            [JsonProperty("managementURL")] string managementURL,
            [JsonProperty("nonSubscriptionTransactions")] List<StoreTransaction> nonSubscriptionTransactions,
            [JsonProperty("subscriptionsByProductIdentifier")] Dictionary<string, SubscriptionInfo> subscriptionsByProductIdentifier)
        {
            Entitlements = entitlements;
            ActiveSubscriptions = activeSubscriptions;
            AllPurchasedProductIdentifiers = allPurchasedProductIdentifiers;
            LatestExpirationDateMillis = latestExpirationDateMillis;
            FirstSeenMillis = firstSeenMillis;
            OriginalAppUserId = originalAppUserId;
            RequestDateMillis = requestDateMillis;
            OriginalPurchaseDateMillis = originalPurchaseDateMillis;
            AllExpirationDatesMillis = allExpirationDatesMillis;

            var allExpirationDates = new Dictionary<string, DateTime?>();

            foreach (var (productId, value) in allExpirationDatesMillis)
            {
                allExpirationDates.Add(productId, FromOptionalUnixTimeInMilliseconds(value));
            }

            AllExpirationDates = allExpirationDates;

            var allPurchaseDates = new Dictionary<string, DateTime?>();
            AllPurchaseDatesMillis = allPurchaseDatesMillis;

            foreach (var (productId, value) in allPurchaseDatesMillis)
            {
                allExpirationDates.Add(productId, FromOptionalUnixTimeInMilliseconds(value));
            }

            AllExpirationDates = allPurchaseDates;
            OriginalApplicationVersion = originalApplicationVersion;
            ManagementURL = managementURL;
            NonSubscriptionTransactions = nonSubscriptionTransactions;
            SubscriptionsByProductIdentifier = subscriptionsByProductIdentifier;
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
                   $"{nameof(SubscriptionsByProductIdentifier)}: {DictToString(SubscriptionsByProductIdentifier)}\n" +
                   $"{nameof(OriginalApplicationVersion)}: {OriginalApplicationVersion}";
        }

    }
}
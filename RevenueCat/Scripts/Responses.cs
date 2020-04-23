using System;
using System.Collections.Generic;
using JetBrains.Annotations;

public partial class Purchases
{
    [Serializable]
    private class ReceiveProductsResponse
    {
        public List<ProductResponse> products;
        public Error error;
    }
        [Serializable]
    private class ReceivePurchaserInfoResponse
    {
        public PurchaserInfoResponse purchaserInfo;
        public Error error;
    }

    [Serializable]
    private class MakePurchaseResponse
    {
        public string productIdentifier;
        public PurchaserInfoResponse purchaserInfo;
        public bool userCancelled;
        public Error error;
    }

    [Serializable]
    public class PurchaserInfoResponse
    {
        public EntitlementInfosResponse entitlements;
        public List<string> activeSubscriptions;
        public List<string> allPurchasedProductIdentifiers;
        public long latestExpirationDateMillis;
        public long firstSeenMillis;
        public string originalAppUserId;
        public long requestDateMillis;
        public List<string> allExpirationDatesMillisKeys;
        public List<long> allExpirationDatesMillisValues;
        public List<string> allPurchaseDatesMillisKeys;
        public List<long> allPurchaseDatesMillisValues;
        public string originalApplicationVersion;
    }

    [Serializable]
    public class OfferingsResponse
    {
        public List<string> allKeys;
        public List<OfferingResponse> allValues;
        public OfferingResponse current;
    }

    [Serializable]
    public class GetOfferingsResponse
    {
        public OfferingsResponse offerings;
        public Error error;
    }

    [Serializable]
    public class OfferingResponse
    {
        public string identifier;
        public string serverDescription;
        public List<PackageResponse> availablePackages;
        [CanBeNull] public PackageResponse lifetime;
        [CanBeNull] public PackageResponse annual;
        [CanBeNull] public PackageResponse sixMonth;
        [CanBeNull] public PackageResponse threeMonth;
        [CanBeNull] public PackageResponse twoMonth;
        [CanBeNull] public PackageResponse monthly;
        [CanBeNull] public PackageResponse weekly;
    }

    [Serializable]
    public class PackageResponse
    {
        public string identifier;
        public string packageType;
        public ProductResponse product;
        public string offeringIdentifier;
    }

    [Serializable]
    public class EntitlementInfosResponse
    {
        public List<string> allKeys;
        public List<EntitlementInfoResponse> allValues;
        public List<string> activeKeys;
        public List<EntitlementInfoResponse> activeValues;
    }

    [Serializable]
    public class EntitlementInfoResponse
    {
        public string identifier;
        public bool isActive;
        public bool willRenew;
        public string periodType;
        public long latestPurchaseDateMillis;
        public long originalPurchaseDateMillis;
        [CanBeNull] public long expirationDateMillis;
        public string store;
        public string productIdentifier;
        public bool isSandbox;
        [CanBeNull] public long unsubscribeDetectedAtMillis;
        [CanBeNull] public long billingIssueDetectedAtMillis;
    }

    [Serializable]
    public class IntroEligibilityResponse
    {
        public int status;
        public string description;
    }

    [Serializable]
    public class MapResponse<K, V>
    {
        public List<K> keys;
        public List<V> values;
    }

    [Serializable]
    public class ProductResponse
    {
        public string title;
        public string identifier;
        public string description;
        public float price;
        public string price_string;
        [CanBeNull] public string currency_code;
        public float intro_price;
        public string intro_price_string;
        public string intro_price_period;
        public string intro_price_period_unit;
        [CanBeNull] public int intro_price_period_number_of_units;
        [CanBeNull] public int intro_price_cycles;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
        private readonly PurchaserInfoResponse _response;

        public PurchaserInfo(PurchaserInfoResponse response)
        {
            _response = response;
        }

        public EntitlementInfos Entitlements
        {
            get { return new EntitlementInfos(_response.entitlements); }
        }

        public List<string> ActiveSubscriptions
        {
            get { return _response.activeSubscriptions; }
        }

        public List<string> AllPurchasedProductIdentifiers
        {
            get { return _response.allPurchasedProductIdentifiers; }
        }

        public DateTime? LatestExpirationDate
        {
            get
            {
                if (_response.latestExpirationDateMillis != 0L)
                {
                    return FromUnixTime(_response.latestExpirationDateMillis);
                }
                else
                {
                    return null;
                }
            }
        }

        public DateTime FirstSeen
        {            
            get { return FromUnixTime(_response.firstSeenMillis); }
        }

        public string OriginalAppUserId
        {
            get { return _response.originalAppUserId; }
        }

        public DateTime RequestDate
        {
            get { return FromUnixTime(_response.requestDateMillis); }
        }

        public Dictionary<string, DateTime?> AllExpirationDates
        {
            get
            {
                var allExpirations = new Dictionary<string, DateTime?>();
                for (var i = 0; i < _response.allExpirationDatesMillisKeys.Count; i++)
                {
                    if (_response.allExpirationDatesMillisValues[i] != 0L)
                    {
                        var date = FromUnixTime(_response.allExpirationDatesMillisValues[i]);
                        allExpirations[_response.allExpirationDatesMillisKeys[i]] = date;
                    }
                    else
                    {
                        allExpirations[_response.allExpirationDatesMillisKeys[i]] = null;
                    }
                }

                return allExpirations;
            }
        }

        public Dictionary<string, DateTime> AllPurchaseDates
        {
            get
            {
                var allPurchases = new Dictionary<string, DateTime>();
                for (var i = 0; i < _response.allPurchaseDatesMillisKeys.Count; i++)
                {
                    var date = FromUnixTime(_response.allPurchaseDatesMillisValues[i]);
                    allPurchases[_response.allPurchaseDatesMillisKeys[i]] = date;
                }

                return allPurchases;
            }
        }

        public string OriginalApplicationVersion
        {
            get { return _response.originalApplicationVersion; }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    }
}
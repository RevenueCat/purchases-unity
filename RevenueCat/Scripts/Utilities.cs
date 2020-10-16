using System;

namespace RevenueCat
{
    internal static class Utilities {
        internal static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }
    
        internal static DateTime? FromOptionalUnixTime(long unixTime)
        {
            DateTime? value = null;
            if (unixTime != 0L) { 
                value = FromUnixTime(unixTime);
            }
            return value;
        }
    
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    }
}

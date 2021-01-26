using System;

namespace RevenueCat
{
    internal static class Utilities {
        internal static DateTime FromUnixTimeInMilliseconds(long unixTimeInMilliseconds)
        {
            return Epoch.AddSeconds(unixTimeInMilliseconds / 1000.0);
        }
    
        internal static DateTime? FromOptionalUnixTimeInMilliseconds(long unixTimeInMilliseconds)
        {
            DateTime? value = null;
            if (unixTimeInMilliseconds != 0L) { 
                value = FromUnixTimeInMilliseconds(unixTimeInMilliseconds);
            }
            return value;
        }
    
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    }
}

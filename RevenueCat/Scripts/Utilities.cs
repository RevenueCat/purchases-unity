using System;
using System.Collections.Generic;
using System.Linq;

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

        internal static String DictToString<T>(Dictionary<string, T> dictionary)
        {
            var items = dictionary.Select(kvp => string.Format($"{kvp.Key} : Id={kvp.Value.ToString()}"));
            return $"{{ \n { string.Join(Environment.NewLine, items) }\n }} \n ";
        }
    
        internal static String ListToString<T>(List<T> list)
        {
            var items = list.Select(arg => $"{arg.ToString()}");
            return $"{{ \n { string.Join(Environment.NewLine, items) }\n }} \n";
        }
    }
}

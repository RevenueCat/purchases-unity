using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RevenueCat
{
    internal static class Utilities
    {
        internal static bool IsAndroidEmulator()
        {
            try
            {
                // From https://stackoverflow.com/questions/51880866/detect-if-game-running-in-android-emulator
                var osBuild = new AndroidJavaClass("android.os.Build");
                var fingerPrint = osBuild.GetStatic<string>("FINGERPRINT");
                return fingerPrint.Contains("generic");
            }
            catch
            {
                // Throws error when running on non-Android platforms
                return false;
            }
        }

        internal static DateTime FromUnixTimeInMilliseconds(long unixTimeInMilliseconds)
        {
            return Epoch.AddSeconds(unixTimeInMilliseconds / 1000.0);
        }

        internal static DateTime? FromOptionalUnixTimeInMilliseconds(long? unixTimeInMilliseconds)
        {
            DateTime? value = null;

            if (unixTimeInMilliseconds.HasValue)
            {
                value = FromUnixTimeInMilliseconds(unixTimeInMilliseconds.Value);
            }

            return value;
        }

        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static String DictToString<T>(IReadOnlyDictionary<string, T> dictionary)
        {
            var items = dictionary.Select(kvp => $"{kvp.Key} : Id={kvp.Value.ToString()}");
            return $"{{ \n {string.Join(Environment.NewLine, items)}\n }} \n ";
        }

        internal static String ListToString<T>(IEnumerable<T> list)
        {
            var items = list.Select(arg => $"{arg.ToString()}");
            return $"{{ \n {string.Join(Environment.NewLine, items)}\n }} \n";
        }

        internal static DateTime? FromISO8601(string iso8601)
        {
            if (string.IsNullOrWhiteSpace(iso8601))
            {
                return null;
            }
            try
            {
                return DateTime.Parse(iso8601);
            }
            catch (FormatException e)
            {
                Debug.Log($"Error parsing ISO8601 date: {iso8601}: {e.Message}");
                return null;
            }
        }
    }
}

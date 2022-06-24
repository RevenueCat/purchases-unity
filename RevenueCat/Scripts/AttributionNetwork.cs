using System;
using System.Diagnostics.CodeAnalysis;

public partial class Purchases
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [Obsolete("AttributionNetwork is deprecated, please use Set<NetworkID> methods instead.", true)]
    public enum AttributionNetwork
    {
        APPLE_SEARCH_ADS = 0,
        ADJUST = 1,
        APPSFLYER = 2,
        BRANCH = 3,
        TENJIN = 4,
        FACEBOOK = 5
    }
}
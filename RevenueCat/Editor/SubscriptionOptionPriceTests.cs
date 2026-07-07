#if UNITY_EDITOR
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCat.Editor
{
    internal static class SubscriptionOptionPriceTests
    {
        [UnityEditor.InitializeOnLoadMethod]
        private static void VerifyAmountMicrosParsing()
        {
            var priceJson = JSON.Parse(
                @"{""formatted"":""¥64,500"",""amountMicros"":64500000000,""currencyCode"":""JPY""}");
            var price = new Purchases.SubscriptionOption.Price(priceJson);

            if (price.AmountMicros != 64500000000L)
            {
                Debug.LogError(
                    $"Expected AmountMicros 64500000000 but got {price.AmountMicros}");
            }
        }
    }
}
#endif

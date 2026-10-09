using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class NativeBridgeRequestIdContractTests
{
    [Test]
    public void IosRequestAwareResponseNormalizesNilRequestId()
    {
        var source = File.ReadAllText(
            Path.Combine(
                Application.dataPath,
                "RevenueCat/Plugins/iOS/PurchasesUnityHelper.m"));
        var helper = ExtractBlock(
            source,
            "- (void)sendJSONObject:(nullable NSDictionary *)jsonObject");

        Assert.That(
            helper,
            Does.Contain(
                "response[RCCallbackRequestIdKey] = requestId ?: @\"\";"));
    }

    private static string ExtractBlock(string source, string marker)
    {
        var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
        Assert.That(markerIndex, Is.GreaterThanOrEqualTo(0));
        var openingBrace = source.IndexOf('{', markerIndex);
        Assert.That(openingBrace, Is.GreaterThanOrEqualTo(0));

        var depth = 0;
        for (var index = openingBrace; index < source.Length; index++)
        {
            if (source[index] == '{')
            {
                depth++;
            }
            else if (source[index] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source.Substring(
                        markerIndex,
                        index - markerIndex + 1);
                }
            }
        }

        Assert.Fail($"Unclosed block after marker '{marker}'.");
        return null;
    }
}

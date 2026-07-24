using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class NativeBridgeParseFailureContractTests
{
    private string _androidSource;
    private string _iosSource;

    [SetUp]
    public void SetUp()
    {
        _androidSource = File.ReadAllText(
            Path.Combine(
                Application.dataPath,
                "RevenueCat/Plugins/Android/PurchasesWrapper.java"));
        _iosSource = File.ReadAllText(
            Path.Combine(
                Application.dataPath,
                "RevenueCat/Plugins/iOS/PurchasesUnityHelper.m"));
    }

    [Test]
    public void AndroidGetProductsParseFailureReturnsEmptyProducts()
    {
        var branch = GetLastJavaJsonCatch(
            "public static void getProducts(");

        AssertAndroidFallback(
            branch,
            "Failure parsing product identifiers",
            "RECEIVE_PRODUCTS");
        Assert.That(branch, Does.Contain("\"products\""));
        Assert.That(branch, Does.Contain("new JSONArray()"));
    }

    [Test]
    public void AndroidPurchasePackageParseFailureReturnsEmptyPurchase()
    {
        var branch = GetLastJavaJsonCatch(
            "public static void purchasePackage(String packageIdentifier,");

        AssertAndroidFallback(branch, "logJSONException(e);", "MAKE_PURCHASE");
        Assert.That(
            branch,
            Does.Contain("sendEmptyJSONObject(MAKE_PURCHASE, requestId);"));
    }

    [Test]
    public void AndroidEligibilityParseFailureReturnsEmptyObject()
    {
        var branch = GetLastJavaJsonCatch(
            "public static void checkTrialOrIntroductoryPriceEligibility(");

        AssertAndroidFallback(
            branch,
            "Failure parsing product identifiers",
            "CHECK_ELIGIBILITY");
        Assert.That(
            branch,
            Does.Contain("sendEmptyJSONObject(CHECK_ELIGIBILITY, requestId);"));
    }

    [Test]
    public void AndroidCanMakePaymentsParseFailureReturnsFalse()
    {
        var branch = GetLastJavaJsonCatch(
            "public static void canMakePayments(");

        AssertAndroidFallback(branch, "logJSONException(e);", "CAN_MAKE_PAYMENTS");
        Assert.That(branch, Does.Contain("\"canMakePayments\""));
        Assert.That(branch, Does.Contain("false"));
    }

    [Test]
    public void IosGetProductsParseFailureReturnsEmptyProducts()
    {
        var branch = GetObjectiveCErrorBranch("void _RCGetProducts(");

        AssertIosFallback(
            branch,
            "Error parsing productIdentifiers JSON",
            "RECEIVE_PRODUCTS",
            "getProducts:");
        Assert.That(branch, Does.Contain("@\"products\": @[]"));
    }

    [Test]
    public void IosPurchasePackageParseFailureReturnsEmptyPurchase()
    {
        var branch = GetObjectiveCErrorBranch("void _RCPurchasePackage(");

        AssertIosFallback(
            branch,
            "Error parsing presentedOfferingContext JSON",
            "MAKE_PURCHASE",
            "purchasePackage:");
        Assert.That(branch, Does.Contain("sendJSONObject:nil"));
    }

    [Test]
    public void IosEligibilityParseFailureReturnsEmptyObject()
    {
        var branch = GetObjectiveCErrorBranch(
            "void _RCCheckTrialOrIntroductoryPriceEligibility(");

        AssertIosFallback(
            branch,
            "Error parsing productIdentifiers JSON",
            "CHECK_ELIGIBILITY",
            "checkTrialOrIntroductoryPriceEligibility:");
        Assert.That(branch, Does.Contain("sendJSONObject:nil"));
    }

    [Test]
    public void IosCanMakePaymentsParseFailureReturnsFalse()
    {
        var branch = GetObjectiveCErrorBranch("void _RCCanMakePayments(");

        AssertIosFallback(
            branch,
            "Error parsing features JSON",
            "CAN_MAKE_PAYMENTS",
            "canMakePaymentsWithFeatures:");
        Assert.That(branch, Does.Contain("@\"canMakePayments\": @NO"));
    }

    [Test]
    public void IosRequestAwareResponseNormalizesNilRequestId()
    {
        var helper = ExtractBlock(
            _iosSource,
            "- (void)sendJSONObject:(nullable NSDictionary *)jsonObject");

        Assert.That(
            helper,
            Does.Contain(
                "response[RCCallbackRequestIdKey] = requestId ?: @\"\";"));
    }

    private string GetLastJavaJsonCatch(string methodSignature)
    {
        var method = ExtractBlock(_androidSource, methodSignature);
        var catchIndex = method.LastIndexOf(
            "catch (JSONException e)",
            StringComparison.Ordinal);
        Assert.That(catchIndex, Is.GreaterThanOrEqualTo(0));
        return ExtractBlock(method, "catch (JSONException e)", catchIndex);
    }

    private string GetObjectiveCErrorBranch(string functionSignature)
    {
        var function = ExtractBlock(_iosSource, functionSignature);
        return ExtractBlock(function, "if (error)");
    }

    private static void AssertAndroidFallback(
        string branch,
        string logMarker,
        string responseMethod)
    {
        Assert.That(branch, Does.Contain(logMarker));
        Assert.That(branch, Does.Contain(responseMethod));
        Assert.That(branch, Does.Contain("requestId"));
        Assert.That(
            CountOccurrences(branch, "sendJSONObject(") +
            CountOccurrences(branch, "sendEmptyJSONObject("),
            Is.EqualTo(1));
        Assert.That(branch, Does.Not.Contain("CommonKt."));
    }

    private static void AssertIosFallback(
        string branch,
        string logMarker,
        string responseMethod,
        string nativeOperation)
    {
        Assert.That(branch, Does.Contain(logMarker));
        Assert.That(branch, Does.Contain(responseMethod));
        Assert.That(
            branch,
            Does.Contain("requestId:convertCString(requestId)"));
        Assert.That(
            CountOccurrences(branch, "sendJSONObject:"),
            Is.EqualTo(1));
        Assert.That(branch, Does.Not.Contain(nativeOperation));
    }

    private static string ExtractBlock(
        string source,
        string marker,
        int startIndex = 0)
    {
        var markerIndex = source.IndexOf(
            marker,
            startIndex,
            StringComparison.Ordinal);
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

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(
                   value,
                   index,
                   StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}

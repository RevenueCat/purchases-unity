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
        var function = GetAndroidFunction(
            "public static void getProducts(");
        var branch = GetLastJavaJsonCatch(function);

        AssertAndroidFallback(
            function,
            "Failure parsing product identifiers",
            "RECEIVE_PRODUCTS");
        Assert.That(branch, Does.Contain("\"products\""));
        Assert.That(branch, Does.Contain("new JSONArray()"));
        Assert.That(
            branch,
            Does.Contain(
                "sendJSONObject(response, RECEIVE_PRODUCTS, requestId);"));
    }

    [Test]
    public void AndroidPurchasePackageParseFailureReturnsEmptyPurchase()
    {
        var function = GetAndroidFunction(
            "public static void purchasePackage(String packageIdentifier,");
        var branch = GetLastJavaJsonCatch(function);

        AssertAndroidFallback(
            function,
            "logJSONException(e);",
            "MAKE_PURCHASE");
        Assert.That(
            branch,
            Does.Contain("sendEmptyJSONObject(MAKE_PURCHASE, requestId);"));
    }

    [Test]
    public void AndroidEligibilityParseFailureReturnsEmptyObject()
    {
        var function = GetAndroidFunction(
            "public static void checkTrialOrIntroductoryPriceEligibility(");
        var branch = GetLastJavaJsonCatch(function);

        AssertAndroidFallback(
            function,
            "Failure parsing product identifiers",
            "CHECK_ELIGIBILITY");
        Assert.That(
            branch,
            Does.Contain("sendEmptyJSONObject(CHECK_ELIGIBILITY, requestId);"));
    }

    [Test]
    public void AndroidCanMakePaymentsParseFailureReturnsFalse()
    {
        var function = GetAndroidFunction(
            "public static void canMakePayments(");
        var branch = GetLastJavaJsonCatch(function);

        AssertAndroidFallback(
            function,
            "logJSONException(e);",
            "CAN_MAKE_PAYMENTS");
        Assert.That(branch, Does.Contain("\"canMakePayments\""));
        Assert.That(branch, Does.Contain("false"));
        Assert.That(
            branch,
            Does.Contain(
                "sendJSONObject(response, CAN_MAKE_PAYMENTS, requestId);"));
    }

    [Test]
    public void IosGetProductsParseFailureReturnsEmptyProducts()
    {
        var function = GetIosFunction("void _RCGetProducts(");
        var branch = GetObjectiveCErrorBranch(function);

        AssertIosFallback(
            function,
            "Error parsing productIdentifiers JSON",
            "RECEIVE_PRODUCTS",
            "getProducts:");
        Assert.That(branch, Does.Contain("@\"products\": @[]"));
    }

    [Test]
    public void IosPurchasePackageParseFailureReturnsEmptyPurchase()
    {
        var function = GetIosFunction("void _RCPurchasePackage(");
        var branch = GetObjectiveCErrorBranch(function);

        AssertIosFallback(
            function,
            "Error parsing presentedOfferingContext JSON",
            "MAKE_PURCHASE",
            "purchasePackage:");
        Assert.That(branch, Does.Contain("sendJSONObject:nil"));
    }

    [Test]
    public void IosEligibilityParseFailureReturnsEmptyObject()
    {
        var function = GetIosFunction(
            "void _RCCheckTrialOrIntroductoryPriceEligibility(");
        var branch = GetObjectiveCErrorBranch(function);

        AssertIosFallback(
            function,
            "Error parsing productIdentifiers JSON",
            "CHECK_ELIGIBILITY",
            "checkTrialOrIntroductoryPriceEligibility:");
        Assert.That(branch, Does.Contain("sendJSONObject:nil"));
    }

    [Test]
    public void IosCanMakePaymentsParseFailureReturnsFalse()
    {
        var function = GetIosFunction("void _RCCanMakePayments(");
        var branch = GetObjectiveCErrorBranch(function);

        AssertIosFallback(
            function,
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

    private string GetAndroidFunction(string methodSignature)
    {
        return ExtractBlock(_androidSource, methodSignature);
    }

    private string GetIosFunction(string functionSignature)
    {
        return ExtractBlock(_iosSource, functionSignature);
    }

    private static string GetLastJavaJsonCatch(string function)
    {
        var catchIndex = function.LastIndexOf(
            "catch (JSONException e)",
            StringComparison.Ordinal);
        Assert.That(catchIndex, Is.GreaterThanOrEqualTo(0));
        return ExtractBlock(
            function,
            "catch (JSONException e)",
            catchIndex);
    }

    private static string GetObjectiveCErrorBranch(string function)
    {
        return ExtractBlock(function, "if (error)");
    }

    private static void AssertAndroidFallback(
        string function,
        string logMarker,
        string responseMethod)
    {
        var branch = GetLastJavaJsonCatch(function);
        Assert.That(branch, Does.Contain(logMarker));
        Assert.That(branch, Does.Contain(responseMethod));
        Assert.That(branch, Does.Contain("requestId"));
        Assert.That(
            CountOccurrences(branch, "sendJSONObject(") +
            CountOccurrences(branch, "sendEmptyJSONObject("),
            Is.EqualTo(1));
        Assert.That(branch, Does.Not.Contain("CommonKt."));

        var branchIndex = function.LastIndexOf(
            branch,
            StringComparison.Ordinal);
        var afterCatch = function.Substring(branchIndex + branch.Length);
        Assert.That(
            afterCatch.Trim(),
            Is.EqualTo("}"),
            "The parse-failure catch must terminate the Java function.");
    }

    private static void AssertIosFallback(
        string function,
        string logMarker,
        string responseMethod,
        string nativeOperation)
    {
        var branch = GetObjectiveCErrorBranch(function);
        Assert.That(branch, Does.Contain(logMarker));
        Assert.That(branch, Does.Contain(responseMethod));
        Assert.That(
            branch,
            Does.Contain("requestId:convertCString(requestId)"));
        Assert.That(
            CountOccurrences(branch, "sendJSONObject:"),
            Is.EqualTo(1));
        Assert.That(branch, Does.Not.Contain(nativeOperation));

        var responseIndex = branch.IndexOf(
            "sendJSONObject:",
            StringComparison.Ordinal);
        var returnIndex = branch.IndexOf(
            "return;",
            responseIndex,
            StringComparison.Ordinal);
        Assert.That(
            returnIndex,
            Is.GreaterThan(responseIndex),
            "The parse-failure response must be followed by a return.");
        Assert.That(
            CountOccurrences(branch, "return;"),
            Is.EqualTo(1));
        Assert.That(
            branch.Substring(returnIndex + "return;".Length).Trim(),
            Is.EqualTo("}"),
            "The return must terminate the iOS parse-failure branch.");
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

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PurchasesCallbackRoutingTests
{
    private GameObject _gameObject;
    private Purchases _purchases;

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject("PurchasesCallbackRoutingTests");
        _purchases = _gameObject.AddComponent<Purchases>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_gameObject);
    }

    [Test]
    public void StorefrontResponsesInvokeMatchingCallbacksOutOfOrder()
    {
        var received = new List<string>();
        var firstId = _purchases.RegisterCallback<Purchases.GetStorefrontFunc>(
            storefront => received.Add($"first:{storefront.CountryCode}"));
        var secondId = _purchases.RegisterCallback<Purchases.GetStorefrontFunc>(
            storefront => received.Add($"second:{storefront.CountryCode}"));

        LogAssert.Expect(
            LogType.Assert,
            "Assertion failed on expression: 'ShouldRunBehaviour()'");
        _purchases.SendMessage(
            "_receiveStorefront",
            $"{{\"requestId\":\"{secondId}\",\"countryCode\":\"US\"}}");
        LogAssert.Expect(
            LogType.Assert,
            "Assertion failed on expression: 'ShouldRunBehaviour()'");
        _purchases.SendMessage(
            "_receiveStorefront",
            $"{{\"requestId\":\"{firstId}\",\"countryCode\":\"CA\"}}");

        CollectionAssert.AreEqual(
            new[] { "second:US", "first:CA" },
            received);
    }

    [Test]
    public void DuplicateResponseDoesNotInvokeCallbackTwice()
    {
        var calls = 0;
        var requestId = _purchases.RegisterCallback<Purchases.GetStorefrontFunc>(
            _ => calls++);
        var response =
            $"{{\"requestId\":\"{requestId}\",\"countryCode\":\"US\"}}";

        LogAssert.Expect(
            LogType.Assert,
            "Assertion failed on expression: 'ShouldRunBehaviour()'");
        _purchases.SendMessage("_receiveStorefront", response);
        LogAssert.Expect(
            LogType.Assert,
            "Assertion failed on expression: 'ShouldRunBehaviour()'");
        LogAssert.Expect(
            LogType.Warning,
            $"No pending callback found for request ID '{requestId}'.");
        _purchases.SendMessage("_receiveStorefront", response);

        Assert.That(calls, Is.EqualTo(1));
    }
}

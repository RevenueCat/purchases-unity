using System;
using NUnit.Framework;
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class IntroductoryOfferEligibilityJWSTests
    {
        private sealed class EligibilityWrapper : PurchasesWrapperSpy, IIntroductoryOfferEligibilityPurchasesWrapper
        {
            internal string Token;
            internal string Product;
            internal Purchases.Package Package;
            internal int Calls;
            public void PurchaseProductWithIntroductoryOfferEligibilityJWS(string productIdentifier, string introductoryOfferEligibilityJWS)
            { Product = productIdentifier; Token = introductoryOfferEligibilityJWS; Calls++; }
            public void PurchasePackageWithIntroductoryOfferEligibilityJWS(Purchases.Package package, string introductoryOfferEligibilityJWS)
            { Package = package; Token = introductoryOfferEligibilityJWS; Calls++; }
        }
        private GameObject _gameObject;
        private Purchases _purchases;
        private EligibilityWrapper _wrapper;
        private const string Token = "header.deny.signature";

        [SetUp] public void SetUp()
        {
            _gameObject = new GameObject("EligibilityTests");
            _purchases = _gameObject.AddComponent<Purchases>();
            _wrapper = new EligibilityWrapper();
            _purchases.SetWrapper(_wrapper);
        }
        [TearDown] public void TearDown() { UnityEngine.Object.DestroyImmediate(_gameObject); }

        [Test] public void ForwardsProductTokenWithoutRegularPurchase()
        {
            _purchases.PurchaseProductWithIntroductoryOfferEligibilityJWS("product", Token, _ => { });
            Assert.That(_wrapper.Token, Is.EqualTo(Token));
            Assert.That(_wrapper.Product, Is.EqualTo("product"));
            Assert.That(_wrapper.Calls, Is.EqualTo(1));
            Assert.That(_wrapper.Invocations, Is.Empty);
        }
        [Test] public void ForwardsPackageAndTokenWithoutRegularPurchase()
        {
            var package = new Purchases.Package(JSON.Parse("{\"identifier\":\"monthly\",\"packageType\":\"MONTHLY\",\"product\":{},\"presentedOfferingContext\":{\"offeringIdentifier\":\"offering\"}}"));
            _purchases.PurchasePackageWithIntroductoryOfferEligibilityJWS(package, Token, _ => { });
            Assert.That(_wrapper.Package, Is.SameAs(package));
            Assert.That(_wrapper.Token, Is.EqualTo(Token));
            Assert.That(_wrapper.Invocations, Is.Empty);
        }
        [TestCase(null)] [TestCase("")] public void RejectsMissingOrEmptyTokenBeforeWrapper(string token)
        {
            Assert.Throws<ArgumentException>(() => _purchases.PurchaseProductWithIntroductoryOfferEligibilityJWS("product", token, _ => { }));
            Assert.That(_wrapper.Calls, Is.Zero);
            Assert.That(_wrapper.Invocations, Is.Empty);
        }
        [Test] public void ExistingWrappersRemainCompatibleAndRejectNewCapability()
        {
            var legacy = new PurchasesWrapperSpy();
            _purchases.SetWrapper(legacy);
            Assert.Throws<PlatformNotSupportedException>(() => _purchases.PurchaseProductWithIntroductoryOfferEligibilityJWS("product", Token, _ => { }));
            Assert.That(legacy.Invocations, Is.Empty);
            _purchases.PurchaseProduct("product", _ => { });
            Assert.That(legacy.LastInvocation.Method, Is.EqualTo("PurchaseProduct"));
        }
    }
}

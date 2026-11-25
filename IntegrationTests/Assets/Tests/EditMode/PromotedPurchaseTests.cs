using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RevenueCat.Tests
{
    public class PromotedPurchaseTests
    {
        private GameObject _gameObject;
        private Purchases _purchases;
        private PurchasesWrapperSpy _wrapper;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("PromotedPurchaseTests");
            _purchases = _gameObject.AddComponent<Purchases>();
            _wrapper = new PurchasesWrapperSpy();
            _purchases.SetWrapper(_wrapper);
        }

        [TearDown]
        public void TearDown() { Object.DestroyImmediate(_gameObject); }

        [Test]
        public void LateListenerReplaysPendingPurchasesWithoutStartingThem()
        {
            Send("_receivePromotedPurchase", "{\"callbackID\":\"one\"}");
            Send("_receivePromotedPurchase", "{\"callbackID\":\"two\"}");
            var calls = 0;
            Purchases.ShouldPurchasePromoProductListener listener = _ => calls++;
            _purchases.AddShouldPurchasePromoProductListener(listener);
            Assert.That(calls, Is.EqualTo(2));
            Assert.That(_wrapper.Invocations, Is.Empty);
            Assert.That(_purchases.RemoveShouldPurchasePromoProductListener(listener), Is.True);
            Assert.That(_purchases.RemoveShouldPurchasePromoProductListener(listener), Is.False);
            Send("_receivePromotedPurchase", "{\"callbackID\":\"three\"}");
            Assert.That(calls, Is.EqualTo(2));
            _purchases.AddShouldPurchasePromoProductListener(listener);
            Assert.That(calls, Is.EqualTo(5));
        }

        [Test]
        public void PurchaseIsConsumedOnceAcrossListenersAndSurvivesListenerRemoval()
        {
            Action<Purchases.MakePurchaseFunc> first = null, second = null;
            Purchases.ShouldPurchasePromoProductListener listener = purchase => first = purchase;
            _purchases.AddShouldPurchasePromoProductListener(listener);
            _purchases.AddShouldPurchasePromoProductListener(purchase => second = purchase);
            Send("_receivePromotedPurchase", "{\"callbackID\":\"one\"}");
            _purchases.RemoveShouldPurchasePromoProductListener(listener);
            first(_ => { });
            Assert.That(_wrapper.LastInvocation.Method, Is.EqualTo("MakeDeferredPurchase"));
            Assert.That(_wrapper.LastInvocation.Arguments[0], Is.EqualTo("one"));
            Assert.That(_wrapper.LastInvocation.Arguments[1], Is.EqualTo(_gameObject.name));
            Assert.Throws<InvalidOperationException>(() => second(_ => { }));
            Assert.That(_wrapper.Invocations.Count, Is.EqualTo(1));
            _purchases.AddShouldPurchasePromoProductListener(_ => Assert.Fail("Consumed purchase replayed"));
        }

        [Test]
        public void DeferredResponsesAreCorrelatedWithoutOverwritingOrdinaryPurchaseCallbacks()
        {
            Action<Purchases.MakePurchaseFunc> startOne = null, startTwo = null;
            _purchases.AddShouldPurchasePromoProductListener(purchase =>
            {
                if (startOne == null) startOne = purchase;
                else startTwo = purchase;
            });
            Send("_receivePromotedPurchase", "{\"callbackID\":\"one\"}");
            Send("_receivePromotedPurchase", "{\"callbackID\":\"two\"}");
            Purchases.PurchaseResult ordinary = null, one = null, two = null;
            _purchases.PurchaseProduct("normal", result => ordinary = result);
            var completions = 0;
            startOne(result => { one = result; completions++; });
            startTwo(result => { two = result; completions++; });
            Send("_makeDeferredPurchase", "{\"callbackID\":\"two\",\"error\":{\"code\":2,\"message\":\"failed\"}}");
            Send("_makeDeferredPurchase", "{\"callbackID\":\"one\",\"userCancelled\":true,\"error\":{\"code\":1,\"message\":\"cancelled\"}}");
            Send("_makeDeferredPurchase", "{\"callbackID\":\"one\"}");
            Assert.That(completions, Is.EqualTo(2));
            Assert.That(one.UserCancelled, Is.True);
            Assert.That(one.Error.Code, Is.EqualTo(1));
            Assert.That(two.Error.Code, Is.EqualTo(2));
            Assert.That(ordinary, Is.Null);
            Send("_makePurchase", "{\"userCancelled\":true,\"error\":{\"code\":1,\"message\":\"cancelled\"}}");
            Assert.That(ordinary.UserCancelled, Is.True);
            _purchases.AddShouldPurchasePromoProductListener(_ => Assert.Fail("Failed purchase replayed"));
        }

        [Test]
        public void ListenerCanRemoveItselfWithoutSkippingTheNextListener()
        {
            var calls = 0;
            Purchases.ShouldPurchasePromoProductListener selfRemoving = null;
            selfRemoving = _ => _purchases.RemoveShouldPurchasePromoProductListener(selfRemoving);
            _purchases.AddShouldPurchasePromoProductListener(selfRemoving);
            _purchases.AddShouldPurchasePromoProductListener(_ => calls++);
            Send("_receivePromotedPurchase", "{\"callbackID\":\"one\"}");
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void ReentrantReplaySkipsPurchasesConsumedByAnotherListener()
        {
            Send("_receivePromotedPurchase", "{\"callbackID\":\"one\"}");
            Send("_receivePromotedPurchase", "{\"callbackID\":\"two\"}");
            var outerCalls = 0;
            _purchases.AddShouldPurchasePromoProductListener(_ =>
            {
                outerCalls++;
                _purchases.AddShouldPurchasePromoProductListener(purchase => purchase(result => { }));
            });
            Assert.That(outerCalls, Is.EqualTo(1));
            Assert.That(_wrapper.Invocations.Count, Is.EqualTo(2));
        }

        [Test]
        public void SavedFunctionRejectsAfterComponentIsDestroyed()
        {
            Action<Purchases.MakePurchaseFunc> start = null;
            _purchases.AddShouldPurchasePromoProductListener(purchase => start = purchase);
            Send("_receivePromotedPurchase", "{\"callbackID\":\"one\"}");
            Object.DestroyImmediate(_gameObject);
            Assert.Throws<InvalidOperationException>(() => start(_ => { }));
            Assert.That(_wrapper.Invocations, Is.Empty);
        }

        private void Send(string method, string json)
        {
            typeof(Purchases).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_purchases, new object[] { json });
        }
    }
}

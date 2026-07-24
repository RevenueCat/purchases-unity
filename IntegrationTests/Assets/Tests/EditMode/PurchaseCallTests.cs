using System.Reflection;
using NUnit.Framework;
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class PurchaseCallTests
    {
        private GameObject _gameObject;
        private Purchases _purchases;
        private PurchasesWrapperSpy _wrapper;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("RevenueCatTests");
            _purchases = _gameObject.AddComponent<Purchases>();
            _wrapper = new PurchasesWrapperSpy();
            _purchases.SetWrapper(_wrapper);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void PurchaseProductForwardsArgumentsAndDeliversResponse()
        {
            Purchases.PurchaseResult receivedResult = null;

            _purchases.PurchaseProduct(
                "monthly",
                result => receivedResult = result,
                "subs",
                "old_monthly",
                Purchases.ProrationMode.ImmediateAndChargeFullPrice,
                true
            );

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.PurchaseProduct), 7);
            Assert.That(invocation.Arguments[0], Is.EqualTo("monthly"));
            Assert.That(invocation.Arguments[1], Is.EqualTo("subs"));
            Assert.That(invocation.Arguments[2], Is.EqualTo("old_monthly"));
            Assert.That(invocation.Arguments[3], Is.EqualTo(Purchases.ProrationMode.ImmediateAndChargeFullPrice));
            Assert.That(invocation.Arguments[4], Is.True);
            Assert.That(invocation.Arguments[5], Is.Null);
            Assert.That(invocation.Arguments[6], Is.Null);

            SendNativeResponse("_makePurchase",
                "{\"transaction\":{\"transactionIdentifier\":\"transaction_id\"," +
                "\"productIdentifier\":\"monthly\",\"purchaseDateMillis\":1700000000000}," +
                "\"userCancelled\":false}");

            Assert.That(receivedResult, Is.Not.Null);
            Assert.That(receivedResult.ProductIdentifier, Is.EqualTo("monthly"));
            Assert.That(receivedResult.StoreTransaction.TransactionIdentifier, Is.EqualTo("transaction_id"));
            Assert.That(receivedResult.UserCancelled, Is.False);
            Assert.That(receivedResult.Error, Is.Null);
        }

        [Test]
        public void PurchasePackageForwardsArguments()
        {
            var package = CreatePackage();

            _purchases.PurchasePackage(
                package,
                _ => { },
                "old_monthly",
                Purchases.ProrationMode.ImmediateWithTimeProration,
                true
            );

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.PurchasePackage), 5);
            Assert.That(invocation.Arguments[0], Is.SameAs(package));
            Assert.That(invocation.Arguments[1], Is.EqualTo("old_monthly"));
            Assert.That(invocation.Arguments[2], Is.EqualTo(Purchases.ProrationMode.ImmediateWithTimeProration));
            Assert.That(invocation.Arguments[3], Is.True);
            Assert.That(invocation.Arguments[4], Is.Null);
        }

        [Test]
        public void PurchaseDiscountedProductForwardsDiscountAndDeliversResponse()
        {
            var discount = CreatePromotionalOffer();
            Purchases.PurchaseResult receivedResult = null;

            _purchases.PurchaseDiscountedProduct("monthly", discount, result => receivedResult = result);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.PurchaseProduct), 7);
            Assert.That(invocation.Arguments[0], Is.EqualTo("monthly"));
            Assert.That(invocation.Arguments[1], Is.EqualTo("subs"));
            Assert.That(invocation.Arguments[2], Is.Null);
            Assert.That(invocation.Arguments[3],
                Is.EqualTo(Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy));
            Assert.That(invocation.Arguments[4], Is.False);
            Assert.That(invocation.Arguments[5], Is.Null);
            Assert.That(invocation.Arguments[6], Is.SameAs(discount));

            SendNativeResponse("_makePurchase",
                "{\"transaction\":{\"transactionIdentifier\":\"transaction_id\"," +
                "\"productIdentifier\":\"monthly\",\"purchaseDateMillis\":1700000000000}," +
                "\"userCancelled\":false}");

            Assert.That(receivedResult, Is.Not.Null);
            Assert.That(receivedResult.ProductIdentifier, Is.EqualTo("monthly"));
        }

        [Test]
        public void PurchaseDiscountedPackageForwardsDiscountAndDeliversResponse()
        {
            var package = CreatePackage();
            var discount = CreatePromotionalOffer();
            Purchases.PurchaseResult receivedResult = null;

            _purchases.PurchaseDiscountedPackage(package, discount, result => receivedResult = result);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.PurchasePackage), 5);
            Assert.That(invocation.Arguments[0], Is.SameAs(package));
            Assert.That(invocation.Arguments[1], Is.Null);
            Assert.That(invocation.Arguments[2],
                Is.EqualTo(Purchases.ProrationMode.UnknownSubscriptionUpgradeDowngradePolicy));
            Assert.That(invocation.Arguments[3], Is.False);
            Assert.That(invocation.Arguments[4], Is.SameAs(discount));

            SendNativeResponse("_makePurchase",
                "{\"transaction\":{\"transactionIdentifier\":\"transaction_id\"," +
                "\"productIdentifier\":\"$rc_monthly\",\"purchaseDateMillis\":1700000000000}," +
                "\"userCancelled\":false}");

            Assert.That(receivedResult, Is.Not.Null);
        }

        [Test]
        public void PurchaseSubscriptionOptionForwardsArguments()
        {
            var subscriptionOption = CreateSubscriptionOption();
            var googleProductChangeInfo =
                new Purchases.GoogleProductChangeInfo("old_monthly", Purchases.ProrationMode.ImmediateWithTimeProration);

            _purchases.PurchaseSubscriptionOption(subscriptionOption, _ => { }, googleProductChangeInfo, true);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.PurchaseSubscriptionOption), 3);
            Assert.That(invocation.Arguments[0], Is.SameAs(subscriptionOption));
            Assert.That(invocation.Arguments[1], Is.SameAs(googleProductChangeInfo));
            Assert.That(invocation.Arguments[2], Is.True);
        }

        [Test]
        public void PurchaseProductWithWinBackOfferForwardsArgumentsAndDeliversResponse()
        {
            var storeProduct = CreateStoreProduct();
            var winBackOffer = CreateWinBackOffer();
            Purchases.PurchaseResult receivedResult = null;

            _purchases.PurchaseProductWithWinBackOffer(storeProduct, winBackOffer, result => receivedResult = result);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.PurchaseProductWithWinBackOffer), 2);
            Assert.That(invocation.Arguments[0], Is.SameAs(storeProduct));
            Assert.That(invocation.Arguments[1], Is.SameAs(winBackOffer));

            SendNativeResponse("_purchaseProductWithWinBackOffer",
                "{\"transaction\":{\"transactionIdentifier\":\"transaction_id\"," +
                "\"productIdentifier\":\"monthly\",\"purchaseDateMillis\":1700000000000}," +
                "\"userCancelled\":false}");

            Assert.That(receivedResult, Is.Not.Null);
            Assert.That(receivedResult.ProductIdentifier, Is.EqualTo("monthly"));
        }

        [Test]
        public void PurchasePackageWithWinBackOfferForwardsArgumentsAndDeliversResponse()
        {
            var package = CreatePackage();
            var winBackOffer = CreateWinBackOffer();
            Purchases.PurchaseResult receivedResult = null;

            _purchases.PurchasePackageWithWinBackOffer(package, winBackOffer, result => receivedResult = result);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.PurchasePackageWithWinBackOffer), 2);
            Assert.That(invocation.Arguments[0], Is.SameAs(package));
            Assert.That(invocation.Arguments[1], Is.SameAs(winBackOffer));

            SendNativeResponse("_purchasePackageWithWinBackOffer",
                "{\"transaction\":{\"transactionIdentifier\":\"transaction_id\"," +
                "\"productIdentifier\":\"$rc_monthly\",\"purchaseDateMillis\":1700000000000}," +
                "\"userCancelled\":false}");

            Assert.That(receivedResult, Is.Not.Null);
        }

        [Test]
        public void PurchaseResultParsesUserCancellationWithoutTransactionOrError()
        {
            Purchases.PurchaseResult receivedResult = null;
            _purchases.PurchaseProduct("monthly", result => receivedResult = result);

            SendNativeResponse("_makePurchase", "{\"userCancelled\":true}");

            Assert.That(receivedResult, Is.Not.Null);
            Assert.That(receivedResult.UserCancelled, Is.True);
            Assert.That(receivedResult.CustomerInfo, Is.Null);
            Assert.That(receivedResult.StoreTransaction, Is.Null);
            Assert.That(receivedResult.ProductIdentifier, Is.Null);
            Assert.That(receivedResult.Error, Is.Null);
        }

        [Test]
        public void PurchaseResultParsesErrorOnlyResponse()
        {
            Purchases.PurchaseResult receivedResult = null;
            _purchases.PurchaseProduct("monthly", result => receivedResult = result);

            SendNativeResponse("_makePurchase",
                "{\"error\":{\"message\":\"Purchase failed\",\"code\":7," +
                "\"underlyingErrorMessage\":\"Store declined\",\"readableErrorCode\":\"STORE_PROBLEM_ERROR\"}}");

            Assert.That(receivedResult, Is.Not.Null);
            Assert.That(receivedResult.Error, Is.Not.Null);
            Assert.That(receivedResult.Error.Code, Is.EqualTo(7));
            Assert.That(receivedResult.UserCancelled, Is.False);
            Assert.That(receivedResult.CustomerInfo, Is.Null);
            Assert.That(receivedResult.StoreTransaction, Is.Null);
        }

        private PurchasesWrapperSpy.Invocation AssertLastInvocation(string method, int argumentCount)
        {
            Assert.That(_wrapper.Invocations, Has.Count.EqualTo(1));
            Assert.That(_wrapper.LastInvocation.Method, Is.EqualTo(method));
            Assert.That(_wrapper.LastInvocation.Arguments, Has.Length.EqualTo(argumentCount));
            return _wrapper.LastInvocation;
        }

        private void SendNativeResponse(string method, string response)
        {
            var receiver = typeof(Purchases).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(receiver, Is.Not.Null, $"Native response receiver {method} does not exist");
            receiver.Invoke(_purchases, new object[] { response });
        }

        private static Purchases.Package CreatePackage()
        {
            return new Purchases.Package(JSONNode.Parse(
                "{\"identifier\":\"$rc_monthly\",\"packageType\":\"MONTHLY\"," +
                "\"product\":{\"title\":\"Monthly\",\"identifier\":\"monthly\"," +
                "\"description\":\"Monthly access\",\"price\":9.99,\"priceString\":\"$9.99\"," +
                "\"currencyCode\":\"USD\",\"productCategory\":\"SUBSCRIPTION\"}," +
                "\"presentedOfferingContext\":{\"offeringIdentifier\":\"default\"}}"
            ));
        }

        private static Purchases.StoreProduct CreateStoreProduct()
        {
            return new Purchases.StoreProduct(JSONNode.Parse(
                "{\"title\":\"Monthly\",\"identifier\":\"monthly\"," +
                "\"description\":\"Monthly access\",\"price\":9.99,\"priceString\":\"$9.99\"," +
                "\"currencyCode\":\"USD\",\"productCategory\":\"SUBSCRIPTION\"}"
            ));
        }

        private static Purchases.SubscriptionOption CreateSubscriptionOption()
        {
            return new Purchases.SubscriptionOption(JSONNode.Parse(
                "{\"id\":\"monthly:base\",\"storeProductId\":\"monthly\",\"productId\":\"monthly\"," +
                "\"tags\":[],\"isBasePlan\":true," +
                "\"billingPeriod\":{\"unit\":\"MONTH\",\"value\":1,\"iso8601\":\"P1M\"},\"isPrepaid\":false}"
            ));
        }

        private static Purchases.PromotionalOffer CreatePromotionalOffer()
        {
            return new Purchases.PromotionalOffer(JSONNode.Parse(
                "{\"identifier\":\"promo\",\"keyIdentifier\":\"key\",\"nonce\":\"nonce\"," +
                "\"signature\":\"sig\",\"timestamp\":1700000000000}"
            ));
        }

        private static Purchases.WinBackOffer CreateWinBackOffer()
        {
            return new Purchases.WinBackOffer(JSONNode.Parse(
                "{\"identifier\":\"winback\",\"price\":4.99,\"priceString\":\"$4.99\",\"cycles\":1," +
                "\"period\":\"P1M\",\"periodUnit\":\"MONTH\",\"periodNumberOfUnits\":1}"
            ));
        }
    }
}

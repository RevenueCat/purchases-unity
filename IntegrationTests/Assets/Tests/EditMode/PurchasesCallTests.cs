using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class PurchasesCallTests
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
        public void ConfigureForwardsEveryConfigurationValue()
        {
            var configuration = Purchases.PurchasesConfiguration.Builder
                .Init("test_api_key")
                .SetAppUserId("app_user_id")
                .SetPurchasesAreCompletedBy(Purchases.PurchasesAreCompletedBy.MyApp,
                    Purchases.StoreKitVersion.StoreKit2)
                .SetUserDefaultsSuiteName("suite_name")
                .SetUseAmazon(true)
                .SetDangerousSettings(new Purchases.DangerousSettings(false))
                .SetShouldShowInAppMessagesAutomatically(true)
                .SetEntitlementVerificationMode(Purchases.EntitlementVerificationMode.Informational)
                .SetPendingTransactionsForPrepaidPlansEnabled(true)
                .SetDiagnosticsEnabled(true)
                .SetAutomaticDeviceIdentifierCollectionEnabled(false)
                .SetPreferredUILocaleOverride("de_DE")
                .Build();

            _purchases.Configure(configuration);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.Setup), 14);
            Assert.That(invocation.Arguments[0], Is.EqualTo(_gameObject.name));
            Assert.That(invocation.Arguments[1], Is.EqualTo("test_api_key"));
            Assert.That(invocation.Arguments[2], Is.EqualTo("app_user_id"));
            Assert.That(invocation.Arguments[3], Is.EqualTo(Purchases.PurchasesAreCompletedBy.MyApp));
            Assert.That(invocation.Arguments[4], Is.EqualTo(Purchases.StoreKitVersion.StoreKit2));
            Assert.That(invocation.Arguments[5], Is.EqualTo("suite_name"));
            Assert.That(invocation.Arguments[6], Is.True);
            Assert.That(JSONNode.Parse((string)invocation.Arguments[7])["AutoSyncPurchases"].AsBool, Is.False);
            Assert.That(invocation.Arguments[8], Is.True);
            Assert.That(invocation.Arguments[9], Is.EqualTo(Purchases.EntitlementVerificationMode.Informational));
            Assert.That(invocation.Arguments[10], Is.True);
            Assert.That(invocation.Arguments[11], Is.True);
            Assert.That(invocation.Arguments[12], Is.False);
            Assert.That(invocation.Arguments[13], Is.EqualTo("de_DE"));
        }

        [Test]
        public void GetProductsForwardsArgumentsAndDeliversResponseOnce()
        {
            var identifiers = new[] { "monthly", "annual" };
            List<Purchases.StoreProduct> receivedProducts = null;
            Purchases.Error receivedError = null;
            var callbackCount = 0;

            _purchases.GetProducts(identifiers, (products, error) =>
            {
                callbackCount++;
                receivedProducts = products;
                receivedError = error;
            }, "inapp");

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.GetProducts), 2);
            Assert.That(invocation.Arguments[0], Is.SameAs(identifiers));
            Assert.That(invocation.Arguments[1], Is.EqualTo("inapp"));

            const string response =
                "{\"products\":[{\"title\":\"Lifetime\",\"identifier\":\"lifetime\"," +
                "\"description\":\"Lifetime access\",\"price\":99.99,\"priceString\":\"$99.99\"," +
                "\"currencyCode\":\"USD\",\"productCategory\":\"NON_SUBSCRIPTION\"}]}";
            SendNativeResponse("_receiveProducts", response);
            SendNativeResponse("_receiveProducts", response);

            Assert.That(callbackCount, Is.EqualTo(1));
            Assert.That(receivedError, Is.Null);
            Assert.That(receivedProducts, Has.Count.EqualTo(1));
            Assert.That(receivedProducts[0].Identifier, Is.EqualTo("lifetime"));
        }

        [Test]
        public void GetProductsDeliversNativeError()
        {
            List<Purchases.StoreProduct> receivedProducts = null;
            Purchases.Error receivedError = null;

            _purchases.GetProducts(new[] { "missing" }, (products, error) =>
            {
                receivedProducts = products;
                receivedError = error;
            });

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.GetProducts), 2);
            Assert.That(invocation.Arguments[1], Is.EqualTo("subs"));

            SendNativeResponse("_receiveProducts",
                "{\"error\":{\"message\":\"Product missing\",\"code\":5," +
                "\"underlyingErrorMessage\":\"Store returned no product\"," +
                "\"readableErrorCode\":\"PRODUCT_NOT_AVAILABLE_FOR_PURCHASE_ERROR\"}}");

            Assert.That(receivedProducts, Is.Null);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(5));
            Assert.That(receivedError.Message, Is.EqualTo("Product missing"));
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
        public void SetAttributesSerializesStringsAndNulls()
        {
            _purchases.SetAttributes(new Dictionary<string, string>
            {
                ["plan"] = "premium",
                ["nickname"] = null
            });

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.SetAttributes), 1);
            var attributes = JSONNode.Parse((string)invocation.Arguments[0]);
            Assert.That(attributes["plan"].Value, Is.EqualTo("premium"));
            Assert.That(attributes["nickname"].IsNull, Is.True);
        }

        [Test]
        public void GetOfferingsCallsWrapperAndDeliversResponseOnce()
        {
            Purchases.Offerings receivedOfferings = null;
            Purchases.Error receivedError = null;
            var callbackCount = 0;

            _purchases.GetOfferings((offerings, error) =>
            {
                callbackCount++;
                receivedOfferings = offerings;
                receivedError = error;
            });

            AssertLastInvocation(nameof(IPurchasesWrapper.GetOfferings), 0);

            const string response = "{\"offerings\":{\"all\":{},\"current\":null}}";
            SendNativeResponse("_getOfferings", response);
            SendNativeResponse("_getOfferings", response);

            Assert.That(callbackCount, Is.EqualTo(1));
            Assert.That(receivedError, Is.Null);
            Assert.That(receivedOfferings, Is.Not.Null);
            Assert.That(receivedOfferings.All, Is.Empty);
            Assert.That(receivedOfferings.Current, Is.Null);
        }

        [Test]
        public void CanMakePaymentsNormalizesNullFeaturesAndDeliversError()
        {
            var receivedCanMakePayments = true;
            Purchases.Error receivedError = null;

            _purchases.CanMakePayments(null, (canMakePayments, error) =>
            {
                receivedCanMakePayments = canMakePayments;
                receivedError = error;
            });

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.CanMakePayments), 1);
            Assert.That((Purchases.BillingFeature[])invocation.Arguments[0], Is.Empty);

            SendNativeResponse("_canMakePayments",
                "{\"error\":{\"message\":\"Billing unavailable\",\"code\":2," +
                "\"underlyingErrorMessage\":\"No store\",\"readableErrorCode\":\"STORE_PROBLEM_ERROR\"}}");

            Assert.That(receivedCanMakePayments, Is.False);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(2));
        }

        [Test]
        public void TrackCustomPaywallImpressionWithoutParametersCreatesEmptyParameters()
        {
            _purchases.TrackCustomPaywallImpression();

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.TrackCustomPaywallImpression), 1);
            var parameters = (Purchases.CustomPaywallImpressionParams)invocation.Arguments[0];
            Assert.That(parameters.PaywallId, Is.Null);
            Assert.That(parameters.OfferingId, Is.Null);
            Assert.That(parameters.Offering, Is.Null);
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
    }
}

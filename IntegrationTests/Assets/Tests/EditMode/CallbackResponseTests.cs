using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RevenueCat.SimpleJSON;
using UnityEngine;
using UnityEngine.TestTools;

namespace RevenueCat.Tests
{
    public class CallbackResponseTests
    {
        private const string MinimalCustomerInfoJson =
            "{\"entitlements\":{\"all\":{},\"active\":{}}," +
            "\"activeSubscriptions\":[],\"allPurchasedProductIdentifiers\":[]," +
            "\"firstSeenMillis\":1700000000000,\"originalAppUserId\":\"user_1\"," +
            "\"requestDateMillis\":1700000000001,\"allExpirationDatesMillis\":{}," +
            "\"allPurchaseDatesMillis\":{},\"nonSubscriptionTransactions\":[]," +
            "\"subscriptionsByProductIdentifier\":{}}";

        private const string ErrorJson =
            "{\"error\":{\"message\":\"Something went wrong\",\"code\":3," +
            "\"underlyingErrorMessage\":\"Backend error\",\"readableErrorCode\":\"UNKNOWN_ERROR\"}}";

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
        public void GetCustomerInfoDeliversResponse()
        {
            Purchases.CustomerInfo receivedInfo = null;
            Purchases.Error receivedError = null;

            _purchases.GetCustomerInfo((info, error) =>
            {
                receivedInfo = info;
                receivedError = error;
            });

            AssertLastInvocation(nameof(IPurchasesWrapper.GetCustomerInfo), 0);

            SendNativeResponse("_getCustomerInfo", "{\"customerInfo\":" + MinimalCustomerInfoJson + "}");

            Assert.That(receivedError, Is.Null);
            Assert.That(receivedInfo, Is.Not.Null);
            Assert.That(receivedInfo.OriginalAppUserId, Is.EqualTo("user_1"));
        }

        [Test]
        public void GetCustomerInfoDeliversNativeError()
        {
            Purchases.CustomerInfo receivedInfo = null;
            Purchases.Error receivedError = null;

            _purchases.GetCustomerInfo((info, error) =>
            {
                receivedInfo = info;
                receivedError = error;
            });

            SendNativeResponse("_getCustomerInfo", ErrorJson);

            Assert.That(receivedInfo, Is.Null);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(3));
        }

        [Test]
        public void LogInForwardsAppUserIdAndDeliversCreatedFlag()
        {
            Purchases.CustomerInfo receivedInfo = null;
            var receivedCreated = false;
            Purchases.Error receivedError = null;

            _purchases.LogIn("new_user", (info, created, error) =>
            {
                receivedInfo = info;
                receivedCreated = created;
                receivedError = error;
            });

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.LogIn), 1);
            Assert.That(invocation.Arguments[0], Is.EqualTo("new_user"));

            SendNativeResponse("_logIn", "{\"customerInfo\":" + MinimalCustomerInfoJson + ",\"created\":true}");

            Assert.That(receivedError, Is.Null);
            Assert.That(receivedCreated, Is.True);
            Assert.That(receivedInfo, Is.Not.Null);
        }

        [Test]
        public void LogInDeliversNativeError()
        {
            Purchases.CustomerInfo receivedInfo = null;
            var receivedCreated = true;
            Purchases.Error receivedError = null;

            _purchases.LogIn("new_user", (info, created, error) =>
            {
                receivedInfo = info;
                receivedCreated = created;
                receivedError = error;
            });

            SendNativeResponse("_logIn", ErrorJson);

            Assert.That(receivedInfo, Is.Null);
            Assert.That(receivedCreated, Is.False);
            Assert.That(receivedError, Is.Not.Null);
        }

        [Test]
        public void LogOutDeliversCustomerInfo()
        {
            Purchases.CustomerInfo receivedInfo = null;

            _purchases.LogOut((info, error) => receivedInfo = info);

            AssertLastInvocation(nameof(IPurchasesWrapper.LogOut), 0);

            SendNativeResponse("_logOut", "{\"customerInfo\":" + MinimalCustomerInfoJson + "}");

            Assert.That(receivedInfo, Is.Not.Null);
        }

        [Test]
        public void RestorePurchasesDeliversCustomerInfo()
        {
            Purchases.CustomerInfo receivedInfo = null;

            _purchases.RestorePurchases((info, error) => receivedInfo = info);

            AssertLastInvocation(nameof(IPurchasesWrapper.RestorePurchases), 0);

            SendNativeResponse("_restorePurchases", "{\"customerInfo\":" + MinimalCustomerInfoJson + "}");

            Assert.That(receivedInfo, Is.Not.Null);
        }

        [Test]
        public void SyncPurchasesWithCallbackDeliversCustomerInfo()
        {
            Purchases.CustomerInfo receivedInfo = null;

            _purchases.SyncPurchases((info, error) => receivedInfo = info);

            AssertLastInvocation(nameof(IPurchasesWrapper.SyncPurchases), 0);

            SendNativeResponse("_syncPurchases", "{\"customerInfo\":" + MinimalCustomerInfoJson + "}");

            Assert.That(receivedInfo, Is.Not.Null);
        }

        [Test]
        public void SyncAttributesAndOfferingsIfNeededDeliversOfferings()
        {
            Purchases.Offerings receivedOfferings = null;

            _purchases.SyncAttributesAndOfferingsIfNeeded((offerings, error) => receivedOfferings = offerings);

            AssertLastInvocation(nameof(IPurchasesWrapper.SyncAttributesAndOfferingsIfNeeded), 0);

            SendNativeResponse("_syncAttributesAndOfferingsIfNeeded", "{\"offerings\":{\"all\":{},\"current\":null}}");

            Assert.That(receivedOfferings, Is.Not.Null);
            Assert.That(receivedOfferings.All, Is.Empty);
        }

        [Test]
        public void CheckTrialOrIntroductoryPriceEligibilityDeliversDictionary()
        {
            // IPurchasesWrapper.CheckTrialOrIntroductoryPriceEligibility takes a single array parameter, so the
            // spy's `params object[]` forwarding aliases the array itself as Invocations.Arguments rather than
            // wrapping it as a single element.
            var identifiers = new[] { "monthly", "annual" };
            Dictionary<string, Purchases.IntroEligibility> receivedEligibility = null;

            _purchases.CheckTrialOrIntroductoryPriceEligibility(identifiers, eligibility => receivedEligibility = eligibility);

            Assert.That(_wrapper.Invocations, Has.Count.EqualTo(1));
            Assert.That(_wrapper.LastInvocation.Method,
                Is.EqualTo(nameof(IPurchasesWrapper.CheckTrialOrIntroductoryPriceEligibility)));
            Assert.That(_wrapper.LastInvocation.Arguments, Is.SameAs(identifiers));

            SendNativeResponse("_checkTrialOrIntroductoryPriceEligibility",
                "{\"monthly\":{\"status\":1,\"description\":\"eligible\"}}");

            Assert.That(receivedEligibility, Is.Not.Null);
            Assert.That((int)receivedEligibility["monthly"].Status, Is.EqualTo(1));
            Assert.That(receivedEligibility["monthly"].Description, Is.EqualTo("eligible"));
        }

        [Test]
        public void GetStorefrontDeliversPopulatedStorefront()
        {
            Purchases.Storefront receivedStorefront = null;

            _purchases.GetStorefront(storefront => receivedStorefront = storefront);

            AssertLastInvocation(nameof(IPurchasesWrapper.GetStorefront), 0);

            SendNativeResponse("_receiveStorefront", "{\"countryCode\":\"US\"}");

            Assert.That(receivedStorefront, Is.Not.Null);
            Assert.That(receivedStorefront.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public void GetStorefrontReturnsNullForEmptyObject()
        {
            Purchases.Storefront receivedStorefront = new Purchases.Storefront("non-null-sentinel");

            _purchases.GetStorefront(storefront => receivedStorefront = storefront);

            SendNativeResponse("_receiveStorefront", "{}");

            Assert.That(receivedStorefront, Is.Null);
        }

        [Test]
        public void GetStorefrontReturnsNullWhenCountryCodeMissing()
        {
            Purchases.Storefront receivedStorefront = new Purchases.Storefront("non-null-sentinel");

            _purchases.GetStorefront(storefront => receivedStorefront = storefront);

            LogAssert.Expect(LogType.Error, "StorefrontCallback received null countryCode");
            SendNativeResponse("_receiveStorefront", "{\"foo\":\"bar\"}");

            Assert.That(receivedStorefront, Is.Null);
        }

        [Test]
        public void GetPromotionalOfferDeliversOffer()
        {
            var storeProduct = CreateStoreProduct();
            var discount = CreateDiscount();
            Purchases.PromotionalOffer receivedOffer = null;

            _purchases.GetPromotionalOffer(storeProduct, discount, (offer, error) => receivedOffer = offer);

            var invocation = AssertLastInvocation(nameof(IPurchasesWrapper.GetPromotionalOffer), 2);
            Assert.That(invocation.Arguments[0], Is.EqualTo(storeProduct.Identifier));
            Assert.That(invocation.Arguments[1], Is.EqualTo(discount.Identifier));

            SendNativeResponse("_getPromotionalOffer",
                "{\"identifier\":\"promo\",\"keyIdentifier\":\"key\",\"nonce\":\"nonce\"," +
                "\"signature\":\"sig\",\"timestamp\":1700000000000}");

            Assert.That(receivedOffer, Is.Not.Null);
            Assert.That(receivedOffer.Identifier, Is.EqualTo("promo"));
        }

        [Test]
        public void GetPromotionalOfferDeliversNativeError()
        {
            var storeProduct = CreateStoreProduct();
            var discount = CreateDiscount();
            Purchases.PromotionalOffer receivedOffer = null;
            Purchases.Error receivedError = null;

            _purchases.GetPromotionalOffer(storeProduct, discount, (offer, error) =>
            {
                receivedOffer = offer;
                receivedError = error;
            });

            SendNativeResponse("_getPromotionalOffer", ErrorJson);

            Assert.That(receivedOffer, Is.Null);
            Assert.That(receivedError, Is.Not.Null);
        }

        [Test]
        public void GetCurrentOfferingForPlacementDeliversNullWhenNoOfferingKey()
        {
            Purchases.Offering receivedOffering = CreateOffering();
            Purchases.Error receivedError = new Purchases.Error(JSONNode.Parse(
                "{\"message\":\"m\",\"code\":1,\"underlyingErrorMessage\":\"u\",\"readableErrorCode\":\"r\"}"));

            _purchases.GetCurrentOfferingForPlacement("onboarding", (offering, error) =>
            {
                receivedOffering = offering;
                receivedError = error;
            });

            AssertLastInvocation(nameof(IPurchasesWrapper.GetCurrentOfferingForPlacement), 1);

            SendNativeResponse("_getCurrentOfferingForPlacement", "{}");

            Assert.That(receivedOffering, Is.Null);
            Assert.That(receivedError, Is.Null);
        }

        [Test]
        public void GetCurrentOfferingForPlacementDeliversOffering()
        {
            Purchases.Offering receivedOffering = null;

            _purchases.GetCurrentOfferingForPlacement("onboarding", (offering, error) => receivedOffering = offering);

            SendNativeResponse("_getCurrentOfferingForPlacement",
                "{\"offering\":{\"identifier\":\"default\",\"serverDescription\":\"desc\",\"availablePackages\":[]}}");

            Assert.That(receivedOffering, Is.Not.Null);
            Assert.That(receivedOffering.Identifier, Is.EqualTo("default"));
        }

        [Test]
        public void GetCurrentOfferingForPlacementDeliversErrorWhenOfferingKeyPresent()
        {
            Purchases.Offering receivedOffering = CreateOffering();
            Purchases.Error receivedError = null;

            _purchases.GetCurrentOfferingForPlacement("onboarding", (offering, error) =>
            {
                receivedOffering = offering;
                receivedError = error;
            });

            SendNativeResponse("_getCurrentOfferingForPlacement",
                "{\"offering\":{},\"error\":{\"message\":\"No offering\",\"code\":9," +
                "\"underlyingErrorMessage\":\"none\",\"readableErrorCode\":\"NOT_FOUND\"}}");

            Assert.That(receivedOffering, Is.Null);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(9));
        }

        [Test]
        public void GetAmazonLWAConsentStatusDeliversConsent()
        {
            var receivedConsent = false;
            Purchases.Error receivedError = null;

            _purchases.GetAmazonLWAConsentStatus((hasConsented, error) =>
            {
                receivedConsent = hasConsented;
                receivedError = error;
            });

            AssertLastInvocation(nameof(IPurchasesWrapper.GetAmazonLWAConsentStatus), 0);

            SendNativeResponse("_getAmazonLWAConsentStatus", "{\"amazonLWAConsentStatus\":true}");

            Assert.That(receivedConsent, Is.True);
            Assert.That(receivedError, Is.Null);
        }

        [Test]
        public void GetAmazonLWAConsentStatusDeliversNativeError()
        {
            var receivedConsent = true;
            Purchases.Error receivedError = null;

            _purchases.GetAmazonLWAConsentStatus((hasConsented, error) =>
            {
                receivedConsent = hasConsented;
                receivedError = error;
            });

            SendNativeResponse("_getAmazonLWAConsentStatus", ErrorJson);

            Assert.That(receivedConsent, Is.False);
            Assert.That(receivedError, Is.Not.Null);
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

        private static Purchases.StoreProduct CreateStoreProduct()
        {
            return new Purchases.StoreProduct(JSONNode.Parse(
                "{\"title\":\"Monthly\",\"identifier\":\"monthly\"," +
                "\"description\":\"Monthly access\",\"price\":9.99,\"priceString\":\"$9.99\"," +
                "\"currencyCode\":\"USD\",\"productCategory\":\"SUBSCRIPTION\"}"
            ));
        }

        private static Purchases.Discount CreateDiscount()
        {
            return new Purchases.Discount(JSONNode.Parse(
                "{\"identifier\":\"intro\",\"price\":4.99,\"priceString\":\"$4.99\",\"cycles\":1," +
                "\"period\":\"P1M\",\"periodUnit\":\"MONTH\",\"periodNumberOfUnits\":1}"
            ));
        }

        private static Purchases.Offering CreateOffering()
        {
            return new Purchases.Offering(JSONNode.Parse(
                "{\"identifier\":\"default\",\"serverDescription\":\"desc\",\"availablePackages\":[]}"
            ));
        }
    }
}

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RevenueCat.SimpleJSON;
using UnityEngine;
using UnityEngine.TestTools;

namespace RevenueCat.Tests
{
    /// <summary>
    /// Exercises the <c>_*</c> receivers that the native wrappers call through
    /// <c>UnitySendMessage</c>. Every JSON payload here mirrors what
    /// purchases-hybrid-common hands to <c>PurchasesUnityHelper.m</c> /
    /// <c>PurchasesWrapper.java</c>, which forward it verbatim.
    ///
    /// Two properties of that wire format are easy to get wrong:
    ///   * Optional fields arrive as explicit nulls, not missing keys. Android's
    ///     <c>Map.convertToJson()</c> maps Kotlin <c>null</c> to <c>JSONObject.NULL</c> and the
    ///     iOS mappers use <c>NSNull()</c>. SimpleJSON treats a null node and an absent node
    ///     alike, but the fixtures spell the nulls out so they match what ships.
    ///   * The <c>error</c> object comes from <c>PurchasesError.map()</c> on Android and
    ///     <c>ErrorContainer.init</c> on iOS. Both always send <c>code</c>, <c>message</c> and
    ///     <c>underlyingErrorMessage</c>, and add <c>readableErrorCode</c> plus the deprecated
    ///     <c>readable_error_code</c> whenever the underlying error carries one.
    ///     That value is the enum name on Android (<c>StoreProblemError</c>) and the
    ///     SCREAMING_SNAKE <c>ErrorCode.codeName</c> on iOS (<c>STORE_PROBLEM</c>), so the
    ///     fixtures below pick whichever platform can actually produce the callback.
    /// </summary>
    public class CallbackResponseTests
    {
        #region Payload fixtures

        /// CustomerInfo.map() / CustomerInfo.dictionary for a user with no purchases.
        /// The ISO strings use Android's Iso8601Utils format; iOS drops the milliseconds.
        /// Unity only reads the *Millis variants.
        private const string CustomerInfoJson =
            "{\"entitlements\":{\"all\":{},\"active\":{},\"verification\":\"NOT_REQUESTED\"}," +
            "\"activeSubscriptions\":[],\"allPurchasedProductIdentifiers\":[]," +
            "\"latestExpirationDate\":null,\"latestExpirationDateMillis\":null," +
            "\"firstSeen\":\"2023-11-14T22:13:20.000Z\",\"firstSeenMillis\":1700000000000," +
            "\"originalAppUserId\":\"user_1\"," +
            "\"requestDate\":\"2023-11-14T22:13:20.001Z\",\"requestDateMillis\":1700000000001," +
            "\"allExpirationDates\":{},\"allExpirationDatesMillis\":{}," +
            "\"allPurchaseDates\":{},\"allPurchaseDatesMillis\":{}," +
            "\"originalApplicationVersion\":null," +
            "\"originalPurchaseDate\":null,\"originalPurchaseDateMillis\":null," +
            "\"managementURL\":null,\"nonSubscriptionTransactions\":[]," +
            "\"subscriptionsByProductIdentifier\":{}}";

        /// StoreProduct.map() for a Google Play in-app product. Android emits every key with an
        /// explicit null; iOS omits presentedOfferingIdentifier/presentedOfferingContext entirely.
        private const string NonSubscriptionProductJson =
            "{\"identifier\":\"lifetime\",\"description\":\"Lifetime access\",\"title\":\"Lifetime\"," +
            "\"price\":99.99,\"priceString\":\"$99.99\",\"currencyCode\":\"USD\"," +
            "\"introPrice\":null,\"discounts\":null," +
            "\"pricePerWeek\":null,\"pricePerMonth\":null,\"pricePerYear\":null," +
            "\"pricePerWeekString\":null,\"pricePerMonthString\":null,\"pricePerYearString\":null," +
            "\"productCategory\":\"NON_SUBSCRIPTION\",\"productType\":\"CONSUMABLE\"," +
            "\"subscriptionPeriod\":null,\"defaultOption\":null,\"subscriptionOptions\":null," +
            "\"presentedOfferingIdentifier\":null,\"presentedOfferingContext\":null}";

        /// Offering.map() / Offering.dictionary for an offering with no packages.
        private const string OfferingWithoutPackagesJson =
            "{\"identifier\":\"default\",\"serverDescription\":\"desc\",\"metadata\":{}," +
            "\"availablePackages\":[],\"lifetime\":null,\"annual\":null,\"sixMonth\":null," +
            "\"threeMonth\":null,\"twoMonth\":null,\"monthly\":null,\"weekly\":null," +
            "\"webCheckoutUrl\":null}";

        /// Offerings.map() with no offerings configured. iOS omits "current" instead of
        /// sending null; SimpleJSON handles both the same way.
        private const string EmptyOfferingsJson = "{\"offerings\":{\"all\":{},\"current\":null}}";

        /// The "error" envelope both platforms send. Values differ per platform — Android uses
        /// PurchasesErrorCode.name and .description, iOS uses ErrorCode.codeName and the
        /// NSError's localizedDescription — so each call site picks values the platform that
        /// owns that callback can actually produce.
        private static string ErrorJson(int code, string message, string readableErrorCode,
            string underlyingErrorMessage = "")
        {
            return "{\"error\":{\"code\":" + code +
                   ",\"message\":\"" + message + "\"" +
                   ",\"underlyingErrorMessage\":\"" + underlyingErrorMessage + "\"" +
                   ",\"readableErrorCode\":\"" + readableErrorCode + "\"" +
                   ",\"readable_error_code\":\"" + readableErrorCode + "\"}}";
        }

        #endregion

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

            const string response = "{\"products\":[" + NonSubscriptionProductJson + "]}";
            SendNativeResponse("_receiveProducts", response);
            SendNativeResponse("_receiveProducts", response);

            Assert.That(callbackCount, Is.EqualTo(1));
            Assert.That(receivedError, Is.Null);
            Assert.That(receivedProducts, Has.Count.EqualTo(1));
            var product = receivedProducts[0];
            Assert.That(product.Identifier, Is.EqualTo("lifetime"));
            Assert.That(product.ProductCategory, Is.EqualTo(Purchases.ProductCategory.NON_SUBSCRIPTION));
            // The nulls the mappers always send must not leak through as values.
            Assert.That(product.IntroductoryPrice, Is.Null);
            Assert.That(product.SubscriptionPeriod, Is.Null);
            Assert.That(product.PresentedOfferingContext, Is.Null);
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

            // Only PurchasesWrapper.java has an error path here: iOS getProductInfo has no
            // error callback and always reports an empty product list instead.
            SendNativeResponse("_receiveProducts", ErrorJson(2, "There was a problem with the store.",
                "StoreProblemError", "Error connecting to the store."));

            Assert.That(receivedProducts, Is.Null);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(2));
            Assert.That(receivedError.Message, Is.EqualTo("There was a problem with the store."));
            Assert.That(receivedError.ReadableErrorCode, Is.EqualTo("StoreProblemError"));
            Assert.That(receivedError.UnderlyingErrorMessage, Is.EqualTo("Error connecting to the store."));
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

            SendNativeResponse("_getOfferings", EmptyOfferingsJson);
            SendNativeResponse("_getOfferings", EmptyOfferingsJson);

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

            // This is the only error common.kt can report for canMakePayments, and only on
            // Android: iOS returns a plain BOOL and never sends an error.
            SendNativeResponse("_canMakePayments", ErrorJson(0,
                "Unknown error. Check the underlying error for more details.",
                "UnknownError", "Invalid feature type passed to canMakePayments."));

            Assert.That(receivedCanMakePayments, Is.False);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(0));
            Assert.That(receivedError.ReadableErrorCode, Is.EqualTo("UnknownError"));
            Assert.That(receivedError.UnderlyingErrorMessage,
                Is.EqualTo("Invalid feature type passed to canMakePayments."));
        }

        [Test]
        public void CanMakePaymentsDeliversValue()
        {
            var receivedCanMakePayments = false;
            Purchases.Error receivedError = null;

            _purchases.CanMakePayments((canMakePayments, error) =>
            {
                receivedCanMakePayments = canMakePayments;
                receivedError = error;
            });

            SendNativeResponse("_canMakePayments", "{\"canMakePayments\":true}");

            Assert.That(receivedCanMakePayments, Is.True);
            Assert.That(receivedError, Is.Null);
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

            SendNativeResponse("_getCustomerInfo", "{\"customerInfo\":" + CustomerInfoJson + "}");

            Assert.That(receivedError, Is.Null);
            Assert.That(receivedInfo, Is.Not.Null);
            Assert.That(receivedInfo.OriginalAppUserId, Is.EqualTo("user_1"));
            Assert.That(receivedInfo.Entitlements.Verification,
                Is.EqualTo(Purchases.VerificationResult.NotRequested));
            // The mappers send these as explicit nulls, which must not become "null" strings
            // or epoch dates.
            Assert.That(receivedInfo.ManagementURL, Is.Null);
            Assert.That(receivedInfo.OriginalApplicationVersion, Is.Null);
            Assert.That(receivedInfo.LatestExpirationDate, Is.Null);
            Assert.That(receivedInfo.OriginalPurchaseDate, Is.Null);
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

            SendNativeResponse("_getCustomerInfo", ErrorJson(10, "A network error has occurred.",
                "NETWORK_ERROR", "The Internet connection appears to be offline."));

            Assert.That(receivedInfo, Is.Null);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(10));
            Assert.That(receivedError.ReadableErrorCode, Is.EqualTo("NETWORK_ERROR"));
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

            SendNativeResponse("_logIn", "{\"customerInfo\":" + CustomerInfoJson + ",\"created\":true}");

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

            SendNativeResponse("_logIn",
                ErrorJson(14, "The app user ID is not valid.", "INVALID_APP_USER_ID"));

            Assert.That(receivedInfo, Is.Null);
            Assert.That(receivedCreated, Is.False);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(14));
        }

        [Test]
        public void LogOutDeliversCustomerInfo()
        {
            Purchases.CustomerInfo receivedInfo = null;

            _purchases.LogOut((info, error) => receivedInfo = info);

            AssertLastInvocation(nameof(IPurchasesWrapper.LogOut), 0);

            SendNativeResponse("_logOut", "{\"customerInfo\":" + CustomerInfoJson + "}");

            Assert.That(receivedInfo, Is.Not.Null);
        }

        [Test]
        public void RestorePurchasesDeliversCustomerInfo()
        {
            Purchases.CustomerInfo receivedInfo = null;

            _purchases.RestorePurchases((info, error) => receivedInfo = info);

            AssertLastInvocation(nameof(IPurchasesWrapper.RestorePurchases), 0);

            SendNativeResponse("_restorePurchases", "{\"customerInfo\":" + CustomerInfoJson + "}");

            Assert.That(receivedInfo, Is.Not.Null);
        }

        [Test]
        public void SyncPurchasesWithCallbackDeliversCustomerInfo()
        {
            Purchases.CustomerInfo receivedInfo = null;

            _purchases.SyncPurchases((info, error) => receivedInfo = info);

            AssertLastInvocation(nameof(IPurchasesWrapper.SyncPurchases), 0);

            SendNativeResponse("_syncPurchases", "{\"customerInfo\":" + CustomerInfoJson + "}");

            Assert.That(receivedInfo, Is.Not.Null);
        }

        [Test]
        public void SyncAttributesAndOfferingsIfNeededDeliversOfferings()
        {
            Purchases.Offerings receivedOfferings = null;

            _purchases.SyncAttributesAndOfferingsIfNeeded((offerings, error) => receivedOfferings = offerings);

            AssertLastInvocation(nameof(IPurchasesWrapper.SyncAttributesAndOfferingsIfNeeded), 0);

            SendNativeResponse("_syncAttributesAndOfferingsIfNeeded", EmptyOfferingsJson);

            Assert.That(receivedOfferings, Is.Not.Null);
            Assert.That(receivedOfferings.All, Is.Empty);
        }

        [Test]
        public void CheckTrialOrIntroductoryPriceEligibilityDeliversAndroidStatuses()
        {
            var identifiers = new[] { "monthly", "annual" };
            Dictionary<string, Purchases.IntroEligibility> receivedEligibility = null;

            _purchases.CheckTrialOrIntroductoryPriceEligibility(identifiers, eligibility => receivedEligibility = eligibility);

            var invocation =
                AssertLastInvocation(nameof(IPurchasesWrapper.CheckTrialOrIntroductoryPriceEligibility), 1);
            Assert.That(invocation.Arguments[0], Is.SameAs(identifiers));

            // common.kt on Android is a stub: it echoes every requested identifier back with
            // INTRO_ELIGIBILITY_STATUS_UNKNOWN and a fixed description.
            SendNativeResponse("_checkTrialOrIntroductoryPriceEligibility",
                "{\"monthly\":{\"status\":0,\"description\":\"Status indeterminate.\"}," +
                "\"annual\":{\"status\":0,\"description\":\"Status indeterminate.\"}}");

            Assert.That(receivedEligibility, Is.Not.Null);
            Assert.That(receivedEligibility, Has.Count.EqualTo(2));
            Assert.That(receivedEligibility["monthly"].Status,
                Is.EqualTo(Purchases.IntroEligibilityStatus.IntroEligibilityStatusUnknown));
            Assert.That(receivedEligibility["monthly"].Description, Is.EqualTo("Status indeterminate."));
            Assert.That(receivedEligibility["annual"].Status,
                Is.EqualTo(Purchases.IntroEligibilityStatus.IntroEligibilityStatusUnknown));
        }

        [Test]
        public void CheckTrialOrIntroductoryPriceEligibilityDeliversIosStatuses()
        {
            Dictionary<string, Purchases.IntroEligibility> receivedEligibility = null;

            _purchases.CheckTrialOrIntroductoryPriceEligibility(new[] { "monthly" },
                eligibility => receivedEligibility = eligibility);

            // CommonFunctionality on iOS sends IntroEligibilityStatus.rawValue plus
            // IntroEligibility.description.
            SendNativeResponse("_checkTrialOrIntroductoryPriceEligibility",
                "{\"monthly\":{\"status\":2," +
                "\"description\":\"Eligible for trial or introductory price.\"}}");

            Assert.That(receivedEligibility, Is.Not.Null);
            Assert.That(receivedEligibility["monthly"].Status,
                Is.EqualTo(Purchases.IntroEligibilityStatus.IntroEligibilityStatusEligible));
            Assert.That(receivedEligibility["monthly"].Description,
                Is.EqualTo("Eligible for trial or introductory price."));
        }

        [Test]
        public void GetStorefrontDeliversPopulatedStorefront()
        {
            Purchases.Storefront receivedStorefront = null;

            _purchases.GetStorefront(storefront => receivedStorefront = storefront);

            AssertLastInvocation(nameof(IPurchasesWrapper.GetStorefront), 0);

            // common.kt on Android maps the storefront to a single countryCode entry.
            SendNativeResponse("_receiveStorefront", "{\"countryCode\":\"US\"}");

            Assert.That(receivedStorefront, Is.Not.Null);
            Assert.That(receivedStorefront.CountryCode, Is.EqualTo("US"));
        }

        [Test]
        public void GetStorefrontIgnoresIosIdentifier()
        {
            Purchases.Storefront receivedStorefront = null;

            _purchases.GetStorefront(storefront => receivedStorefront = storefront);

            // CommonFunctionality on iOS also sends the App Store storefront identifier,
            // which Purchases.Storefront has no field for.
            SendNativeResponse("_receiveStorefront", "{\"identifier\":\"143441\",\"countryCode\":\"USA\"}");

            Assert.That(receivedStorefront, Is.Not.Null);
            Assert.That(receivedStorefront.CountryCode, Is.EqualTo("USA"));
        }

        [Test]
        public void GetStorefrontReturnsNullForEmptyObject()
        {
            Purchases.Storefront receivedStorefront = new Purchases.Storefront("non-null-sentinel");

            _purchases.GetStorefront(storefront => receivedStorefront = storefront);

            // Both wrappers send "{}" when the store has no storefront to report.
            SendNativeResponse("_receiveStorefront", "{}");

            Assert.That(receivedStorefront, Is.Null);
        }

        [Test]
        public void GetStorefrontReturnsNullWhenCountryCodeMissing()
        {
            Purchases.Storefront receivedStorefront = new Purchases.Storefront("non-null-sentinel");

            _purchases.GetStorefront(storefront => receivedStorefront = storefront);

            // Defensive: neither wrapper can send a non-empty storefront without a
            // countryCode, but the receiver guards against it, so pin the guard down.
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

            // PromotionalOffer.rc_dictionary on iOS. Android has no success path at all.
            SendNativeResponse("_getPromotionalOffer",
                "{\"identifier\":\"promo\",\"keyIdentifier\":\"key\"," +
                "\"nonce\":\"7dcbc0ad-9d4d-4a49-9a90-1a0e0f1cd3cd\"," +
                "\"signature\":\"sig\",\"timestamp\":1700000000000}");

            Assert.That(receivedOffer, Is.Not.Null);
            Assert.That(receivedOffer.Identifier, Is.EqualTo("promo"));
            Assert.That(receivedOffer.Nonce, Is.EqualTo("7dcbc0ad-9d4d-4a49-9a90-1a0e0f1cd3cd"));
            Assert.That(receivedOffer.Timestamp, Is.EqualTo(1700000000000L));
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

            // CommonFunctionality.productNotFoundError builds a bare NSError, so this is one of
            // the few payloads with no readableErrorCode, and it carries userCancelled.
            SendNativeResponse("_getPromotionalOffer",
                "{\"error\":{\"code\":5,\"message\":\"Couldn't find discount\"," +
                "\"underlyingErrorMessage\":\"\",\"userCancelled\":false}}");

            Assert.That(receivedOffer, Is.Null);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(5));
            Assert.That(receivedError.Message, Is.EqualTo("Couldn't find discount"));
            Assert.That(receivedError.ReadableErrorCode, Is.Null);
        }

        [Test]
        public void GetPromotionalOfferDeliversEmptyErrorOnAndroid()
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

            // common.kt's getPromotionalOffer returns an ErrorContainer with an empty info map,
            // so PurchasesWrapper.java sends an error object with no fields at all.
            SendNativeResponse("_getPromotionalOffer", "{\"error\":{}}");

            Assert.That(receivedOffer, Is.Null);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(0));
            Assert.That(receivedError.Message, Is.Null);
        }

        [Test]
        public void GetCurrentOfferingForPlacementDeliversNullWhenPlacementHasNoOffering()
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

            // Android sends "{}" via sendEmptyJSONObject; on iOS assigning a nil offering to the
            // response dictionary drops the key, producing the same payload.
            SendNativeResponse("_getCurrentOfferingForPlacement", "{}");

            Assert.That(receivedOffering, Is.Null);
            Assert.That(receivedError, Is.Null);
        }

        [Test]
        public void GetCurrentOfferingForPlacementDeliversOffering()
        {
            Purchases.Offering receivedOffering = null;

            _purchases.GetCurrentOfferingForPlacement("onboarding", (offering, error) => receivedOffering = offering);

            SendNativeResponse("_getCurrentOfferingForPlacement", "{\"offering\":" + OfferingWithoutPackagesJson + "}");

            Assert.That(receivedOffering, Is.Not.Null);
            Assert.That(receivedOffering.Identifier, Is.EqualTo("default"));
            Assert.That(receivedOffering.Metadata, Is.Empty);
            Assert.That(receivedOffering.WebCheckoutUrl, Is.Null);
        }

        [Test]
        public void GetCurrentOfferingForPlacementDeliversNativeError()
        {
            Purchases.Offering receivedOffering = CreateOffering();
            Purchases.Error receivedError = null;
            var callbackCount = 0;

            _purchases.GetCurrentOfferingForPlacement("onboarding", (offering, error) =>
            {
                callbackCount++;
                receivedOffering = offering;
                receivedError = error;
            });

            // Both wrappers send only the "error" key on failure — no "offering" key — so a
            // failure has to be distinguishable from "no offering configured for this placement".
            SendNativeResponse("_getCurrentOfferingForPlacement",
                ErrorJson(10, "A network error has occurred.", "NETWORK_ERROR"));

            Assert.That(callbackCount, Is.EqualTo(1));
            Assert.That(receivedOffering, Is.Null);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(10));
            Assert.That(receivedError.ReadableErrorCode, Is.EqualTo("NETWORK_ERROR"));
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

            // Amazon-only callback: AmazonBilling reports a StoreProblemError when the user data
            // has no LWA consent status.
            SendNativeResponse("_getAmazonLWAConsentStatus", ErrorJson(2,
                "There was a problem with the store.", "StoreProblemError",
                "Failed to get LWA Consent Status from user data. It was null."));

            Assert.That(receivedConsent, Is.False);
            Assert.That(receivedError, Is.Not.Null);
            Assert.That(receivedError.Code, Is.EqualTo(2));
            Assert.That(receivedError.ReadableErrorCode, Is.EqualTo("StoreProblemError"));
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
            return new Purchases.StoreProduct(JSONNode.Parse(NonSubscriptionProductJson));
        }

        /// StoreProductDiscount.rc_dictionary on iOS. Android never sends discounts.
        private static Purchases.Discount CreateDiscount()
        {
            return new Purchases.Discount(JSONNode.Parse(
                "{\"identifier\":\"intro\",\"price\":4.99,\"priceString\":\"$4.99\",\"cycles\":1," +
                "\"period\":\"P1M\",\"periodUnit\":\"MONTH\",\"periodNumberOfUnits\":1}"
            ));
        }

        private static Purchases.Offering CreateOffering()
        {
            return new Purchases.Offering(JSONNode.Parse(OfferingWithoutPackagesJson));
        }
    }
}

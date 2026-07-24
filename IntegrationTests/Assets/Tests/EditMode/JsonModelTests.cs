using System;
using NUnit.Framework;
using RevenueCat.SimpleJSON;

namespace RevenueCat.Tests
{
    public class JsonModelTests
    {
        private const string MinimalCustomerInfoJson =
            "{\"entitlements\":{\"all\":{},\"active\":{}}," +
            "\"activeSubscriptions\":[],\"allPurchasedProductIdentifiers\":[]," +
            "\"firstSeenMillis\":1700000000000,\"originalAppUserId\":\"user_1\"," +
            "\"requestDateMillis\":1700000000001,\"allExpirationDatesMillis\":{}," +
            "\"allPurchaseDatesMillis\":{},\"nonSubscriptionTransactions\":[]," +
            "\"subscriptionsByProductIdentifier\":{}}";

        [Test]
        public void VirtualCurrenciesParsesCurrenciesByCode()
        {
            var response = JSONNode.Parse(
                "{\"all\":{" +
                "\"COIN\":{\"balance\":120,\"name\":\"Coins\",\"code\":\"COIN\",\"serverDescription\":\"Earned in game\"}," +
                "\"GEM\":{\"balance\":4,\"name\":\"Gems\",\"code\":\"GEM\",\"serverDescription\":null}" +
                "}}"
            );

            var virtualCurrencies = new Purchases.VirtualCurrencies(response);

            Assert.That(virtualCurrencies.All.Keys, Is.EquivalentTo(new[] { "COIN", "GEM" }));
            Assert.That(virtualCurrencies.All["COIN"].Balance, Is.EqualTo(120));
            Assert.That(virtualCurrencies.All["COIN"].Name, Is.EqualTo("Coins"));
            Assert.That(virtualCurrencies.All["COIN"].ServerDescription, Is.EqualTo("Earned in game"));
            Assert.That(virtualCurrencies.All["GEM"].Balance, Is.EqualTo(4));
            Assert.That(virtualCurrencies.All["GEM"].ServerDescription, Is.Null);
        }

        [Test]
        public void SubscriptionPriceSupportsValuesAboveInt32Range()
        {
            var response = JSONNode.Parse(
                "{\"formatted\":\"$4,294.97\",\"amountMicros\":4294970000,\"currencyCode\":\"USD\"}"
            );

            var price = new Purchases.SubscriptionOption.Price(response);

            Assert.That(price.AmountMicros, Is.EqualTo(4294970000L));
        }

        [Test]
        public void StoreProductFallsBackToUnknownProductCategory()
        {
            var response = JSONNode.Parse(
                "{\"title\":\"Lifetime\",\"identifier\":\"lifetime\",\"description\":\"Lifetime access\"," +
                "\"price\":99.99,\"priceString\":\"$99.99\",\"currencyCode\":\"USD\"," +
                "\"productCategory\":\"UNRECOGNIZED\"}"
            );

            var product = new Purchases.StoreProduct(response);

            Assert.That(product.Identifier, Is.EqualTo("lifetime"));
            Assert.That(product.ProductCategory, Is.EqualTo(Purchases.ProductCategory.UNKNOWN));
            Assert.That(product.DefaultOption, Is.Null);
            Assert.That(product.Discounts, Is.Null);
        }

        [Test]
        public void CustomerInfoParsesFullPayload()
        {
            var response = JSONNode.Parse(
                "{\"entitlements\":{" +
                "\"all\":{\"premium\":{\"identifier\":\"premium\",\"isActive\":true,\"willRenew\":true," +
                "\"periodType\":\"NORMAL\",\"latestPurchaseDateMillis\":1700000000000," +
                "\"originalPurchaseDateMillis\":1690000000000,\"store\":\"APP_STORE\"," +
                "\"productIdentifier\":\"monthly\",\"isSandbox\":false}}," +
                "\"active\":{\"premium\":{\"identifier\":\"premium\",\"isActive\":true,\"willRenew\":true," +
                "\"periodType\":\"NORMAL\",\"latestPurchaseDateMillis\":1700000000000," +
                "\"originalPurchaseDateMillis\":1690000000000,\"store\":\"APP_STORE\"," +
                "\"productIdentifier\":\"monthly\",\"isSandbox\":false}}}," +
                "\"activeSubscriptions\":[\"monthly\"]," +
                "\"allPurchasedProductIdentifiers\":[\"monthly\",\"lifetime\"]," +
                "\"firstSeenMillis\":1700000000000,\"originalAppUserId\":\"user_1\"," +
                "\"requestDateMillis\":1700000000001," +
                "\"originalPurchaseDateMillis\":1690000000000," +
                "\"latestExpirationDateMillis\":1720000000000," +
                "\"managementURL\":\"https://mgmt\"," +
                "\"allExpirationDatesMillis\":{\"monthly\":1720000000000,\"lifetime\":0}," +
                "\"allPurchaseDatesMillis\":{\"monthly\":1700000000000,\"lifetime\":0}," +
                "\"originalApplicationVersion\":\"1.0\"," +
                "\"nonSubscriptionTransactions\":[{\"transactionIdentifier\":\"txn_1\"," +
                "\"productIdentifier\":\"lifetime\",\"purchaseDateMillis\":1700000000000}]," +
                "\"subscriptionsByProductIdentifier\":{\"monthly\":{\"productIdentifier\":\"monthly\"," +
                "\"purchaseDate\":\"2023-01-01T00:00:00Z\",\"store\":\"APP_STORE\",\"isSandbox\":false," +
                "\"periodType\":\"NORMAL\",\"isActive\":true,\"willRenew\":true}}}"
            );

            var customerInfo = new Purchases.CustomerInfo(response);

            Assert.That(customerInfo.OriginalAppUserId, Is.EqualTo("user_1"));
            Assert.That(customerInfo.Entitlements.All["premium"].IsActive, Is.True);
            Assert.That(customerInfo.Entitlements.Active.ContainsKey("premium"), Is.True);
            Assert.That(customerInfo.ActiveSubscriptions, Is.EquivalentTo(new[] { "monthly" }));
            Assert.That(customerInfo.AllPurchasedProductIdentifiers, Has.Count.EqualTo(2));
            Assert.That(customerInfo.OriginalPurchaseDate, Is.Not.Null);
            Assert.That(customerInfo.LatestExpirationDate, Is.Not.Null);
            Assert.That(customerInfo.ManagementURL, Is.EqualTo("https://mgmt"));
            Assert.That(customerInfo.OriginalApplicationVersion, Is.EqualTo("1.0"));
            Assert.That(customerInfo.NonSubscriptionTransactions, Has.Count.EqualTo(1));
            Assert.That(customerInfo.NonSubscriptionTransactions[0].ProductIdentifier, Is.EqualTo("lifetime"));
            Assert.That(customerInfo.SubscriptionsByProductIdentifier["monthly"].ProductIdentifier,
                Is.EqualTo("monthly"));

            // A millis value of exactly 0 is indistinguishable from an absent date and is parsed as null.
            Assert.That(customerInfo.AllExpirationDates["monthly"], Is.Not.Null);
            Assert.That(customerInfo.AllExpirationDates["lifetime"], Is.Null);
            Assert.That(customerInfo.AllPurchaseDates["monthly"], Is.Not.Null);
            Assert.That(customerInfo.AllPurchaseDates["lifetime"], Is.Null);
        }

        [Test]
        public void SubscriptionOptionParsesFullPayloadWithPricingPhases()
        {
            const string pricingPhaseJson =
                "{\"billingPeriod\":{\"unit\":\"MONTH\",\"value\":1,\"iso8601\":\"P1M\"}," +
                "\"recurrenceMode\":\"INFINITE_RECURRING\",\"billingCycleCount\":0," +
                "\"price\":{\"formatted\":\"$9.99\",\"amountMicros\":9990000,\"currencyCode\":\"USD\"}," +
                "\"offerPaymentMode\":\"SINGLE_PAYMENT\"}";

            var response = JSONNode.Parse(
                "{\"id\":\"monthly:base\",\"storeProductId\":\"monthly\",\"productId\":\"monthly\"," +
                "\"tags\":[\"tag1\"],\"isBasePlan\":true," +
                "\"billingPeriod\":{\"unit\":\"MONTH\",\"value\":1,\"iso8601\":\"P1M\"},\"isPrepaid\":false," +
                "\"pricingPhases\":[" + pricingPhaseJson + "]," +
                "\"fullPricePhase\":" + pricingPhaseJson + "," +
                "\"presentedOfferingContext\":{\"offeringIdentifier\":\"default\"}," +
                "\"installmentsInfo\":{\"commitmentPaymentsCount\":3,\"renewalCommitmentPaymentsCount\":1}}"
            );

            var option = new Purchases.SubscriptionOption(response);

            Assert.That(option.Tags, Is.EquivalentTo(new[] { "tag1" }));
            Assert.That(option.PricingPhases, Has.Length.EqualTo(1));
            Assert.That(option.FullPricePhase, Is.Not.Null);
            Assert.That(option.FullPricePhase.RecurrenceMode,
                Is.EqualTo(Purchases.SubscriptionOption.RecurrenceMode.INFINITE_RECURRING));
            Assert.That(option.PresentedOfferingContext, Is.Not.Null);
            Assert.That(option.PresentedOfferingContext.OfferingIdentifier, Is.EqualTo("default"));
            Assert.That(option.OptionInstallmentsInfo, Is.Not.Null);
            Assert.That(option.OptionInstallmentsInfo.CommitmentPaymentsCount, Is.EqualTo(3));
        }

        [Test]
        public void SubscriptionOptionParsesMinimalPayloadWithoutOptionalFields()
        {
            var response = JSONNode.Parse(
                "{\"id\":\"monthly:base\",\"storeProductId\":\"monthly\",\"productId\":\"monthly\"," +
                "\"tags\":[],\"isBasePlan\":true," +
                "\"billingPeriod\":{\"unit\":\"MONTH\",\"value\":1,\"iso8601\":\"P1M\"},\"isPrepaid\":false}"
            );

            var option = new Purchases.SubscriptionOption(response);

            Assert.That(option.PricingPhases, Is.Null);
            Assert.That(option.FullPricePhase, Is.Null);
            Assert.That(option.FreePhase, Is.Null);
            Assert.That(option.IntroPhase, Is.Null);
            Assert.That(option.PresentedOfferingContext, Is.Null);
            Assert.That(option.OptionInstallmentsInfo, Is.Null);
        }

        [Test]
        public void SubscriptionOptionPricingPhaseFallsBackToUnknownForUnrecognizedEnums()
        {
            var response = JSONNode.Parse(
                "{\"id\":\"monthly:base\",\"storeProductId\":\"monthly\",\"productId\":\"monthly\"," +
                "\"tags\":[],\"isBasePlan\":true," +
                "\"billingPeriod\":{\"unit\":\"MONTH\",\"value\":1,\"iso8601\":\"P1M\"},\"isPrepaid\":false," +
                "\"fullPricePhase\":{\"billingPeriod\":{\"unit\":\"MONTH\",\"value\":1,\"iso8601\":\"P1M\"}," +
                "\"recurrenceMode\":\"BOGUS\",\"billingCycleCount\":0," +
                "\"price\":{\"formatted\":\"$9.99\",\"amountMicros\":9990000,\"currencyCode\":\"USD\"}," +
                "\"offerPaymentMode\":\"BOGUS\"}}"
            );

            var option = new Purchases.SubscriptionOption(response);

            Assert.That(option.FullPricePhase.RecurrenceMode,
                Is.EqualTo(Purchases.SubscriptionOption.RecurrenceMode.UNKNOWN));
            Assert.That(option.FullPricePhase.OfferPaymentMode,
                Is.EqualTo(Purchases.SubscriptionOption.OfferPaymentMode.UNKNOWN));
        }

        [Test]
        public void WebPurchaseRedemptionWrapsRedemptionLink()
        {
            var redemption = new Purchases.WebPurchaseRedemption("https://rev.cat/redeem/abc");

            Assert.That(redemption.RedemptionLink, Is.EqualTo("https://rev.cat/redeem/abc"));
        }

        [Test]
        public void WebPurchaseRedemptionResultParsesSuccessVariant()
        {
            var response = JSONNode.Parse(
                "{\"result\":\"SUCCESS\",\"customerInfo\":" + MinimalCustomerInfoJson + "}"
            );

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.InstanceOf<Purchases.WebPurchaseRedemptionResult.Success>());
            Assert.That(((Purchases.WebPurchaseRedemptionResult.Success)result).CustomerInfo, Is.Not.Null);
        }

        [Test]
        public void WebPurchaseRedemptionResultParsesErrorVariant()
        {
            var response = JSONNode.Parse(
                "{\"result\":\"ERROR\",\"error\":{\"message\":\"Redemption failed\",\"code\":11," +
                "\"underlyingErrorMessage\":\"Backend rejected\",\"readableErrorCode\":\"UNKNOWN_ERROR\"}}"
            );

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.InstanceOf<Purchases.WebPurchaseRedemptionResult.RedemptionError>());
            Assert.That(((Purchases.WebPurchaseRedemptionResult.RedemptionError)result).Error.Code, Is.EqualTo(11));
        }

        [Test]
        public void WebPurchaseRedemptionResultParsesInvalidTokenVariant()
        {
            var response = JSONNode.Parse("{\"result\":\"INVALID_TOKEN\"}");

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.SameAs(Purchases.WebPurchaseRedemptionResult.InvalidToken.Instance));
        }

        [Test]
        public void WebPurchaseRedemptionResultParsesExpiredVariant()
        {
            var response = JSONNode.Parse("{\"result\":\"EXPIRED\",\"obfuscatedEmail\":\"a***@b.com\"}");

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.InstanceOf<Purchases.WebPurchaseRedemptionResult.Expired>());
            Assert.That(((Purchases.WebPurchaseRedemptionResult.Expired)result).ObfuscatedEmail,
                Is.EqualTo("a***@b.com"));
        }

        [Test]
        public void WebPurchaseRedemptionResultParsesPurchaseBelongsToOtherUserVariant()
        {
            var response = JSONNode.Parse("{\"result\":\"PURCHASE_BELONGS_TO_OTHER_USER\"}");

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.SameAs(Purchases.WebPurchaseRedemptionResult.PurchaseBelongsToOtherUser.Instance));
        }

        [Test]
        public void WebPurchaseRedemptionResultThrowsForUnrecognizedResultType()
        {
            var response = JSONNode.Parse("{\"result\":\"SOMETHING_ELSE\"}");

            Assert.Throws<ArgumentException>(() => Purchases.WebPurchaseRedemptionResult.FromJson(response));
        }
    }
}

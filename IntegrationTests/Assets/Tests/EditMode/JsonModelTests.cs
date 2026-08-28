using System;
using System.IO;
using NUnit.Framework;
using RevenueCat;
using RevenueCat.SimpleJSON;
using UnityEngine;

namespace RevenueCat.Tests
{
    public class JsonModelTests
    {
        private static JSONNode LoadFixture(string filename)
        {
            var path = Path.Combine(Application.dataPath, "Tests", "EditMode", "JsonModelFixtures", filename);
            return JSONNode.Parse(File.ReadAllText(path));
        }

        [Test]
        public void VirtualCurrenciesParsesCurrenciesByCode()
        {
            var response = LoadFixture("virtual-currencies.json");

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
            var response = LoadFixture("subscription-price-large-amount.json");

            var price = new Purchases.SubscriptionOption.Price(response);

            Assert.That(price.AmountMicros, Is.EqualTo(4294970000L));
        }

        [Test]
        public void StoreProductParsesKnownProductCategory()
        {
            var response = LoadFixture("store-product-known-category.json");

            var product = new Purchases.StoreProduct(response);

            Assert.That(product.ProductCategory, Is.EqualTo(Purchases.ProductCategory.SUBSCRIPTION));
        }

        [Test]
        public void StoreProductFallsBackToUnknownProductCategory()
        {
            // "UNRECOGNIZED" is deliberately synthetic: StoreProductMapper can only send
            // SUBSCRIPTION, NON_SUBSCRIPTION or UNKNOWN. This pins down the parser's fallback for
            // a category added natively before Unity knows about it.
            var response = LoadFixture("store-product-unrecognized-category.json");

            var product = new Purchases.StoreProduct(response);

            Assert.That(product.Identifier, Is.EqualTo("lifetime"));
            Assert.That(product.ProductCategory, Is.EqualTo(Purchases.ProductCategory.UNKNOWN));
            Assert.That(product.DefaultOption, Is.Null);
            Assert.That(product.Discounts, Is.Null);
        }

        [Test]
        public void CustomerInfoParsesFullPayload()
        {
            var response = LoadFixture("customer-info-full.json");

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
            var response = LoadFixture("subscription-option-full.json");

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
            var response = LoadFixture("subscription-option-minimal.json");

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
            // "BOGUS" is deliberately synthetic, like the UNRECOGNIZED category above: it stands in
            // for a recurrenceMode or offerPaymentMode that Android starts sending before Unity
            // knows the name, and pins down the UNKNOWN fallback.
            var response = LoadFixture("subscription-option-unrecognized-enums.json");

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
            // The nested CustomerInfo comes from purchases-hybrid-common's CustomerInfo.map()
            // (Android) / CustomerInfo.dictionary (iOS) for a user with no purchases. Every
            // optional field is an explicit null, matching the payload that actually ships.
            var response = LoadFixture("web-purchase-redemption-success.json");

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.InstanceOf<Purchases.WebPurchaseRedemptionResult.Success>());
            Assert.That(((Purchases.WebPurchaseRedemptionResult.Success)result).CustomerInfo, Is.Not.Null);
        }

        [Test]
        public void WebPurchaseRedemptionResultParsesErrorVariant()
        {
            // readableErrorCode is ErrorCode.codeName on iOS and PurchasesErrorCode.name on
            // Android; both duplicate it under the deprecated readable_error_code. Code 11 is
            // InvalidCredentialsError, so these are the iOS values for that code.
            var response = LoadFixture("web-purchase-redemption-error.json");

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.InstanceOf<Purchases.WebPurchaseRedemptionResult.RedemptionError>());
            Assert.That(((Purchases.WebPurchaseRedemptionResult.RedemptionError)result).Error.Code, Is.EqualTo(11));
        }

        [Test]
        public void WebPurchaseRedemptionResultParsesInvalidTokenVariant()
        {
            var response = LoadFixture("web-purchase-redemption-invalid-token.json");

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.SameAs(Purchases.WebPurchaseRedemptionResult.InvalidToken.Instance));
        }

        [Test]
        public void WebPurchaseRedemptionResultParsesExpiredVariant()
        {
            var response = LoadFixture("web-purchase-redemption-expired.json");

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.InstanceOf<Purchases.WebPurchaseRedemptionResult.Expired>());
            Assert.That(((Purchases.WebPurchaseRedemptionResult.Expired)result).ObfuscatedEmail,
                Is.EqualTo("a***@b.com"));
        }

        [Test]
        public void WebPurchaseRedemptionResultParsesPurchaseBelongsToOtherUserVariant()
        {
            var response = LoadFixture("web-purchase-redemption-other-user.json");

            var result = Purchases.WebPurchaseRedemptionResult.FromJson(response);

            Assert.That(result, Is.SameAs(Purchases.WebPurchaseRedemptionResult.PurchaseBelongsToOtherUser.Instance));
        }

        [Test]
        public void WebPurchaseRedemptionResultThrowsForUnrecognizedResultType()
        {
            var response = LoadFixture("web-purchase-redemption-unrecognized.json");

            Assert.Throws<ArgumentException>(() => Purchases.WebPurchaseRedemptionResult.FromJson(response));
        }

        [Test]
        public void RewardVerificationTokenParsesFields()
        {
            var response = LoadFixture("reward-verification-token.json");

            var token = new Purchases.RewardVerificationToken(response);

            Assert.That(token.CustomData, Is.EqualTo("txn_abc123"));
            Assert.That(token.ClientTransactionId, Is.EqualTo("txn_abc123"));
            Assert.That(token.AppUserID, Is.EqualTo("user_1"));
        }

        [Test]
        public void VerifiedRewardParsesVirtualCurrencyVariant()
        {
            var response = LoadFixture("verified-reward-virtual-currency.json");

            var reward = (Purchases.VerifiedReward.VirtualCurrency)Purchases.VerifiedReward.FromJson(response);

            Assert.That(reward.Code, Is.EqualTo("COIN"));
            Assert.That(reward.Amount, Is.EqualTo(50));
        }

        [Test]
        public void VerifiedRewardParsesEntitlementVariant()
        {
            var response = LoadFixture("verified-reward-entitlement.json");

            var reward = (Purchases.VerifiedReward.Entitlement)Purchases.VerifiedReward.FromJson(response);

            Assert.That(reward.Identifier, Is.EqualTo("premium"));
            Assert.That(reward.ExpiresAt, Is.EqualTo("2023-11-14T22:13:20.000Z"));
            Assert.That(reward.ExpiresAtMillis, Is.EqualTo(1700000000000));
        }

        [Test]
        public void VerifiedRewardParsesNoRewardVariant()
        {
            var response = LoadFixture("verified-reward-no-reward.json");

            var reward = Purchases.VerifiedReward.FromJson(response);

            Assert.That(reward, Is.SameAs(Purchases.VerifiedReward.NoReward.Instance));
        }

        [Test]
        public void VerifiedRewardFallsBackToUnsupportedForUnrecognizedType()
        {
            // Deliberately synthetic, like the UNRECOGNIZED category tests above: stands in for a
            // reward type added natively before Unity knows about it.
            var response = LoadFixture("verified-reward-unsupported.json");

            var reward = Purchases.VerifiedReward.FromJson(response);

            Assert.That(reward, Is.SameAs(Purchases.VerifiedReward.Unsupported.Instance));
        }

        [Test]
        public void RewardVerificationResultParsesRewardAndMoreRewards()
        {
            var response = LoadFixture("reward-verification-result-success.json");

            var result = new Purchases.RewardVerificationResult(response);

            Assert.That(result.Failed, Is.False);
            Assert.That(result.Reward, Is.InstanceOf<Purchases.VerifiedReward.VirtualCurrency>());
            Assert.That(result.MoreRewards, Has.Count.EqualTo(1));
            Assert.That(result.MoreRewards[0], Is.InstanceOf<Purchases.VerifiedReward.Entitlement>());
        }

        [Test]
        public void RewardVerificationResultLeavesRewardNullWhenFailed()
        {
            var response = LoadFixture("reward-verification-result-failed.json");

            var result = new Purchases.RewardVerificationResult(response);

            Assert.That(result.Failed, Is.True);
            Assert.That(result.Reward, Is.Null);
            Assert.That(result.MoreRewards, Is.Empty);
        }

        [Test]
        public void RewardVerificationResultTreatsExplicitNullMoreRewardsAsEmpty()
        {
            // moreRewards can arrive as an explicit JSON null rather than an empty array or a
            // missing key; this must not throw.
            var response = LoadFixture("reward-verification-result-null-more-rewards.json");

            var result = new Purchases.RewardVerificationResult(response);

            Assert.That(result.Failed, Is.False);
            Assert.That(result.Reward, Is.InstanceOf<Purchases.VerifiedReward.VirtualCurrency>());
            Assert.That(result.MoreRewards, Is.Empty);
        }

        [Test]
        public void RewardedAdTrackingMetadataSerializesAllFields()
        {
            var metadata = new RewardedAdTrackingMetadata(
                AdTracker.MediatorName.AdMob,
                AdTracker.Format.Rewarded,
                "ad_unit_1",
                "impression_1",
                "network_1",
                "placement_1");

            var json = JSONNode.Parse(metadata.ToJsonString());

            Assert.That(json["mediatorName"].Value, Is.EqualTo("AdMob"));
            Assert.That(json["adFormat"].Value, Is.EqualTo("rewarded"));
            Assert.That(json["adUnitId"].Value, Is.EqualTo("ad_unit_1"));
            Assert.That(json["impressionId"].Value, Is.EqualTo("impression_1"));
            Assert.That(json["networkName"].Value, Is.EqualTo("network_1"));
            Assert.That(json["placement"].Value, Is.EqualTo("placement_1"));
        }

        [Test]
        public void RewardedAdTrackingMetadataOmitsOptionalFieldsWhenNull()
        {
            var metadata = new RewardedAdTrackingMetadata(
                AdTracker.MediatorName.AppLovin,
                AdTracker.Format.Interstitial,
                "ad_unit_1",
                "impression_1");

            var json = JSONNode.Parse(metadata.ToJsonString());

            Assert.That(json.HasKey("networkName"), Is.False);
            Assert.That(json.HasKey("placement"), Is.False);
        }
    }
}

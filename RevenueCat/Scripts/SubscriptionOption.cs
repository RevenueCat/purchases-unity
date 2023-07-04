using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;
using UnityEngine;
public partial class Purchases
{
    /// Contains all details associated with a SubscriptionOption
    /// Used only for Google
    public class SubscriptionOption
    {
        /**
         * Identifier of the subscription option
         * If this SubscriptionOption represents a base plan, this will be the basePlanId.
         * If it represents an offer, it will be {basePlanId}:{offerId}
         */
        public readonly string ID;

        /**
         * Identifier of the StoreProduct associated with this SubscriptionOption
         * This will be {subId}:{basePlanId}
         */
        public readonly string StoreProductId;

        /**
         * Identifer of the subscription associated with this SubscriptionOption
         * This will be {subId}
         */
        public readonly string ProductId;

        /**
         * Pricing phases defining a user's payment plan for the product over time.
         */
        public readonly PricingPhase[] PricingPhases;

        /**
         * Tags defined on the base plan or offer. Empty for Amazon.
         */
        public readonly string[] Tags;

        /**
         * True if this SubscriptionOption represents a subscription base plan (rather than an offer).
         */
        public readonly bool IsBasePlan;

        /**
         * The subscription period of fullPricePhase (after free and intro trials).
         */
        public readonly Period BillingPeriod;

        /**
         * True if the subscription is pre-paid.
         */
        public readonly bool IsPrepaid;

        /**
         * The full price PricingPhase of the subscription.
         * Looks for the last price phase of the SubscriptionOption.
         */
        [CanBeNull] public readonly PricingPhase FullPricePhase;

        /**
         * The free trial PricingPhase of the subscription.
         * Looks for the first pricing phase of the SubscriptionOption where amountMicros is 0.
         * There can be a freeTrialPhase and an introductoryPhase in the same SubscriptionOption.
         */
        [CanBeNull] public readonly PricingPhase FreePhase;

        /**
         * The intro trial PricingPhase of the subscription.
         * Looks for the first pricing phase of the SubscriptionOption where amountMicros is greater than 0.
         * There can be a freeTrialPhase and an introductoryPhase in the same SubscriptionOption.
         */
        [CanBeNull] public readonly PricingPhase IntroPhase;

        /**
         * Offering identifier the subscription option was presented from
         */
        [CanBeNull] public readonly string PresentedOfferingIdentifier;

        public SubscriptionOption(JSONNode response)
        {
            ID = response["id"];
            StoreProductId = response["storeProductId"];
            ProductId = response["productId"];
            var tagsResponse = response["tags"];
            var tagsTemporaryList = new List<string>(); 
            foreach (var tag in tagsResponse)
            {
                tagsTemporaryList.Add(tag.Value);
            }
            Tags = tagsTemporaryList.ToArray();
            IsBasePlan = response["isBasePlan"];
            BillingPeriod = new Period(response["billingPeriod"]);
            IsPrepaid = response["isPrepaid"];
            var fullPricePhaseNode = response["fullPricePhase"];
            if (fullPricePhaseNode != null && !fullPricePhaseNode.IsNull)
            {
                FullPricePhase = new PricingPhase(fullPricePhaseNode);
            }
            var freePhaseNode = response["freePhase"];
            if (freePhaseNode != null && !freePhaseNode.IsNull)
            {
                FreePhase = new PricingPhase(freePhaseNode);
            }
            var introPhaseNode = response["introPhase"];
            if (introPhaseNode != null && !introPhaseNode.IsNull)
            {
                IntroPhase = new PricingPhase(introPhaseNode);
            }
            PresentedOfferingIdentifier = response["presentedOfferingIdentifier"];
        }

        public override string ToString()
        {
            return $"{nameof(ID)}: {ID}\n" +
                   $"{nameof(StoreProductId)}: {StoreProductId}\n" +
                   $"{nameof(ProductId)}: {ProductId}\n" +
                   $"{nameof(Tags)}: {Tags}\n" +
                   $"{nameof(IsBasePlan)}: {IsBasePlan}\n" +
                   $"{nameof(BillingPeriod)}: {BillingPeriod}\n" +
                   $"{nameof(IsPrepaid)}: {IsPrepaid}\n" +
                   $"{nameof(FullPricePhase)}: {FullPricePhase}\n" +
                   $"{nameof(FreePhase)}: {FreePhase}\n" +
                   $"{nameof(IntroPhase)}: {IntroPhase}\n" +
                   $"{nameof(PresentedOfferingIdentifier)}: {PresentedOfferingIdentifier}";
        }

        public class PricingPhase
        {
            /**
            * Billing period for which the PricingPhase applies
            */
            public readonly Period BillingPeriod;

            /**
            * Recurrence mode of the PricingPhase
            */
            public readonly RecurrenceMode RecurrenceMode;

            /**
            * Number of cycles for which the pricing phase applies.
            * Null for infiniteRecurring or finiteRecurring recurrence modes.
            */
            [CanBeNull] public readonly int BillingCycleCount;

            /**
            * Price of the PricingPhase
            */
            public readonly Price Price;

            /**
            * Indicates how the pricing phase is charged for finiteRecurring pricing phases
            */
            [CanBeNull] public readonly OfferPaymentMode OfferPaymentMode;

            public PricingPhase(JSONNode response)
            {
                BillingPeriod = new Period(response["billingPeriod"]);
                // RecurrenceMode = response["recurrenceMode"];
                BillingCycleCount = response["billingCycleCount"];
                Price = new Price(response["price"]);
            }

            public override string ToString()
            {
                return $"{nameof(BillingPeriod)}: {BillingPeriod}\n" +
                    $"{nameof(RecurrenceMode)}: {RecurrenceMode}\n" +
                    $"{nameof(BillingCycleCount)}: {BillingCycleCount}\n" +
                    $"{nameof(Price)}: {Price}\n" +
                    $"{nameof(OfferPaymentMode)}: {OfferPaymentMode}\n";
            }
        }

        public class Period
        {
            public readonly PeriodUnit Unit;

            /**
            * The increment of time that a subscription period is specified in
            */
            public readonly int Value;

            /**
            * Specified in ISO 8601 format. For example, P1W equates to one week,
            * P1M equates to one month, P3M equates to three months, P6M equates to six months,
            * and P1Y equates to one year
            */
            public readonly string ISO8601;

            public Period(JSONNode response)
            {
                Unit = PeriodUnit.PeriodUnitDay; //response["unit"]
                Value = (int) response["value"];
                ISO8601 = response["iso8601"];
            }

            public override string ToString()
            {
                return $"{nameof(Unit)}: {Unit}\n" +
                    $"{nameof(Value)}: {Value}\n" +
                    $"{nameof(ISO8601)}: {ISO8601}\n";
            }
        }

        public enum PeriodUnit {
            PeriodUnitDay = 0,

            PeriodUnitWeek = 1,

            PeriodUnitMonth = 2,

            PeriodUnitYear = 3,

            PeriodUnitUnknown = 4
        }

        /**
        * Recurrence mode for a pricing phase
        */
        public enum RecurrenceMode {
            /**
            * Pricing phase repeats infinitely until cancellation
            */
            RecurrenceModeInfiniteRecurring = 1,
            /**
            * Pricing phase repeats for a fixed number of billing periods
            */
            RecurrenceModeFiniteRecurring = 2,
            /**
            * Pricing phase does not repeat
            */
            RecurrenceModeNonRecurring = 3,
        }

        /**
        * Payment mode for offer pricing phases. Google Play only.
        */
        public enum OfferPaymentMode {
            /**
            * Subscribers don't pay until the specified period ends
            */
            OfferPaymentModeFreeTrial = 0,
            /**
            * Subscribers pay up front for a specified period
            */
            OfferPaymentModeSinglePayment = 1,
            /**
            * Subscribers pay a discounted amount for a specified number of periods
            */
            OfferPaymentModeDiscountedRecurringPayment = 2
        }

        /**
        * Contains all the details associated with a Price
        */
        public class Price {
            /**
            * Formatted price of the item, including its currency sign. For example $3.00
            */
            public readonly string Formatted;

            /**
            * Price in micro-units, where 1,000,000 micro-units equal one unit of the currency.
            * 
            * For example, if price is "€7.99", price_amount_micros is 7,990,000. This value represents
            * the localized, rounded price for a particular currency.
            */
            public readonly int AmountMicros;

            /**
            * Returns ISO 4217 currency code for price and original price.
            * 
            * For example, if price is specified in British pounds sterling, price_currency_code is "GBP".
            * If currency code cannot be determined, currency symbol is returned.
            */
            public readonly string CurrencyCode;

            public Price(JSONNode response)
            {
                Formatted = response["formatted"];
                AmountMicros = response["amountMicros"];
                CurrencyCode = response["currencyCode"];
            }

            public override string ToString()
            {
                return $"{nameof(Formatted)}: {Formatted}\n" +
                    $"{nameof(AmountMicros)}: {AmountMicros}\n" +
                    $"{nameof(CurrencyCode)}: {CurrencyCode}\n";
            }
        }
    }
}
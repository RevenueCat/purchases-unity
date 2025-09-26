using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace RevenueCat
{
    /// <summary>
    /// Contains all details associated with a SubscriptionOption
    /// Used only for Google
    /// </summary>
    public sealed class SubscriptionOption
    {
        /// <summary>
        /// Identifier of the subscription option
        /// If this SubscriptionOption represents a base plan, this will be the basePlanId.
        /// If it represents an offer, it will be {basePlanId}:{offerId}
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Identifier of the StoreProduct associated with this SubscriptionOption
        /// This will be {subId}:{basePlanId}
        /// </summary>
        public string StoreProductId { get; }

        /// <summary>
        /// Identifier of the subscription associated with this SubscriptionOption
        /// This will be {subId}
        /// </summary>
        public string ProductId { get; }

        /// <summary>
        /// Pricing phases defining a user's payment plan for the product over time.
        /// </summary>
        public IReadOnlyList<PricingPhase> PricingPhases { get; }

        /// <summary>
        /// Tags defined on the base plan or offer. Empty for Amazon.
        /// </summary>
        public IReadOnlyList<string> Tags { get; }

        /// <summary>
        /// True if this SubscriptionOption represents a subscription base plan (rather than an offer).
        /// </summary>
        public bool IsBasePlan { get; }

        /// <summary>
        /// The subscription period of fullPricePhase (after free and intro trials).
        /// </summary>
        public Period BillingPeriod { get; }

        /// <summary>
        /// True if the subscription is pre-paid.
        /// </summary>
        public bool IsPrepaid { get; }

        /// <summary>
        /// The full price PricingPhase of the subscription.
        /// Looks for the last price phase of the SubscriptionOption.
        /// </summary>
        [CanBeNull]
        public PricingPhase FullPricePhase { get; }

        /// <summary>
        /// The free trial PricingPhase of the subscription.
        /// Looks for the first pricing phase of the SubscriptionOption where amountMicros is 0.
        /// There can be a freeTrialPhase and an introductoryPhase in the same SubscriptionOption.
        /// </summary>
        [CanBeNull]
        public PricingPhase FreePhase { get; }

        /// <summary>
        /// The intro trial PricingPhase of the subscription.
        /// Looks for the first pricing phase of the SubscriptionOption where amountMicros is greater than 0.
        /// There can be a freeTrialPhase and an introductoryPhase in the same SubscriptionOption.
        /// </summary>
        [CanBeNull]
        public PricingPhase IntroPhase { get; }

        [CanBeNull]
        public PresentedOfferingContext PresentedOfferingContext { get; }

        /// <summary>
        /// Offering identifier the subscription option was presented from
        /// </summary>
        [Obsolete("Deprecated, use PresentedOfferingContext instead.", false)]
        [CanBeNull]
        [JsonIgnore]
        public string PresentedOfferingIdentifier => PresentedOfferingContext?.OfferingIdentifier;

        /// <summary>
        /// Information about the installment subscription. Currently only supported in Google Play.
        /// </summary>
        [CanBeNull]
        public InstallmentsInfo OptionInstallmentsInfo { get; }

        [JsonConstructor]
        internal SubscriptionOption(
            [JsonProperty("id")] string id,
            [JsonProperty("storeProductId")] string storeProductId,
            [JsonProperty("productId")] string productId,
            [JsonProperty("pricingPhases")] List<PricingPhase> pricingPhases,
            [JsonProperty("tags")] List<string> tags,
            [JsonProperty("isBasePlan")] bool isBasePlan,
            [JsonProperty("billingPeriod")] Period billingPeriod,
            [JsonProperty("isPrepaid")] bool isPrepaid,
            [JsonProperty("fullPricePhase")] PricingPhase fullPricePhase,
            [JsonProperty("freePhase")] PricingPhase freePhase,
            [JsonProperty("introPhase")] PricingPhase introPhase,
            [JsonProperty("presentedOfferingContext")] PresentedOfferingContext presentedOfferingContext,
            [JsonProperty("installmentsInfo")] InstallmentsInfo optionInstallmentsInfo)
        {
            Id = id;
            StoreProductId = storeProductId;
            ProductId = productId;
            PricingPhases = pricingPhases;
            Tags = tags;
            IsBasePlan = isBasePlan;
            BillingPeriod = billingPeriod;
            IsPrepaid = isPrepaid;
            FullPricePhase = fullPricePhase;
            FreePhase = freePhase;
            IntroPhase = introPhase;
            PresentedOfferingContext = presentedOfferingContext;
            OptionInstallmentsInfo = optionInstallmentsInfo;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}\n" +
                   $"{nameof(StoreProductId)}: {StoreProductId}\n" +
                   $"{nameof(ProductId)}: {ProductId}\n" +
                   $"{nameof(PricingPhases)}: {PricingPhases}\n" +
                   $"{nameof(Tags)}: {Tags}\n" +
                   $"{nameof(IsBasePlan)}: {IsBasePlan}\n" +
                   $"{nameof(BillingPeriod)}: {BillingPeriod}\n" +
                   $"{nameof(IsPrepaid)}: {IsPrepaid}\n" +
                   $"{nameof(FullPricePhase)}: {FullPricePhase}\n" +
                   $"{nameof(FreePhase)}: {FreePhase}\n" +
                   $"{nameof(IntroPhase)}: {IntroPhase}\n" +
#pragma warning disable CS0618 // Type or member is obsolete
                   $"{nameof(PresentedOfferingIdentifier)}: {PresentedOfferingIdentifier}\n" +
#pragma warning restore CS0618 // Type or member is obsolete
                   $"{nameof(OptionInstallmentsInfo)}: {OptionInstallmentsInfo}\n";
        }

        public sealed class PricingPhase
        {
            /// <summary>
            /// Billing period for which the PricingPhase applies
            /// </summary>
            public Period BillingPeriod { get; }

            /// <summary>
            /// Recurrence mode of the PricingPhase
            /// </summary>
            public RecurrenceMode RecurrenceMode { get; }

            /// <summary>
            /// Number of cycles for which the pricing phase applies.
            /// Null for infiniteRecurring or finiteRecurring recurrence modes.
            /// </summary>
            public int? BillingCycleCount { get; }

            /// <summary>
            /// Price of the PricingPhase
            /// </summary>
            public Price Price { get; }

            /// <summary>
            /// Indicates how the pricing phase is charged for finiteRecurring pricing phases
            /// </summary>
            public OfferPaymentMode OfferPaymentMode { get; }

            [JsonConstructor]
            internal PricingPhase(
                [JsonProperty("billingPeriod")] Period billingPeriod,
                [JsonProperty("recurrenceMode")] RecurrenceMode recurrenceMode,
                [JsonProperty("billingCycleCount")] int? billingCycleCount,
                [JsonProperty("price")] Price price,
                [JsonProperty("offerPaymentMode")] OfferPaymentMode offerPaymentMode)
            {
                BillingPeriod = billingPeriod;
                RecurrenceMode = recurrenceMode;
                BillingCycleCount = billingCycleCount;
                Price = price;
                OfferPaymentMode = offerPaymentMode;
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

        /// <summary>
        /// The number of period units: day, week, month, year, unknown
        /// </summary>
        public sealed class Period
        {
            /// <summary>
            /// The increment of time that a subscription period is specified in
            /// </summary>
            public PeriodUnit Unit { get; }

            /// <summary>
            /// The increment of time that a subscription period is specified in
            /// </summary>
            public int Value { get; }

            /// <summary>
            /// Specified in ISO 8601 format. For example, P1W equates to one week,
            /// P1M equates to one month, P3M equates to three months, P6M equates to six months,
            /// and P1Y equates to one year
            /// </summary>
            public string ISO8601 { get; }

            [JsonConstructor]
            internal Period(
                [JsonProperty("unit")] PeriodUnit unit,
                [JsonProperty("value")] int value,
                [JsonProperty("iso8601")] string iso8601)
            {
                Unit = unit;
                Value = value;
                ISO8601 = iso8601;
            }

            public override string ToString()
            {
                return $"{nameof(Unit)}: {Unit}\n" +
                    $"{nameof(Value)}: {Value}\n" +
                    $"{nameof(ISO8601)}: {ISO8601}\n";
            }
        }

        /// <summary>
        /// Period units for a subscription period
        /// </summary>
        public enum PeriodUnit
        {
            DAY = 0,
            WEEK = 1,
            MONTH = 2,
            YEAR = 3,
            UNKNOWN = 4
        }

        /// <summary>
        /// Recurrence mode for a pricing phase
        /// </summary>
        public enum RecurrenceMode
        {
            /// <summary>
            /// Pricing phase repeats infinitely until cancellation
            /// </summary>
            INFINITE_RECURRING = 1,
            /// <summary>
            /// Pricing phase repeats for a fixed number of billing periods
            /// </summary>
            FINITE_RECURRING = 2,
            /// <summary>
            /// Pricing phase does not repeat
            /// </summary>
            NON_RECURRING = 3,
            UNKNOWN = 4,
        }

        /// <summary>
        /// Payment mode for offer pricing phases. Google Play only.
        /// </summary>
        public enum OfferPaymentMode
        {
            /// <summary>
            /// Subscribers don't pay until the specified period ends
            /// </summary>
            FREE_TRIAL = 0,
            /// <summary>
            /// Subscribers pay up front for a specified period
            /// </summary>
            SINGLE_PAYMENT = 1,
            /// <summary>
            /// Subscribers pay a discounted amount for a specified number of periods
            /// </summary>
            DISCOUNTED_RECURRING_PAYMENT = 2,
            UNKNOWN = 3,
        }

        /// <summary>
        /// Contains all the details associated with a Price
        /// </summary>
        public sealed class Price
        {
            /// <summary>
            /// Formatted price of the item, including its currency sign. For example $3.00
            /// </summary>
            public string Formatted { get; }

            /// <summary>
            /// Price in micro-units, where 1,000,000 micro-units equal one unit of the currency.
            /// For example, if price is "€7.99", price_amount_micros is 7,990,000. This value represents
            /// the localized, rounded price for a particular currency.
            /// </summary>
            public int AmountMicros { get; }

            /// <summary>
            /// Returns ISO 4217 currency code for price and original price.
            /// For example, if price is specified in British pounds sterling, price_currency_code is "GBP".
            /// If currency code cannot be determined, currency symbol is returned.
            /// </summary>
            public string CurrencyCode { get; }

            [JsonConstructor]
            internal Price(
                [JsonProperty("formatted")] string formatted,
                [JsonProperty("amountMicros")] int amountMicros,
                [JsonProperty("currencyCode")] string currencyCode)
            {
                Formatted = formatted;
                AmountMicros = amountMicros;
                CurrencyCode = currencyCode;
            }

            public override string ToString()
            {
                return $"{nameof(Formatted)}: {Formatted}\n" +
                       $"{nameof(AmountMicros)}: {AmountMicros}\n" +
                       $"{nameof(CurrencyCode)}: {CurrencyCode}\n";
            }
        }

        /// <summary>
        /// Type containing information of installment subscriptions. Currently only supported in Google Play.
        /// </summary>
        public sealed class InstallmentsInfo
        {
            /// <summary>
            /// Number of payments the customer commits to in order to purchase the subscription.
            /// </summary>
            public int CommitmentPaymentsCount { get; }

            /// <summary>
            /// After the commitment payments are complete, the number of payments the user commits to upon a renewal.
            /// </summary>
            public int RenewalCommitmentPaymentsCount { get; }

            [JsonConstructor]
            internal InstallmentsInfo(
                [JsonProperty("commitmentPaymentsCount")] int commitmentPaymentsCount,
                [JsonProperty("renewalCommitmentPaymentsCount")] int renewalCommitmentPaymentsCount)
            {
                CommitmentPaymentsCount = commitmentPaymentsCount;
                RenewalCommitmentPaymentsCount = renewalCommitmentPaymentsCount;
            }

            public override string ToString()
            {
                return $"{nameof(CommitmentPaymentsCount)}: {CommitmentPaymentsCount}\n" +
                       $"{nameof(RenewalCommitmentPaymentsCount)}: {RenewalCommitmentPaymentsCount}\n";
            }
        }
    }
}

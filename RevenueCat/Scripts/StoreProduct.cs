using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace RevenueCat
{
    /// <summary>
    /// Type that abstracts products from App Store, Google Play and Amazon into a single interface.
    /// </summary>
    public sealed class StoreProduct
    {
        /// <summary>
        /// Title of the product.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Product Id.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Description of the product.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Price of the product in the local currency.
        /// Contains the price value of DefaultOption for Google Play.
        /// </summary>
        public float Price { get; }

        /// <summary>
        /// Formatted price of the item, including its currency sign.
        /// Contains the formatted price value of DefaultOption for Google Play.
        /// </summary>
        public string PriceString { get; }

        /// <summary>
        /// Null for non-subscription products. The price of the StoreProduct in a weekly recurrence.
        /// This means that, for example, if the period is monthly, the price will be
        /// divided by 4. Note that this value may be an approximation. For Google subscriptions,
        /// this value will use the basePlan to calculate the value.
        /// </summary>
        public float? PricePerWeek { get; }

        /// <summary>
        /// Null for non-subscription products. The price of the StoreProduct in a monthly recurrence.
        /// This means that, for example, if the period is yearly, the price will be
        /// divided by 12. Note that this value may be an approximation. For Google subscriptions,
        /// this value will use the basePlan to calculate the value.
        /// </summary>
        public float? PricePerMonth { get; }

        /// <summary>
        /// Null for non-subscription products. The price of the StoreProduct in a yearly recurrence.
        /// This means that, for example, if the period is monthly, the price will be
        /// multiplied by 12. Note that this value may be an approximation. For Google subscriptions,
        /// this value will use the basePlan to calculate the value.
        /// </summary>
        public float? PricePerYear { get; }

        /// <summary>
        /// Null for non-subscription products. The price of the StoreProduct formatted for the current
        /// locale in a weekly recurrence. This means that, for example, if the period is monthly,
        /// the price will be divided by 4. It uses a currency formatter to format the price in the
        /// given locale. Note that this value may be an approximation. For Google subscriptions,
        /// this value will use the basePlan to calculate the value.
        /// </summary>
        public string PricePerWeekString { get; }

        /// <summary>
        /// Null for non-subscription products. The price of the StoreProduct formatted for the current
        /// locale in a monthly recurrence. This means that, for example, if the period is yearly,
        /// the price will be divided by 12. It uses a currency formatter to format the price in the
        /// given locale. Note that this value may be an approximation. For Google subscriptions,
        /// this value will use the basePlan to calculate the value.
        /// </summary>
        public string PricePerMonthString { get; }

        /// <summary>
        /// Null for non-subscription products. The price of the StoreProduct formatted for the current
        /// locale in a yearly recurrence. This means that, for example, if the period is monthly,
        /// the price will be multiplied by 12. It uses a currency formatter to format the price in the
        /// given locale. Note that this value may be an approximation. For Google subscriptions,
        /// this value will use the basePlan to calculate the value.
        /// </summary>
        public string PricePerYearString { get; }

        /// <summary>
        /// Currency code for price and original price.
        /// Contains the currency code of DefaultOption for Google Play.
        /// </summary>
        [CanBeNull]
        public string CurrencyCode { get; }

        /// <summary>
        /// Introductory price of the product. Null if no introductory price is available.
        /// It contains the free trial if available and user is eligible for it.
        /// Otherwise, it contains the introductory price of the product if the user is eligible for it.
        /// This will be null for non-subscription products.
        /// </summary>
        public IntroductoryPrice IntroductoryPrice { get; }

        /// <summary>
        /// Product category of the product.
        /// </summary>
        public ProductCategory ProductCategory { get; }

        /// <summary>
        /// Default subscription option for a product. Google Play only.
        /// </summary>
        [CanBeNull]
        public SubscriptionOption DefaultOption { get; }

        /// <summary>
        /// Collection of subscription options for a product. Google Play only.
        /// </summary>
        [CanBeNull]
        public IReadOnlyList<SubscriptionOption> SubscriptionOptions { get; }

        /// <summary>
        /// Offering context this package belongs to.
        /// Null if not using offerings or if fetched directly from store via GetProducts.
        /// </summary>
        [CanBeNull]
        public PresentedOfferingContext PresentedOfferingContext { get; }

        [CanBeNull]
        [JsonIgnore]
        [Obsolete("Deprecated, use PresentedOfferingContext instead.", false)]
        public string PresentedOfferingIdentifier => PresentedOfferingContext?.OfferingIdentifier;

        /// <summary>
        /// Collection of iOS promotional offers for a product. Null for Android and Amazon.
        /// </summary>
        [CanBeNull]
        public IReadOnlyList<Discount> Discounts { get; }

        /// <summary>
        /// Subscription period, specified in ISO 8601 format. For example,
        /// P1W equates to one week, P1M equates to one month,
        /// P3M equates to three months, P6M equates to six months,
        /// and P1Y equates to one year.
        /// Note: Not available for Amazon.
        /// </summary>
        [CanBeNull]
        public string SubscriptionPeriod { get; }

        [JsonConstructor]
        internal StoreProduct(
            [JsonProperty("title")] string title,
            [JsonProperty("identifier")] string identifier,
            [JsonProperty("description")] string description,
            [JsonProperty("price")] float price,
            [JsonProperty("priceString")] string priceString,
            [JsonProperty("pricePerWeek")] float? pricePerWeek,
            [JsonProperty("pricePerMonth")] float? pricePerMonth,
            [JsonProperty("pricePerYear")] float? pricePerYear,
            [JsonProperty("pricePerWeekString")] string pricePerWeekString,
            [JsonProperty("pricePerMonthString")] string pricePerMonthString,
            [JsonProperty("pricePerYearString")] string pricePerYearString,
            [JsonProperty("currencyCode")] string currencyCode,
            [JsonProperty("productCategory")] ProductCategory productCategory,
            [JsonProperty("defaultOption")] SubscriptionOption defaultOption,
            [JsonProperty("subscriptionOptions")] List<SubscriptionOption> subscriptionOptions,
            [JsonProperty("presentedOfferingContext")] PresentedOfferingContext presentedOfferingContext,
            [JsonProperty("introductoryPrice")] IntroductoryPrice introductoryPrice,
            [JsonProperty("discounts")] List<Discount> discounts,
            [JsonProperty("subscriptionPeriod")] string subscriptionPeriod)
        {
            Title = title;
            Identifier = identifier;
            Description = description;
            Price = price;
            PriceString = priceString;
            PricePerWeek = pricePerWeek;
            PricePerMonth = pricePerMonth;
            PricePerYear = pricePerYear;
            PricePerWeekString = pricePerWeekString;
            PricePerMonthString = pricePerMonthString;
            PricePerYearString = pricePerYearString;
            CurrencyCode = currencyCode;
            ProductCategory = productCategory;
            DefaultOption = defaultOption;
            SubscriptionOptions = subscriptionOptions;
            PresentedOfferingContext = presentedOfferingContext;
            IntroductoryPrice = introductoryPrice;
            Discounts = discounts;
            SubscriptionPeriod = subscriptionPeriod;
        }

        public override string ToString()
        {
            return $"{nameof(Title)}: {Title}\n" +
                   $"{nameof(Identifier)}: {Identifier}\n" +
                   $"{nameof(Description)}: {Description}\n" +
                   $"{nameof(Price)}: {Price}\n" +
                   $"{nameof(PriceString)}: {PriceString}\n" +
                   $"{nameof(PricePerWeek)}: {PricePerWeek}\n" +
                   $"{nameof(PricePerMonth)}: {PricePerMonth}\n" +
                   $"{nameof(PricePerYear)}: {PricePerYear}\n" +
                   $"{nameof(PricePerWeekString)}: {PricePerWeekString}\n" +
                   $"{nameof(PricePerMonthString)}: {PricePerMonthString}\n" +
                   $"{nameof(PricePerYearString)}: {PricePerYearString}\n" +
                   $"{nameof(CurrencyCode)}: {CurrencyCode}\n" +
                   $"{nameof(ProductCategory)}: {ProductCategory}\n" +
#pragma warning disable CS0618 // Type or member is obsolete
                   $"{nameof(PresentedOfferingIdentifier)}: {PresentedOfferingIdentifier}\n" +
#pragma warning restore CS0618 // Type or member is obsolete
                   $"{nameof(PresentedOfferingContext)}: {PresentedOfferingContext}\n" +
                   $"{nameof(DefaultOption)}: {DefaultOption}\n" +
                   $"{nameof(SubscriptionOptions)}: {SubscriptionOptions}\n" +
                   $"{nameof(IntroductoryPrice)}: {IntroductoryPrice}\n" +
                   $"{nameof(Discounts)}: {Discounts}\n" +
                   $"{nameof(SubscriptionPeriod)}: {SubscriptionPeriod}";
        }
    }
}
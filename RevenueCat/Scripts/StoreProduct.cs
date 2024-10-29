using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RevenueCat.SimpleJSON;

public partial class Purchases
{
    /// <summary>
    /// Type that abstracts products from App Store, Google Play and Amazon into a single interface.
    /// </summary>
    public class StoreProduct
    {
        /// <summary>
        /// Title of the product.
        /// </summary>
        /// <returns></returns>
        public readonly string Title;

        /// <summary>
        /// Product Id.
        /// </summary>
        /// <returns></returns>
        public readonly string Identifier;

        /// <summary>
        /// Description of the product.
        /// </summary>
        /// <returns></returns> 
        public readonly string Description;

        /// <summary>
        /// Price of the product in the local currency.
        /// Contains the price value of DefaultOption for Google Play.
        /// </summary>
        /// <returns></returns>
        public readonly float Price;

        /// <summary>
        /// Formatted price of the item, including its currency sign.
        /// Contains the formatted price value of DefaultOption for Google Play.
        /// </summary>
        /// <returns></returns>
        public readonly string PriceString;

        /// <summary>
        /// Currency code for price and original price.
        /// Contains the currency code of DefaultOption for Google Play.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public readonly string CurrencyCode;
        
        /// <summary>
        /// Introductory price of the product. Null if no introductory price is available.
        /// It contains the free trial if available and user is eligible for it.
        /// Otherwise, it contains the introductory price of the product if the user is eligible for it.
        /// This will be null for non-subscription products.
        /// </summary>
        /// <returns></returns>
        public IntroductoryPrice IntroductoryPrice;

        /// <summary>
        /// Product category of the product.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public readonly ProductCategory ProductCategory;

        /// <summary>
        /// Default subscription option for a product. Google Play only.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public readonly SubscriptionOption DefaultOption;

        /// <summary>
        /// Collection of subscription options for a product. Google Play only.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public readonly SubscriptionOption[] SubscriptionOptions;

        /// <summary>
        /// Offering context this package belongs to.
        /// Null if not using offerings or if fetched directly from store via GetProducts.
        /// </summary>
        [CanBeNull] public readonly PresentedOfferingContext PresentedOfferingContext;

        [Obsolete("Deprecated, use PresentedOfferingContext instead.", false)]
        [CanBeNull] public readonly string PresentedOfferingIdentifier;

        /// <summary>
        /// Collection of iOS promotional offers for a product. Null for Android and Amazon.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public readonly Discount[] Discounts;
        
        /// <summary>
        /// Subscription period, specified in ISO 8601 format. For example,
        /// P1W equates to one week, P1M equates to one month,
        /// P3M equates to three months, P6M equates to six months,
        /// and P1Y equates to one year.
        /// Note: Not available for Amazon.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public readonly string SubscriptionPeriod;
            
        public StoreProduct(JSONNode response)
        {
            Title = response["title"];
            Identifier = response["identifier"];
            Description = response["description"];
            Price = response["price"];
            PriceString = response["priceString"];
            CurrencyCode = response["currencyCode"];
            SubscriptionPeriod = response["subscriptionPeriod"];
            var introPriceJsonNode = response["introPrice"];
            if (introPriceJsonNode != null && !introPriceJsonNode.IsNull)
            {
                IntroductoryPrice = new IntroductoryPrice(introPriceJsonNode);
            }

            var presentedOfferingContexNode = response["presentedOfferingContext"];
            if (presentedOfferingContexNode != null && !presentedOfferingContexNode.IsNull) {
                PresentedOfferingContext = new PresentedOfferingContext(presentedOfferingContexNode);
                PresentedOfferingIdentifier = PresentedOfferingContext.OfferingIdentifier;
            }

            if (!Enum.TryParse(response["productCategory"].Value, out ProductCategory))
            {
                ProductCategory = ProductCategory.UNKNOWN;
            }
            var defaultOptionJsonNode = response["defaultOption"];
            if (defaultOptionJsonNode != null && !defaultOptionJsonNode.IsNull)
            {
                DefaultOption = new SubscriptionOption(defaultOptionJsonNode);
            }

            var subscriptionOptionsResponse = response["subscriptionOptions"];
            if (subscriptionOptionsResponse == null)
            {
                SubscriptionOptions = null;
            }
            else
            {
                var subscriptionOptionsTemporaryList = new List<SubscriptionOption>();
                foreach (var subscriptionOptionResponse in subscriptionOptionsResponse)
                {
                    subscriptionOptionsTemporaryList.Add(new SubscriptionOption(subscriptionOptionResponse));
                }
                SubscriptionOptions = subscriptionOptionsTemporaryList.ToArray();
            }

            var discountsResponse = response["discounts"];
            if (discountsResponse == null)
            {
                Discounts = null;
            }
            else
            {
                var temporaryList = new List<Discount>();
                foreach (var discountResponse in discountsResponse)
                {
                    temporaryList.Add(new Discount(discountResponse));
                }
                Discounts = temporaryList.ToArray();
            }
        }

        public override string ToString()
        {
            return $"{nameof(Title)}: {Title}\n" +
                   $"{nameof(Identifier)}: {Identifier}\n" +
                   $"{nameof(Description)}: {Description}\n" +
                   $"{nameof(Price)}: {Price}\n" +
                   $"{nameof(PriceString)}: {PriceString}\n" +
                   $"{nameof(CurrencyCode)}: {CurrencyCode}\n" +
				   $"{nameof(ProductCategory)}: {ProductCategory}\n" +
                   $"{nameof(PresentedOfferingIdentifier)}: {PresentedOfferingIdentifier}\n" +
                   $"{DefaultOption}\n" +
                   $"{SubscriptionOptions}\n" +
                   $"{IntroductoryPrice}\n" +
                   $"{nameof(Discounts)}: {Discounts}\n" +
                   $"{nameof(SubscriptionPeriod)}: {SubscriptionPeriod}";
        }
    }
}
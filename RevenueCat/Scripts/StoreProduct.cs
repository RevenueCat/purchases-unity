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
        public readonly string Title;
        public readonly string Identifier;
        public readonly string Description;
        public readonly float Price;
        public readonly string PriceString;
        [CanBeNull] public readonly string CurrencyCode;
        public IntroductoryPrice IntroductoryPrice;

        /// <summary>
        /// Collection of iOS promotional offers for a product. Null for Android.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public readonly Discount[] Discounts;
            
        public StoreProduct(JSONNode response)
        {
            Title = response["title"];
            Identifier = response["identifier"];
            Description = response["description"];
            Price = response["price"];
            PriceString = response["priceString"];
            CurrencyCode = response["currencyCode"];
            var introPriceJsonNode = response["introPrice"];
            if (introPriceJsonNode != null && !introPriceJsonNode.IsNull)
            {
                IntroductoryPrice = new IntroductoryPrice(introPriceJsonNode);
            }
            var discountsResponse = response["discounts"];
            if (discountsResponse == null)
            {
                Discounts = null;
                return;
            }
            var temporaryList = new List<Discount>();
            foreach (var discountResponse in discountsResponse)
            {
                temporaryList.Add(new Discount(discountResponse));
            }
            Discounts = temporaryList.ToArray();
        }

        public override string ToString()
        {
            return $"{nameof(Title)}: {Title}\n" +
                   $"{nameof(Identifier)}: {Identifier}\n" +
                   $"{nameof(Description)}: {Description}\n" +
                   $"{nameof(Price)}: {Price}\n" +
                   $"{nameof(PriceString)}: {PriceString}\n" +
                   $"{nameof(CurrencyCode)}: {CurrencyCode}\n" +
                   $"{IntroductoryPrice}\n" +
                   $"{nameof(Discounts)}: {Discounts}";
        }
    }
}
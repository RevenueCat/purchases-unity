using NUnit.Framework;
using RevenueCat.SimpleJSON;

namespace RevenueCat.Tests
{
    public class JsonModelTests
    {
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
    }
}

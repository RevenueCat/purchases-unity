using UnityEngine;

namespace DefaultNamespace
{
    public class DiscountAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.Discount discount = new Purchases.Discount(null);
            string identifier = discount.Identifier;
            float price = discount.Price;
            string priceString = discount.PriceString;
            int cycles = discount.Cycles;
            string period = discount.Period;
            string periodUnit = discount.PeriodUnit;
            int periodNumberOfUnits = discount.PeriodNumberOfUnits;
        }
    }
}
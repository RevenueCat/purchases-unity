using UnityEngine;

namespace DefaultNamespace
{
    public class DiscountAPITests : MonoBehaviour
    {
        private void Start()
        {
            Purchases.Discount discount = new Purchases.Discount(null);
            string identifier = discount.identifier;
            float price = discount.price;
            string priceString = discount.priceString;
            int cycles = discount.cycles;
            string period = discount.period;
            string periodUnit = discount.periodUnit;
            int periodNumberOfUnits = discount.periodNumberOfUnits;
        }
    }
}
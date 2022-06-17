using UnityEngine;

namespace DefaultNamespace
{
    public class DiscountAPITests : MonoBehaviour
    {
        private void Start()
        {
            // TODO: 
            // properties here should be Uppercase
            // also, they're currently read-write, but should be read-only
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
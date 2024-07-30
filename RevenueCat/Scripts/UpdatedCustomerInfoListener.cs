using UnityEngine;

public partial class Purchases
{
    public interface IUpdatedCustomerInfoListener
    {
        public void CustomerInfoReceived(CustomerInfo customerInfo);
    }
}

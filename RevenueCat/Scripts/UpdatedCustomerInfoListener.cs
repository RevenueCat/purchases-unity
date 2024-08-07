using UnityEngine;

public partial class Purchases
{
    public interface IUpdatedCustomerInfoListener
    {
        public void CustomerInfoReceived(CustomerInfo customerInfo);
    }
    
    public abstract class UpdatedCustomerInfoListener : MonoBehaviour, IUpdatedCustomerInfoListener
    {
        public abstract void CustomerInfoReceived(CustomerInfo customerInfo);
    }
}

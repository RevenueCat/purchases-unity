using UnityEngine;

public partial class Purchases
{
    public abstract class UpdatedCustomerInfoListener : MonoBehaviour
    {
        public abstract void CustomerInfoReceived(CustomerInfo customerInfo);
    }
}
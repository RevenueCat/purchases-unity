using UnityEngine;

public partial class Purchases
{
    public abstract class UpdatedPurchaserInfoListener : MonoBehaviour
    {
        public abstract void PurchaserInfoReceived(PurchaserInfo purchaserInfo);
    }
}
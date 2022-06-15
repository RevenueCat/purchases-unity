using UnityEngine;

public class Main : MonoBehaviour
{
    
    // Use this for initialization
    private void Start()
    {
        var purchases = GetComponent<Purchases>();
        purchases.SetDebugLogsEnabled(true);
        
        purchases.GetCustomerInfo((info, error) =>
        {
            
        });
    }

}

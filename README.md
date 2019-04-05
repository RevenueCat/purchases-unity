# Purchases.framework for Unity
The Purchases SDK provided by RevenueCat allows you to implement subscriptions in your Unity app easily.

## 1. Add the Purchases Unity package
Get the latest version of [Purchases.unitypackage](https://github.com/RevenueCat/purchases-unity/releases) from Github. Add it to your Unity project.

## 2. Create a GameObject with the Purchases behavior
The Purchases package will include a MonoBehaviour called Purchases. This will be your access point to RevenueCat from inside Unity. It should be instantiated once and kept as a singleton. You can use properties to configure your API Key, app user ID (if you have one), and product identifiers you want to fetch.

![](https://files.readme.io/9c094e8-Screen_Shot_2018-05-31_at_11.24.09_AM.png)
*The Purchases behaviour is configured with your RevenueCat API key and the product identifiers you want to fetch.*

## 3. Subclass Purchases.Listener MonoBehaviour
The Purchases behavior takes one additional parameter, a GameObject with a Purchases.Listener component. This will be where you handle purchase events, and updated subscriber information from RevenueCat. Here is a simple example:

```C#
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PurchasesListener : Purchases.UpdatedPurchaserInfoListener
{
    void ButtonClicked(string product)
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.MakePurchase(product, (productIdentifier, purchaserInfo, userCancelled, error) =>
        {
            if (!userCancelled)
            {
                if (error != null)
                {
                    LogError(error);
                }
                else
                {
                    DisplayPurchaserInfo(purchaserInfo);
                }
            } else
            {
                Debug.Log("Subtester: User cancelled, don't show an error");
            }
        });
    }
    
    void DoOtherStuff()
    {
        var purchases = GetComponent<Purchases>();
        var data = new AdjustData
        {
            adid = "test",
            network = "network",
            adgroup = "adgroup",
            campaign = "campaign",
            creative = "creative",
            clickLabel = "clickLabel",
            trackerName = "trackerName",
            trackerToken = "trackerToken"
        };

        purchases.AddAttributionData(JsonUtility.ToJson(data), Purchases.AttributionNetwork.ADJUST);
        
        purchases.GetPurchaserInfo((info, error) =>
        {
            Debug.Log("purchaser info " + info.ActiveSubscriptions);
            if (error != null) {
                LogError(error);
            }
        });
        purchases.GetProducts(new []{ "onemonth_freetrial", "annual_freetrial" }, (products, error) =>
        {
            Debug.Log("getProducts " + products);
            if (error != null) {
                LogError(error);
            }
        });
        
        Debug.Log("user ID " + purchases.GetAppUserId());
    }

    void RestoreClicked()
    {
        var purchases = GetComponent<Purchases>();
        purchases.RestoreTransactions((purchaserInfo, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                DisplayPurchaserInfo(purchaserInfo);
            }
        });
    }

    
    public override void PurchaserInfoReceived(Purchases.PurchaserInfo purchaserInfo)
    {
        DisplayPurchaserInfo(purchaserInfo);
    }
    
    private void logError(Purchases.Error error)
    {
        Debug.Log("Subtester: " + JsonUtility.ToJson(error));
    }


    private void DisplayPurchaserInfo(Purchases.PurchaserInfo purchaserInfo)
    {
        // Show purchaser info on screen
    }
}

```

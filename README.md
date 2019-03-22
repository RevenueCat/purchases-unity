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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PurchasesListener : Purchases.Listener
{
    void SendAttribution()
    {
        Purchases purchases = GetComponent<Purchases>();
        Purchases.AdjustData data = new Purchases.AdjustData();

        data.adid = "test";
        data.network = "network";
        data.adgroup = "adgroup";
        data.campaign = "campaign";
        data.creative = "creative";
        data.clickLabel = "clickLabel";
        data.trackerName = "trackerName";
        data.trackerToken = "trackerToken";

        purchases.AddAdjustAttributionData(data);
    }

    void ButtonClicked(string product)
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.MakePurchase(product);
    }

    void RestoreClicked()
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.RestoreTransactions();
    }

    public override void PurchaseSucceeded(string productIdentifier, Purchases.PurchaserInfo purchaserInfo)
    {
        DisplayPurchaserInfo(purchaserInfo);
    }

    public override void PurchaseFailed(string productIdentifier, Purchases.Error error, bool userCanceled)
    {
        if (userCanceled)
        {
            Debug.Log("Subtester: User canceled, don't show an error");
        }
        else
        {
            logError(error);
        }
    }

    public override void PurchaserInfoReceived(Purchases.PurchaserInfo purchaserInfo)
    {
        DisplayPurchaserInfo(purchaserInfo);
    }

    public override void PurchaserInfoReceiveFailed(Purchases.Error error)
    {
        logError(error);
    }

    public override void RestoredPurchases(Purchases.PurchaserInfo purchaserInfo)
    {
        Debug.Log("Subtester: Restore Succeeded");
        DisplayPurchaserInfo(purchaserInfo);
    }

    public override void RestorePurchasesFailed(Purchases.Error error)
    {
        Debug.Log("Subtester: Restore Failed");
        logError(error);
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

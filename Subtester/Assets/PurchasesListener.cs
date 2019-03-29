using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PurchasesListener : Purchases.Listener
{

    public RectTransform parentPanel;
    public GameObject buttonPrefab;
    public Text purchaserInfoLabel;

    // Use this for initialization
    void Start()
    {
        CreateButton("Restore Purchases", () => RestoreClicked(), 100);

        CreateButton("Switch Username", () => SwitchUser(), 200);

        CreateButton("Send Attribution", () => SendAttribution(), 300);

        Purchases purchases = GetComponent<Purchases>();
    }

    private void CreateButton(string label, UnityAction action, float yPos)
    {
        GameObject button = (GameObject)Instantiate(buttonPrefab);

        button.transform.SetParent(parentPanel, false);
        button.transform.position = new Vector2(parentPanel.transform.position.x, yPos);

        Button tempButton = button.GetComponent<Button>();

        Text textComponent = tempButton.GetComponentsInChildren<Text>()[0];
        textComponent.text = label;

        tempButton.onClick.AddListener(action);
    }

    private void SwitchUser()
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.Reset();
    }

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

        purchases.AddAttributionData(JsonUtility.ToJson(data), Purchases.AttributionNetwork.ADJUST);
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

    // Update is called once per frame
    void Update()
    {

    }

    public override void ProductsReceived(List<Purchases.Product> products)
    {
        int yOffset = 0;
        foreach (Purchases.Product p in products)
        {
            String label = p.identifier + " " + p.priceString;
            CreateButton(label, () => ButtonClicked(p.identifier), 500 + yOffset);
            yOffset += 70;
        }
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

    public override void AliasCreated(Purchases.Error error)
    {
        if (error == null)
        {
            Debug.Log("Alias created.");
        }
        else
        {
            Debug.Log("Alias failed.");
            logError(error);
        }
    }

    private void logError(Purchases.Error error)
    {
        Debug.Log("Subtester: " + JsonUtility.ToJson(error));
    }


    private void DisplayPurchaserInfo(Purchases.PurchaserInfo purchaserInfo)
    {
        string text = "";
        foreach (KeyValuePair<string, DateTime> entry in purchaserInfo.AllExpirationDates)
        {
            string active = (DateTime.UtcNow < entry.Value) ? "subscribed" : "expired";
            text += entry.Key + " " + entry.Value + " " + active + "\n";
        }
        text += purchaserInfo.LatestExpirationDate;

        purchaserInfoLabel.text = text;
    }
}

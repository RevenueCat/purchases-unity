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
        purchases.Setup("jerry7");
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

    public override void PurchaseCompleted(string productIdentifier, Purchases.Error error, Purchases.PurchaserInfo purchaserInfo, bool userCanceled)
    {
        if (userCanceled)
        {
            Debug.Log("User canceled, don't show an error");
        }

        if (purchaserInfo != null)
        {
            DisplayPurchaserInfo(purchaserInfo);
        }
    }

    public override void PurchaserInfoReceived(Purchases.PurchaserInfo purchaserInfo)
    {
        DisplayPurchaserInfo(purchaserInfo);
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

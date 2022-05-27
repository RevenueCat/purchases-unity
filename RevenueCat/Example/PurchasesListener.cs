using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PurchasesListener : Purchases.UpdatedCustomerInfoListener
{

    public RectTransform parentPanel;
    public GameObject buttonPrefab;
    public Text customerInfoLabel;

    // Use this for initialization
    private void Start()
    {
        CreateButton("Restore Purchases", RestoreClicked, 100);

        CreateButton("Switch Username", SwitchUser, 200);

        CreateButton("Do Other Stuff", DoOtherStuff, 300);

        var purchases = GetComponent<Purchases>();
        purchases.SetDebugLogsEnabled(true);
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                Debug.Log("offerings received " + offerings.ToString());
                var yOffset = 0;

                foreach (var package in offerings.Current.AvailablePackages)
                {
                    Debug.Log("Package " + package);
                    if (package == null) continue;
                    var label = package.PackageType + " " + package.Product.priceString;
                    CreateButton(label, () => ButtonClicked(package), 500 + yOffset);
                    yOffset += 70;
                }
            }
        });
    }

    private void CreateButton(string label, UnityAction action, float yPos)
    {
        var button = Instantiate(buttonPrefab, parentPanel, false);

        button.transform.position = new Vector2(parentPanel.transform.position.x, yPos);

        var tempButton = button.GetComponent<Button>();

        var textComponent = tempButton.GetComponentsInChildren<Text>()[0];
        textComponent.text = label;

        tempButton.onClick.AddListener(action);
    }

    private void SwitchUser()
    {
        var purchases = GetComponent<Purchases>();
        purchases.LogIn("newUser", (customerInfo, created, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                DisplayCustomerInfo(customerInfo);
            }
        });
    }

    [Serializable]
    private class AdjustData
    {
        // ReSharper disable NotAccessedField.Local
        public string adid;
        public string network;
        public string adgroup;
        public string campaign;
        public string creative;
        public string clickLabel;
        public string trackerName;
        public string trackerToken;
    }

    private void DoOtherStuff()
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
        purchases.SetAutomaticAppleSearchAdsAttributionCollection(true);
        purchases.AddAttributionData(JsonUtility.ToJson(data), Purchases.AttributionNetwork.ADJUST, null);

        purchases.GetCustomerInfo((info, error) =>
        {
            Debug.Log("customer info " + info.ActiveSubscriptions);
            if (error != null)
            {
                LogError(error);
            }
        });
        purchases.GetProducts(new[] { "onemonth_freetrial", "annual_freetrial" }, (products, error) =>
        {
            Debug.Log("getProducts " + products);
            if (error != null)
            {
                LogError(error);
            }
        });

        purchases.SyncPurchases();
        purchases.SetFinishTransactions(false);
        Debug.Log("user ID " + purchases.GetAppUserId());
        Debug.Log("user is anonymous " + purchases.IsAnonymous());
    }

    private void ButtonClicked(Purchases.Package package)
    {
        var purchases = GetComponent<Purchases>();
        purchases.PurchasePackage(package, (productIdentifier, customerInfo, userCancelled, error) =>
        {
            if (!userCancelled)
            {
                if (error != null)
                {
                    LogError(error);
                }
                else
                {
                    DisplayCustomerInfo(customerInfo);
                }
            }
            else
            {
                Debug.Log("Subtester: User cancelled, don't show an error");
            }
        });
    }

    void RestoreClicked()
    {
        var purchases = GetComponent<Purchases>();
        purchases.RestoreTransactions((customerInfo, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                DisplayCustomerInfo(customerInfo);
            }
        });
    }

    public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo)
    {
        Debug.Log(string.Format("customer info received {0}", customerInfo.ActiveSubscriptions));

        DisplayCustomerInfo(customerInfo);
    }

    private void LogError(Purchases.Error error)
    {
        Debug.Log("Subtester: " + JsonUtility.ToJson(error));
    }

    private void DisplayCustomerInfo(Purchases.CustomerInfo customerInfo)
    {
        var text = "";
        foreach (var entry in customerInfo.Entitlements.All)
        {
            var entitlement = entry.Value;
            var active = entitlement.IsActive ? "subscribed" : "expired";
            text += entitlement.Identifier + " " + active + "\n";
        }
        text += customerInfo.LatestExpirationDate;

        customerInfoLabel.text = text;
    }

}

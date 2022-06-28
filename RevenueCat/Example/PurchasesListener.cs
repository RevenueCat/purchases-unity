using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PurchasesListener : Purchases.UpdatedCustomerInfoListener
{

    public RectTransform parentPanel;
    public GameObject buttonPrefab;
    public Text customerInfoLabel;
    private int minYOffestForButtons = 200; // values lower than these don't work great with devices
                                            // with safe areas on iOS
    private int minXOffsetForButtons = 350;
    
    private int xPaddingForButtons = 50;
    private int yPaddingForButtons = 30;
    
    private int maxButtonsPerColumn = 7;
    private int currentButtons = 0;

    // Use this for initialization
    private void Start()
    {
        CreateButton("Get Customer Info", GetCustomerInfo);
        CreateButton("Restore Purchases", RestoreClicked);
        CreateButton("Switch Username", SwitchUser);
        CreateButton("Do Other Stuff", DoOtherStuff);

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
                    var label = package.PackageType + " " + package.StoreProduct.PriceString;
                    CreateButton(label, () => ButtonClicked(package));
                    yOffset += yPaddingForButtons;
                }
            }
        });
    }

    private void CreateButton(string label, UnityAction action)
    {
        var button = Instantiate(buttonPrefab, parentPanel, false);
        var buttonTransform = (RectTransform)buttonPrefab.transform;
        var height = buttonTransform.rect.height;
        var width = buttonTransform.rect.width;

        var xPos = (currentButtons / maxButtonsPerColumn) * (width + xPaddingForButtons) + minXOffsetForButtons;
        var yPos = (currentButtons % maxButtonsPerColumn) * (height + yPaddingForButtons) + minYOffestForButtons;

        button.transform.position = new Vector2(xPos, yPos);

        var tempButton = button.GetComponent<Button>();

        var textComponent = tempButton.GetComponentsInChildren<Text>()[0];
        textComponent.text = label;

        tempButton.onClick.AddListener(action);
        currentButtons++;
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
        purchases.SetAdjustID(null);
        
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

    void GetCustomerInfo()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetCustomerInfo((customerInfo, error) =>
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
    void RestoreClicked()
    {
        var purchases = GetComponent<Purchases>();
        purchases.RestorePurchases((customerInfo, error) =>
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
        customerInfoLabel.text = customerInfo.ToString();
    }

}

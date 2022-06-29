using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PurchasesListener : Purchases.UpdatedCustomerInfoListener
{
    public RectTransform parentPanel;
    public GameObject buttonPrefab;
    public Text infoLabel;

    private int minYOffsetForButtons = 40; // values lower than these don't work great with devices
                                           // with safe areas on iOS
    
    private int minXOffsetForButtons = 20;

    private int xPaddingForButtons = 10;
    private int yPaddingForButtons = 5;

    private int maxButtonsPerRow = 2;
    private int currentButtons = 0;

    // Use this for initialization
    private void Start()
    {
        CreateButton("Get Customer Info", GetCustomerInfo);
        CreateButton("Get Offerings", GetOfferings);
        CreateButton("Sync Purchases", SyncPurchases);
        CreateButton("Restore Purchases", RestorePurchases);
        CreateButton("Switch Username", SwitchUser);
        CreateButton("Can Make Payments", CanMakePayments);
        CreateButton("Set Subs Attributes", SetSubscriberAttributes);
        CreateButton("Log in as \"test\"", LogInAsTest);
        CreateButton("Log in as random id", LogInAsRandomId);
        CreateButton("Log out", LogOut);
        CreateButton("Do Other Stuff", DoOtherStuff);
        CreateButton("Check Intro Eligibility", CheckIntroEligibility);

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
        var buttonTransform = (RectTransform)button.transform;

        var rect = buttonTransform.rect;
        var height = rect.height;
        var width = rect.width;

        var yPos = -1 * ((currentButtons / maxButtonsPerRow) *
            (height + yPaddingForButtons) + minYOffsetForButtons + (height / 2));
        var xPos = (currentButtons % maxButtonsPerRow) * (width + xPaddingForButtons) 
                   + minXOffsetForButtons + (width / 2);
        
        var newButtonTransform = (RectTransform)button.transform;
        newButtonTransform.anchorMin = new Vector2(0, 1);
        newButtonTransform.anchorMax = new Vector2(0, 1);
        
        newButtonTransform.anchoredPosition = new Vector2(xPos, yPos);
        // button.transform.anchorMin = new Vector2(1, 0);
        // button.transform.anchorMax = new Vector2(0, 1);
        
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

    void GetOfferings()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = offerings.ToString();
            }
        });
    }

    void RestorePurchases()
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

    void SyncPurchases()
    {
        var purchases = GetComponent<Purchases>();
        purchases.SyncPurchases();
        infoLabel.text = "Purchases sync started. Note: there's no callback for this method in Unity";
    }

    void CanMakePayments()
    {
        var purchases = GetComponent<Purchases>();
        purchases.CanMakePayments((canMakePayments, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = $"Can make payments: {canMakePayments}";
            }
        });
    }

    void SetSubscriberAttributes()
    {
        var purchases = GetComponent<Purchases>();
        Int32 unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        var timestampString = $"{unixTimestamp}";
        purchases.SetAttributes(new Dictionary<string, string>
        {
            { "timestamp", timestampString },
        });
        purchases.SetEmail($"email_{timestampString}@revenuecat.com");
        purchases.SetPhoneNumber($"{timestampString}");
        purchases.SetDisplayName($"displayName_{timestampString}");
        purchases.SetPushToken($"pushtoken_{timestampString}");
        purchases.SetAdjustID($"adjustId_{timestampString}");
        purchases.SetAppsflyerID($"appsflyerId_{timestampString}");
        purchases.SetFBAnonymousID($"fbAnonymousId_{timestampString}");
        purchases.SetMparticleID($"mparticleId_{timestampString}");
        purchases.SetOnesignalID($"onesignalId_{timestampString}");
        purchases.SetAirshipChannelID($"airshipChannelId_{timestampString}");
        purchases.SetMediaSource($"mediaSource_{timestampString}");
        purchases.SetCampaign($"campaign_{timestampString}");
        purchases.SetAdGroup($"adgroup_{timestampString}");
        purchases.SetAd($"ad_{timestampString}");
        purchases.SetKeyword($"keyword_{timestampString}");
        purchases.SetCreative($"creative_{timestampString}");
        purchases.CollectDeviceIdentifiers();
    }
    
    void LogInAsTest()
    {
        LogIn("test");
    }
    void LogInAsRandomId()
    {
        Guid appUserID = Guid.NewGuid();
        LogIn(appUserID.ToString());
    }

    void LogIn(string appUserID)
    {
        var purchases = GetComponent<Purchases>();
        purchases.LogIn(appUserID, (customerInfo, created, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = $"created: {created}\n customerInfo:\n{customerInfo.ToString()}";
            }
        }); 
    }
    void LogOut()
    {
        var purchases = GetComponent<Purchases>();
        purchases.LogOut((customerInfo, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                infoLabel.text = customerInfo.ToString();
            }
        }); 
    }

    void CheckIntroEligibility()
    {
        var purchases = GetComponent<Purchases>();
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                var products = offerings.All
                    .Values
                    .Select(offering => offering.AvailablePackages)
                    .SelectMany(x => x)
                    .ToList()
                    .Select(package => package.StoreProduct.Identifier)
                    .ToArray();
                purchases.GetProducts(products, (storeProducts, innerError) =>
                {
                    if (innerError != null)
                    {
                        LogError(innerError);
                    }
                    else
                    {
                        var items = products.Select(arg => $"{arg.ToString()}");
                        infoLabel.text = $"{{ \n { string.Join(Environment.NewLine, items) }\n }} \n"; }
                });
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
        infoLabel.text = JsonUtility.ToJson(error);
    }

    private void DisplayCustomerInfo(Purchases.CustomerInfo customerInfo)
    {
        infoLabel.text = customerInfo.ToString();
    }
}
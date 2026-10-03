// Created by Antonio Pallares. Copyright (c) 2026 RevenueCat, Inc.

using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class SDKUpdateTestApp : MonoBehaviour
{
    [Serializable]
    public class BuildInfo
    {
        public string apiKey;
        public string sdkVersion;
        public string sdkSource;
    }

    public GameObject homeScreen;
    public GameObject purchaseScreen;
    public Text appUserId;
    public Text sdkVersion;
    public Text activeEntitlements;
    public Text errorLabel;
    public Button logInButton;
    public Button purchaseScreenButton;
    public Button purchaseButton;
    public Button backButton;

    private Purchases purchases;
    private Purchases.Package monthlyPackage;
    private string userIdToLogIn;

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string SDKUpdateLoginUserId();
    [DllImport("__Internal")]
    private static extern void NativeAccessibilityOverlayInit();
    [DllImport("__Internal")]
    private static extern void NativeAccessibilityOverlayClear();
    [DllImport("__Internal")]
    private static extern void NativeAccessibilityOverlaySetElement(string id, string text,
        int left, int top, int right, int bottom);
#elif UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaClass overlay;
#endif

    private void Start()
    {
        var build = JsonUtility.FromJson<BuildInfo>(Resources.Load<TextAsset>("SDKUpdateBuildInfo").text);
        purchases = GetComponent<Purchases>();
        purchases.Configure(Purchases.PurchasesConfiguration.Builder.Init(build.apiKey).Build());
        purchases.SetLogLevel(Purchases.LogLevel.Verbose);
        sdkVersion.text = "RevenueCat SDK " + build.sdkVersion + " " + build.sdkSource;
        appUserId.text = purchases.GetAppUserId();
        userIdToLogIn = GetLoginUserId();
        logInButton.gameObject.SetActive(!string.IsNullOrEmpty(userIdToLogIn));
        logInButton.onClick.AddListener(LogIn);
        purchaseScreenButton.onClick.AddListener(ShowPurchaseScreen);
        purchaseButton.onClick.AddListener(Purchase);
        backButton.onClick.AddListener(ShowHomeScreen);
#if UNITY_IOS && !UNITY_EDITOR
        NativeAccessibilityOverlayInit();
#elif UNITY_ANDROID && !UNITY_EDITOR
        overlay = new AndroidJavaClass("com.revenuecat.accessibility.NativeAccessibilityOverlay");
        overlay.CallStatic("init");
#endif
        ShowHomeScreen();
    }

    private string GetLoginUserId()
    {
#if UNITY_IOS && !UNITY_EDITOR
        return SDKUpdateLoginUserId();
#elif UNITY_ANDROID && !UNITY_EDITOR
        using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        using var intent = activity.Call<AndroidJavaObject>("getIntent");
        return intent.Call<string>("getStringExtra", "app_user_id_to_log_in");
#else
        return null;
#endif
    }

    private void LogIn()
    {
        logInButton.interactable = false;
        purchases.LogIn(userIdToLogIn, (info, created, error) =>
        {
            logInButton.interactable = true;
            if (error != null) { ShowError(error.Message); return; }
            appUserId.text = purchases.GetAppUserId();
            SetElement("app_user_id", appUserId.text, appUserId.rectTransform);
        });
    }

    private void ShowHomeScreen()
    {
        homeScreen.SetActive(true);
        purchaseScreen.SetActive(false);
        errorLabel.text = "";
        StartCoroutine(UpdateAccessibility());
    }

    private void ShowPurchaseScreen()
    {
        homeScreen.SetActive(false);
        purchaseScreen.SetActive(true);
        errorLabel.text = "";
        activeEntitlements.text = "Loading...";
        monthlyPackage = null;
        purchaseButton.interactable = false;
        StartCoroutine(UpdateAccessibility());
        purchases.GetCustomerInfo((info, error) =>
        {
            if (error != null) { ShowError(error.Message); return; }
            SetCustomerInfo(info);
        });
        purchases.GetOfferings((offerings, error) =>
        {
            if (error != null) { ShowError(error.Message); return; }
            if (!offerings.All.TryGetValue("no_paywall", out var offering) || offering.Monthly == null)
            {
                ShowError("The no_paywall offering must contain a monthly package");
                return;
            }
            monthlyPackage = offering.Monthly;
            purchaseButton.interactable = true;
        });
    }

    private void Purchase()
    {
        if (monthlyPackage == null) return;
        purchaseButton.interactable = false;
        purchases.PurchasePackage(monthlyPackage, result =>
        {
            purchaseButton.interactable = true;
            if (result.Error != null) { ShowError(result.Error.Message); return; }
            if (!result.UserCancelled) SetCustomerInfo(result.CustomerInfo);
        });
    }

    private void SetCustomerInfo(Purchases.CustomerInfo info)
    {
        var entitlements = info.Entitlements.Active.Keys.OrderBy(id => id).ToArray();
        activeEntitlements.text = entitlements.Length == 0 ? "None" : string.Join(", ", entitlements);
        SetElement("active_entitlements", activeEntitlements.text, activeEntitlements.rectTransform);
    }

    private void ShowError(string message)
    {
        errorLabel.text = "Error: " + message;
        SetElement("error", errorLabel.text, errorLabel.rectTransform);
        Debug.LogError(message);
    }

    private IEnumerator UpdateAccessibility()
    {
        yield return null;
#if UNITY_IOS && !UNITY_EDITOR
        NativeAccessibilityOverlayClear();
#elif UNITY_ANDROID && !UNITY_EDITOR
        overlay.CallStatic("clear");
#endif
        if (homeScreen.activeSelf)
        {
            SetElement("sdk_version", sdkVersion.text, sdkVersion.rectTransform);
            SetElement("app_user_id", appUserId.text, appUserId.rectTransform);
            SetElement("purchase_screen_button", "Purchase", purchaseScreenButton.GetComponent<RectTransform>());
            if (logInButton.gameObject.activeSelf)
                SetElement("log_in_button", "Log in", logInButton.GetComponent<RectTransform>());
        }
        else
        {
            SetElement("active_entitlements", activeEntitlements.text, activeEntitlements.rectTransform);
            SetElement("purchase_button", "Purchase monthly subscription", purchaseButton.GetComponent<RectTransform>());
            SetElement("back_button", "Back", backButton.GetComponent<RectTransform>());
        }
    }

    private void SetElement(string id, string text, RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        int left = Mathf.RoundToInt(corners[0].x);
        int right = Mathf.RoundToInt(corners[2].x);
        int top = Screen.height - Mathf.RoundToInt(corners[1].y);
        int bottom = Screen.height - Mathf.RoundToInt(corners[0].y);
#if UNITY_IOS && !UNITY_EDITOR
        NativeAccessibilityOverlaySetElement(id, text, left, top, right, bottom);
#elif UNITY_ANDROID && !UNITY_EDITOR
        overlay.CallStatic("setElement", id, text, left, top, right, bottom);
#endif
    }
}

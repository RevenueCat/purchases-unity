using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

static class SceneSetup
{
    [MenuItem("Build/Setup Scene")]
    public static void SetupScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camera = new GameObject("Main Camera");
        camera.AddComponent<Camera>();
        camera.tag = "MainCamera";

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        var canvas = new GameObject("Canvas");
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(375, 812);
        canvas.AddComponent<GraphicRaycaster>();

        var testCasesScreen = CreatePanel(canvas.transform, "TestCasesScreen");
        CreateText(testCasesScreen.transform, "Title", "Test Cases", 24, FontStyle.Bold,
            new Vector2(0, 0.5f), new Vector2(1, 1f), TextAnchor.MiddleCenter);

        var purchaseBtn = CreateButton(testCasesScreen.transform, "PurchaseButton", "Purchase through paywall",
            new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.4f));

        var purchaseScreen = CreatePanel(canvas.transform, "PurchaseScreen");
        purchaseScreen.SetActive(false);

        var entitlementsLabel = CreateText(purchaseScreen.transform, "EntitlementsLabel", "Entitlements: none", 16, FontStyle.Normal,
            new Vector2(0, 0.6f), new Vector2(1, 0.7f), TextAnchor.MiddleCenter);

        var paywallBtn = CreateButton(purchaseScreen.transform, "PaywallButton", "Present Paywall",
            new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.5f));

        var errorLabel = CreateText(purchaseScreen.transform, "ErrorLabel", "", 14, FontStyle.Normal,
            new Vector2(0, 0.2f), new Vector2(1, 0.3f), TextAnchor.MiddleCenter);
        errorLabel.color = Color.red;
        errorLabel.gameObject.SetActive(false);

        var backBtn = CreateButton(purchaseScreen.transform, "BackButton", "Back",
            new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.2f));

        var purchasesGO = new GameObject("Purchases");
        var purchasesComp = purchasesGO.AddComponent<Purchases>();
        purchasesComp.useRuntimeSetup = true;

        var maestroApp = purchasesGO.AddComponent<MaestroTestApp>();
        maestroApp.testCasesScreen = testCasesScreen;
        maestroApp.purchaseScreen = purchaseScreen;
        maestroApp.entitlementsLabel = entitlementsLabel;
        maestroApp.errorLabel = errorLabel;

        var purchaseBtnComp = purchaseBtn.GetComponent<Button>();
        purchaseBtnComp.onClick.AddListener(maestroApp.ShowPurchaseScreen);

        var paywallBtnComp = paywallBtn.GetComponent<Button>();
        paywallBtnComp.onClick.AddListener(maestroApp.PresentPaywall);

        var backBtnComp = backBtn.GetComponent<Button>();
        backBtnComp.onClick.AddListener(maestroApp.ShowTestCases);

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");

        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true)
        };
        EditorBuildSettings.scenes = scenes;

        PlayerSettings.productName = "MaestroTestApp";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.revenuecat.automatedsdktests");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.revenuecat.automatedsdktests");

        Debug.Log(":: Scene setup completed successfully");
    }

    static GameObject CreatePanel(Transform parent, string name)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    static Text CreateText(Transform parent, string name, string text, int fontSize, FontStyle style,
        Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.alignment = alignment;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.color = Color.white;
        return t;
    }

    static GameObject CreateButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f, 1f);
        go.AddComponent<Button>();

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        var t = textGo.AddComponent<Text>();
        t.text = label;
        t.fontSize = 16;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.color = Color.white;

        return go;
    }
}

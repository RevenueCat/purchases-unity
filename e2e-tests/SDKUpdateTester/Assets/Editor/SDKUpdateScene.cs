// Created by Antonio Pallares. Copyright (c) 2026 RevenueCat, Inc.

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

static class SDKUpdateScene
{
    public static Purchases Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.white;
        var events = new GameObject("EventSystem");
        events.AddComponent<UnityEngine.EventSystems.EventSystem>();
        events.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        var canvas = new GameObject("Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(375, 812);
        scaler.matchWidthOrHeight = 0;
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        var host = new GameObject("Purchases");
        var purchases = host.AddComponent<Purchases>();
        purchases.useRuntimeSetup = true;
        var app = host.AddComponent<SDKUpdateTestApp>();
        app.homeScreen = Panel(canvas.transform, "Home");
        app.sdkVersion = Label(app.homeScreen.transform, "SDKVersion", "", 100, 20);
        app.appUserId = Label(app.homeScreen.transform, "AppUserId", "", 150, 14);
        app.logInButton = Button(app.homeScreen.transform, "LogIn", "Log in", 220);
        app.purchaseScreenButton = Button(app.homeScreen.transform, "PurchaseScreen", "Purchase", 300);

        app.purchaseScreen = Panel(canvas.transform, "Purchase");
        Label(app.purchaseScreen.transform, "Title", "Active entitlements", 100, 20);
        app.activeEntitlements = Label(app.purchaseScreen.transform, "ActiveEntitlements", "Loading...", 150, 16);
        app.purchaseButton = Button(app.purchaseScreen.transform, "Purchase", "Purchase monthly subscription", 220);
        app.backButton = Button(app.purchaseScreen.transform, "Back", "Back", 300);
        app.errorLabel = Label(canvas.transform, "Error", "", 390, 14);
        app.errorLabel.color = Color.red;
        app.purchaseScreen.SetActive(false);
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
        return purchases;
    }

    private static GameObject Panel(Transform parent, string name)
    {
        var panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return panel;
    }

    private static RectTransform Row(Transform parent, string name, float top, float height)
    {
        var row = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        row.SetParent(parent, false);
        row.anchorMin = new Vector2(0, 1);
        row.anchorMax = new Vector2(1, 1);
        row.pivot = new Vector2(0, 1);
        row.offsetMin = new Vector2(16, -top - height);
        row.offsetMax = new Vector2(-16, -top);
        return row;
    }

    private static Text Label(Transform parent, string name, string text, float top, int size)
    {
        var label = Row(parent, name, top, 50).gameObject.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = size;
        label.alignment = TextAnchor.UpperLeft;
        label.color = Color.black;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        return label;
    }

    private static Button Button(Transform parent, string name, string text, float top)
    {
        var rect = Row(parent, name, top, 50);
        rect.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f);
        var button = rect.gameObject.AddComponent<Button>();
        var label = Label(rect, "Text", text, 0, 16);
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        return button;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RevenueCat.UI;

public static class RevenueCatInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeOnLoad()
    {
        SetupPurchases();
        EnsureUIWithButtons();
    }

    private static void SetupPurchases()
    {
        var existing = Object.FindObjectOfType<Purchases>();
        if (existing != null)
        {
            return;
        }

        var go = new GameObject("PurchasesManager");
        Object.DontDestroyOnLoad(go);

        var purchases = go.AddComponent<Purchases>();
        purchases.useRuntimeSetup = false; // use inspector-style setup in Start()

        // Provide keys so Purchases.Start() can configure immediately per platform
#if UNITY_IOS || UNITY_VISIONOS
        purchases.revenueCatAPIKeyApple = "appl_UnETLmScXGAZNPiptqYuBdRCgIK";
#endif
#if UNITY_ANDROID
        purchases.revenueCatAPIKeyGoogle = "goog_zBcCIlPAzFjPIayWTPqSLgSkKAh";
#endif

        // Set log level after Purchases.Start() initialized wrappers
        go.AddComponent<RevenueCatInitRunner>();
    }

    private static void EnsureUIWithButtons()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            // Still add our buttons host for convenience if not found
            var existingButtons = Object.FindObjectOfType<RevenueCatUIButtons>();
            if (existingButtons == null)
            {
                AddButtonsToCanvas(canvas.gameObject);
            }
            return;
        }

        var canvasGo = new GameObject("RevenueCatCanvas");
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        var eventSystem = Object.FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            var esGo = new GameObject("EventSystem");
            eventSystem = esGo.AddComponent<EventSystem>();
        }

        // Ensure the proper input module is present based on active input handling
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
        {
            Object.Destroy(oldModule);
        }
        if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
#else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif

        AddButtonsToCanvas(canvasGo);
    }

    private static void AddButtonsToCanvas(GameObject canvasGo)
    {
        var host = new GameObject("RevenueCatUIButtonsHost");
        host.transform.SetParent(canvasGo.transform, false);
        var buttons = host.AddComponent<RevenueCatUIButtons>();

        // Create a vertical layout to hold buttons
        var panel = new GameObject("Panel");
        panel.transform.SetParent(host.transform, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(320, 140);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.spacing = 10f;

        CreateButton(panel.transform, "Show Paywall", buttons.ShowPaywall);
        CreateButton(panel.transform, "Customer Center", buttons.ShowCustomerCenter);
    }

    private static void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        var buttonGo = new GameObject(label + " Button");
        buttonGo.transform.SetParent(parent, false);

        var image = buttonGo.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.9f, 1f);

        var button = buttonGo.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        var rect = buttonGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 50);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(buttonGo.transform, false);

        var text = textGo.AddComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        var builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtinFont != null)
        {
            text.font = builtinFont;
        }

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}

public class RevenueCatUIButtons : MonoBehaviour
{
    public async void ShowPaywall()
    {
        if (!RevenueCatUI.IsSupported())
        {
            Debug.LogWarning("RevenueCat UI not supported on this platform. Build to iOS/Android.");
            return;
        }
        try
        {
            var result = await RevenueCatUI.PresentPaywall();
            Debug.Log($"Paywall result: {result.Result}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error presenting paywall: {e.Message}");
        }
    }

    public async void ShowCustomerCenter()
    {
        if (!RevenueCatUI.IsSupported())
        {
            Debug.LogWarning("RevenueCat UI not supported on this platform. Build to iOS/Android.");
            return;
        }
        try
        {
            await RevenueCatUI.PresentCustomerCenter();
            Debug.Log("Customer center dismissed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error presenting customer center: {e.Message}");
        }
    }
}



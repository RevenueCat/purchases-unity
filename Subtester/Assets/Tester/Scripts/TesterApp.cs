using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RevenueCat.Tester.Screens;

namespace RevenueCat.Tester
{
    [RequireComponent(typeof(UIDocument))]
    public class TesterApp : Purchases.UpdatedCustomerInfoListener
    {
        [SerializeField] private StyleSheet styleSheet;

        private UIDocument _uiDocument;
        private Purchases _purchases;
        private LogConsole _logConsole;
        private VisualElement _root;
        private VisualElement _contentArea;
        private VisualElement _tabBarContent;
        private Label _statusLabel;
        private Rect _lastSafeArea;

        private readonly List<(string name, ScreenBase screen)> _screens = new();
        private readonly List<Button> _tabButtons = new();
        private int _activeTabIndex = -1;

        public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo)
        {
            _logConsole?.Log($"[Listener] Customer info updated — " +
                             $"Active subs: {customerInfo.ActiveSubscriptions.Count}");
        }

        private void OnEnable()
        {
            _uiDocument = GetComponent<UIDocument>();
            _purchases = FindFirstObjectByType<Purchases>();

            if (_purchases == null)
            {
                Debug.LogError("[Tester] No Purchases component found in scene.");
                return;
            }

            if (_purchases.listener == null)
            {
                _purchases.listener = this;
            }

            BuildUI();
        }

        private void BuildUI()
        {
            _root = _uiDocument.rootVisualElement;
            _root.Clear();
            _root.AddToClassList("root");

            if (styleSheet != null)
            {
                _root.styleSheets.Add(styleSheet);
            }

            BuildStatusBar(_root);
            BuildTabBar(_root);
            BuildContentArea(_root);

            _logConsole = new LogConsole(_root);
            _logConsole.Log("Tester ready");

            CreateScreens();
            PopulateTabBar();
            SelectTab(0);

            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            var safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;

            if (_root == null) return;

            var panel = _root.panel;
            if (panel == null) return;

            var screenHeight = Screen.height;
            var screenWidth = Screen.width;
            if (screenHeight == 0 || screenWidth == 0) return;

            // Screen coords: origin bottom-left, Y up
            // Panel coords: origin top-left, Y down
            // RuntimePanelUtils.ScreenToPanel handles all scaling automatically
            var panelTopLeft = RuntimePanelUtils.ScreenToPanel(panel,
                new Vector2(0, screenHeight));
            var panelBottomRight = RuntimePanelUtils.ScreenToPanel(panel,
                new Vector2(screenWidth, 0));

            var panelSafeTopRight = RuntimePanelUtils.ScreenToPanel(panel,
                new Vector2(safeArea.xMax, safeArea.yMax));
            var panelSafeBottomLeft = RuntimePanelUtils.ScreenToPanel(panel,
                new Vector2(safeArea.xMin, safeArea.yMin));

            float topInset = panelSafeTopRight.y - panelTopLeft.y;
            float bottomInset = panelBottomRight.y - panelSafeBottomLeft.y;
            float leftInset = panelSafeBottomLeft.x - panelTopLeft.x;
            float rightInset = panelBottomRight.x - panelSafeTopRight.x;

            _root.style.paddingTop = Mathf.Max(0, topInset);
            _root.style.paddingBottom = Mathf.Max(0, bottomInset);
            _root.style.paddingLeft = Mathf.Max(0, leftInset);
            _root.style.paddingRight = Mathf.Max(0, rightInset);
        }

        private void BuildStatusBar(VisualElement root)
        {
            var bar = new VisualElement();
            bar.AddToClassList("status-bar");

            var appIdLabel = new Label("App User ID:");
            appIdLabel.AddToClassList("status-label");

            _statusLabel = new Label("...");
            _statusLabel.AddToClassList("status-value");

            bar.Add(appIdLabel);
            bar.Add(_statusLabel);
            root.Add(bar);
        }

        private void BuildTabBar(VisualElement root)
        {
            var tabScroll = new ScrollView(ScrollViewMode.Horizontal);
            tabScroll.AddToClassList("tab-bar");
            tabScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            tabScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            _tabBarContent = tabScroll.contentContainer;
            root.Add(tabScroll);
        }

        private void BuildContentArea(VisualElement root)
        {
            _contentArea = new VisualElement();
            _contentArea.AddToClassList("content-area");
            root.Add(_contentArea);
        }

        private void CreateScreens()
        {
            _screens.Clear();
            _screens.Add(("Identity", new IdentityScreen(_purchases, _logConsole)));
            _screens.Add(("Offerings", new OfferingsScreen(_purchases, _logConsole)));
            _screens.Add(("Purchase", new PurchaseScreen(_purchases, _logConsole)));
            _screens.Add(("Customer", new CustomerInfoScreen(_purchases, _logConsole)));
            _screens.Add(("Paywalls", new PaywallScreen(_purchases, _logConsole)));
            _screens.Add(("Attributes", new AttributesScreen(_purchases, _logConsole)));
            _screens.Add(("Tools", new ToolsScreen(_purchases, _logConsole)));
        }

        private void PopulateTabBar()
        {
            _tabButtons.Clear();

            for (int i = 0; i < _screens.Count; i++)
            {
                var index = i;
                var tab = new Button(() => SelectTab(index));
                tab.text = _screens[i].name;
                tab.AddToClassList("tab-button");
                _tabBarContent.Add(tab);
                _tabButtons.Add(tab);
            }
        }

        private void SelectTab(int index)
        {
            if (_activeTabIndex == index) return;

            if (_activeTabIndex >= 0 && _activeTabIndex < _tabButtons.Count)
            {
                _tabButtons[_activeTabIndex].RemoveFromClassList("tab-active");
            }

            _tabButtons[index].AddToClassList("tab-active");

            _contentArea.Clear();
            _contentArea.Add(_screens[index].screen.Root);

            _activeTabIndex = index;

            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            if (_purchases == null || !_purchases.IsConfigured()) return;

            var appUserId = _purchases.GetAppUserId();
            var anon = _purchases.IsAnonymous() ? " (anon)" : "";
            _statusLabel.text = appUserId + anon;
        }

        private void Update()
        {
            UpdateStatusBar();

            if (Screen.safeArea != _lastSafeArea)
            {
                ApplySafeArea();
            }
        }
    }
}

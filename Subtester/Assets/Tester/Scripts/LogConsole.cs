using System;
using UnityEngine.UIElements;

namespace RevenueCat.Tester
{
    public class LogConsole
    {
        private readonly ScrollView _scrollView;
        private readonly VisualElement _panel;
        private readonly Button _toggleButton;
        private bool _collapsed;

        public LogConsole(VisualElement root)
        {
            _panel = new VisualElement();
            _panel.AddToClassList("log-panel");

            var header = new VisualElement();
            header.AddToClassList("log-header");

            var title = new Label("Console");
            title.AddToClassList("log-title");

            _toggleButton = new Button(ToggleCollapse) { text = "\u25BC" };
            _toggleButton.AddToClassList("log-toggle");

            var clearButton = new Button(Clear) { text = "Clear" };
            clearButton.AddToClassList("log-clear");

            header.Add(title);
            header.Add(_toggleButton);
            header.Add(clearButton);

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.AddToClassList("log-scroll");

            _panel.Add(header);
            _panel.Add(_scrollView);
            root.Add(_panel);
        }

        public void Log(string message)
        {
            AddEntry(message);
        }

        public void LogError(string message)
        {
            AddEntry(message, "log-error");
        }

        public void LogSuccess(string message)
        {
            AddEntry(message, "log-success");
        }

        private void AddEntry(string message, string extraClass = null)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var entry = new Label($"[{timestamp}] {message}");
            entry.AddToClassList("log-entry");
            if (extraClass != null) entry.AddToClassList(extraClass);

            _scrollView.Add(entry);

            entry.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                _scrollView.verticalScroller.value = _scrollView.verticalScroller.highValue;
            });

            UnityEngine.Debug.Log($"[Tester] {message}");
        }

        private void ToggleCollapse()
        {
            _collapsed = !_collapsed;
            if (_collapsed)
            {
                _panel.AddToClassList("log-collapsed");
                _toggleButton.text = "\u25B2";
            }
            else
            {
                _panel.RemoveFromClassList("log-collapsed");
                _toggleButton.text = "\u25BC";
            }
        }

        private void Clear()
        {
            _scrollView.Clear();
        }
    }
}

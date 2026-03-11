using System;
using UnityEngine.UIElements;

namespace RevenueCat.Tester.Screens
{
    public abstract class ScreenBase
    {
        public VisualElement Root { get; }
        protected ScrollView Content { get; }
        protected LogConsole Console { get; }
        protected Purchases Purchases { get; }

        private bool _firstSection = true;

        protected ScreenBase(Purchases purchases, LogConsole console)
        {
            Purchases = purchases;
            Console = console;

            Root = new VisualElement();
            Root.style.flexGrow = 1;

            Content = new ScrollView(ScrollViewMode.Vertical);
            Content.AddToClassList("screen-scroll");
            Root.Add(Content);

            Build();
        }

        protected abstract void Build();

        protected void Log(string message) => Console.Log(message);
        protected void LogError(string message) => Console.LogError(message);
        protected void LogSuccess(string message) => Console.LogSuccess(message);

        protected void LogError(Purchases.Error error)
        {
            Console.LogError($"{error}");
        }

        protected void AddSectionHeader(string text)
        {
            var label = new Label(text);
            label.AddToClassList("section-header");
            if (_firstSection)
            {
                label.AddToClassList("section-header-first");
                _firstSection = false;
            }
            Content.Add(label);
        }

        protected Button AddButton(string text, Action clicked)
        {
            var btn = new Button(clicked) { text = text };
            btn.AddToClassList("action-button");
            Content.Add(btn);
            return btn;
        }

        protected Button AddSecondaryButton(string text, Action clicked)
        {
            var btn = new Button(clicked) { text = text };
            btn.AddToClassList("secondary-button");
            Content.Add(btn);
            return btn;
        }

        protected Button AddDangerButton(string text, Action clicked)
        {
            var btn = new Button(clicked) { text = text };
            btn.AddToClassList("danger-button");
            Content.Add(btn);
            return btn;
        }

        protected VisualElement AddButtonRow(params (string text, Action clicked)[] buttons)
        {
            var row = new VisualElement();
            row.AddToClassList("button-row");
            foreach (var (label, action) in buttons)
            {
                var btn = new Button(action) { text = label };
                btn.AddToClassList("action-button");
                row.Add(btn);
            }
            Content.Add(row);
            return row;
        }

        protected VisualElement AddSecondaryButtonRow(params (string text, Action clicked)[] buttons)
        {
            var row = new VisualElement();
            row.AddToClassList("button-row");
            foreach (var (label, action) in buttons)
            {
                var btn = new Button(action) { text = label };
                btn.AddToClassList("secondary-button");
                row.Add(btn);
            }
            Content.Add(row);
            return row;
        }

        protected TextField AddTextField(string label, string placeholder = "", string value = "")
        {
            var field = new TextField(label) { value = value };
            field.AddToClassList("input-field");
            if (!string.IsNullOrEmpty(placeholder))
            {
                field.textEdition.placeholder = placeholder;
            }
            Content.Add(field);
            return field;
        }

        protected Label AddInfoLabel(string text = "")
        {
            var label = new Label(text);
            label.AddToClassList("info-label");
            Content.Add(label);
            return label;
        }

        protected VisualElement AddDynamicContainer()
        {
            var container = new VisualElement();
            container.AddToClassList("dynamic-container");
            Content.Add(container);
            return container;
        }

        protected void AddSpacer()
        {
            var spacer = new VisualElement();
            spacer.AddToClassList("spacer");
            Content.Add(spacer);
        }
    }
}

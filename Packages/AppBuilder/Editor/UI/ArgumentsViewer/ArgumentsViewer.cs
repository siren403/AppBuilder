using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class ArgumentsViewer : AppBuilderVisualElement
    {
        protected override string UXML => $"Editor/UI/{nameof(ArgumentsViewer)}/{nameof(ArgumentsViewer)}.uxml";
        protected override string USS => $"Editor/UI/{nameof(ArgumentsViewer)}/{nameof(ArgumentsViewer)}.uss";

        private readonly ScrollView _content;

        public Arguments Args
        {
            set
            {
                _content.Clear();

                foreach (var pair in value)
                {
                    var row = new VisualElement();
                    row.AddToClassList("row-container");

                    var left = new Cell() {Text = pair.Key};
                    left.AddToClassList("column-1");

                    var right = new Cell() {Text = pair.Value};
                    right.AddToClassList("column-2");

                    row.Add(left);
                    row.Add(right);

                    _content.Add(row);
                }
            }
        }

        public string CommandLineArgs { set; private get; }
        private readonly Label _labelLog;
        private readonly EnabledTransitionController _logTransitionController;
        private readonly VisualElement _overlay;

        public ArgumentsViewer()
        {
            AddToClassList("popup");

            _overlay = this.Q(className: "popup-overlay");
            _overlay.RemoveFromClassList("unity-button");

            var closeButtons = this.Query<Button>(className: "close").ToList();
            foreach (var button in closeButtons)
            {
                button.clicked += Hide;
            }

            this.Q<Button>("btn-copy").clicked += CopyToClipboard;
            _content = this.Q<ScrollView>("content");

            SetEnabled(false);

            var viewer = this.Q(className: "arguments-viewer");
            viewer.SetEnabled(false);
            viewer.RegisterCallback<TransitionEndEvent>(e =>
            {
                if (!viewer.enabledSelf)
                {
                    SetEnabled(false);
                }
            });

            _labelLog = this.Q<Label>("label-log");
            _logTransitionController = new EnabledTransitionController(_labelLog);
        }

        private void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = CommandLineArgs;
            _labelLog.text = "success";
            _logTransitionController.In();
        }

        public void Show()
        {
            SetEnabled(true);
            this.Q(className: "arguments-viewer").SetEnabled(true);
        }

        public void Hide()
        {
            this.Q(className: "arguments-viewer").SetEnabled(false);
        }

        public new class UxmlFactory : UxmlFactory<ArgumentsViewer, UxmlTraits>
        {
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class ArgumentsViewer : VisualElement
    {
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

        private readonly VisualElement _overlay;

        public ArgumentsViewer()
        {
            this.AddResource(nameof(ArgumentsViewer));
            AddToClassList("popup");

            _overlay = this.Q(className: "popup-overlay");
            _overlay.RemoveFromClassList("unity-button");

            var closeButtons = this.Query<Button>(className: "close").ToList();
            foreach (var button in closeButtons)
            {
                button.clicked += Hide;
            }


            var viewer = this.Q(className: "arguments-viewer");
            viewer.RegisterCallback<TransitionEndEvent>(e =>
            {
                if (e.target is VisualElement ve && ClassListContains("popup-hide"))
                {
                    EnableInClassList("popup-disable", true);
                }
            });

            bool isLogTransision = false;

            this.Q<Button>("btn-copy").clicked += () =>
            {
                CopyToClipboard();
                if (isLogTransision) return;
                isLogTransision = true;
                EnableInClassList("show-log", true);
            };
            _content = this.Q<ScrollView>("content");

            _labelLog = this.Q<Label>("label-log");
            _labelLog.RegisterCallback<TransitionEndEvent>(e =>
            {
                if (!(e.target is VisualElement ve)) return;
                if (ClassListContains("show-log"))
                {
                    EnableInClassList("show-log", false);
                    EnableInClassList("hide-log", true);
                }
                else if (ClassListContains("hide-log"))
                {
                    EnableInClassList("hide-log", false);
                    isLogTransision = false;
                }
            });
        }


        private void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = CommandLineArgs;
            _labelLog.text = "success";
        }

        public void Show()
        {
            EnableInClassList("popup-disable", false);
            EnableInClassList("popup-hide", false);
        }

        public void Hide()
        {
            EnableInClassList("popup-hide", true);
        }

        public new class UxmlFactory : UxmlFactory<ArgumentsViewer, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            private UxmlStringAttributeDescription _class = new() {name = "class"};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is not ArgumentsViewer e) return;


                e.EnableInClassList("popup-disable", _class.GetValueFromBag(bag, cc).Contains("popup-hide"));
            }
        }
    }
}
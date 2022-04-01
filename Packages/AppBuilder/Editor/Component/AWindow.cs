using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = AppBuilder.UI.PackageInfo;

[assembly: UxmlNamespacePrefix("Editor.Component", "comp")]

namespace Editor.Component
{
    #region AWindow

    public class AWindow : UIToolkitWindow
    {
        [MenuItem("AppBuilder/AWindow")]
        public static void ShowWindow()
        {
            UIToolkitWindow wnd = GetWindow<AWindow>();
            wnd.titleContent = new GUIContent("AWindow");
            wnd.minSize = new Vector2(450, 600);
            wnd.Show();
        }

        protected override void Render()
        {
            Load(PackageInfo.GetPath("Editor/Component/UIToolkitWindow.uxml"),
                PackageInfo.GetPath("Editor/Component/UIToolkitWindow.uss"));

            var a = new AComponent();
            Add(a);
            var b = new BComponent();
            a.Add(b);
            var c = new CComponent();
            b.Add(c);
        }

        public static int initCounter = 0;

        private void OnDisable()
        {
            initCounter = 0;
        }
    }

    public class TestComponent : Component
    {
        private readonly Label _label;
        private readonly Toggle _toggle;

        public TestComponent()
        {
            var container = new VisualElement();

            _label = new Label(nameof(AComponent));
            container.Add(_label);

            _toggle = new Toggle()
            {
                value = false
            };
            container.Add(_toggle);
            container.style.flexDirection = FlexDirection.Row;
            _label.text = $"{_label.text}_i{++AWindow.initCounter}";

            Add(container);
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            private readonly UxmlIntAttributeDescription _depth = new() {name = "depth"};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is not TestComponent e) return;
                var depth = _depth.GetValueFromBag(bag, cc);
                e.style.marginLeft = new StyleLength(new Length(depth * 10));
            }
        }
    }

    public class AComponent : TestComponent
    {
        public new class UxmlFactory : UxmlFactory<AComponent, UxmlTraits>
        {
        }
    }

    public class BComponent : TestComponent
    {
        public new class UxmlFactory : UxmlFactory<BComponent, UxmlTraits>
        {
        }
    }

    public class CComponent : TestComponent
    {
        public new class UxmlFactory : UxmlFactory<CComponent, UxmlTraits>
        {
        }
    }

    #endregion


   
}
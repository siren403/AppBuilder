using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder
{
    public static class VisualElementExtensions
    {
        public static Label EnableTextTooltip(this Label label, bool isEnable = true)
        {
            label.tooltip = isEnable ? label.text : string.Empty;
            return label;
        }

        public static void SetPickingMode(this Toggle toggle, PickingMode mode)
        {
            toggle.pickingMode = mode;
            toggle.Children().First().pickingMode = mode;
        }

        public static void Initialize(this DropdownField field,
            List<string> choices,
            int defaultIndex,
            Action<ChangeEvent<string>> callback,
            Action<string> setter = null,
            Func<string, string> getter = null,
            List<string> defaultChoices = null)
        {
            field.RegisterCallback<ChangeEvent<string>>(e =>
            {
                if (!field.choices.Contains(e.newValue))
                {
                    field.SetValueWithoutNotify(field.choices.First());
                }

                setter?.Invoke(field.value);
                callback(e);
            });
            if (choices.Any())
            {
                field.choices = choices;
                field.value = getter?.Invoke(choices[defaultIndex]);
            }
            else if (defaultChoices != null && defaultChoices.Any())
            {
                field.choices = defaultChoices;
                field.value = defaultChoices.First();
            }
        }

        public static void ApplyAsset(this VisualElement element, string uxml, string uss)
        {
            element.CloneTreeUXML(uxml);
            element.AddStyleSheet(uss);
        }

        public static void ApplyPath(this VisualElement element, string path)
        {
            element.CloneTreeUXML(path);
            element.AddStyleSheet(path);
        }

        public static void CloneTreeUXML(this VisualElement element, string uxml)
        {
            if (string.IsNullOrEmpty(uxml))
            {
                // throw new ArgumentNullException(nameof(uxml));
                Debug.LogError($"empty uxml: {uxml}");
                return;
            }

            if (!Path.HasExtension(uxml))
            {
                uxml = Path.ChangeExtension(uxml, "uxml");
            }

            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
            if (tree == null)
            {
                // throw new ArgumentNullException(uxml);
                Debug.LogError($"not found uxml: {uxml}");
                return;
            }

            tree.CloneTree(element);
        }


        public static void AddStyleSheet(this VisualElement element, string uss)
        {
            if (string.IsNullOrEmpty(uss))
            {
                Debug.LogError($"empty uss: {uss}");
                return;
            }

            if (!Path.HasExtension(uss))
            {
                uss = Path.ChangeExtension(uss, "uss");
            }

            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(uss);
            if (style == null) Debug.LogWarning($"not found uss: {uss}");
            else element.styleSheets.Add(style);
        }

        public static void Add<T>(this VisualElement element, T child, out T result) where T : VisualElement
        {
            element.Add(child);
            result = child;
        }


        public static void AddVisualTree(this VisualElement element, string path)
        {
            var tree = LoadVisualTreeAsset(path);
            tree.CloneTree(element);
        }


        private static VisualTreeAsset LoadVisualTreeAsset(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // throw new ArgumentNullException(nameof(uxml));
                Debug.LogError($"empty uxml: {path}");
                return null;
            }

            if (!Path.HasExtension(path))
            {
                path = Path.ChangeExtension(path, "uxml");
            }

            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            if (tree == null)
            {
                // throw new ArgumentNullException(uxml);
                Debug.LogError($"not found uxml: {path}");
                return null;
            }

            return tree;
        }
    }
}
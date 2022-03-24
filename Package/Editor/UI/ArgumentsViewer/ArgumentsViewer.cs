using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class ArgumentsViewer : VisualElement
    {
        public ArgumentsViewer()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                PackageInfo.GetPath($"Editor/UI/{nameof(ArgumentsViewer)}/{nameof(ArgumentsViewer)}.uxml"));
            uxml.CloneTree(this);

            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                PackageInfo.GetPath($"Editor/UI/{nameof(ArgumentsViewer)}/{nameof(ArgumentsViewer)}.uss"));
            styleSheets.Add(uss);

            AddToClassList("arguments-viewer");

            this.Q<Button>("btn-close").clicked += RemoveFromHierarchy;
        }
    }
}
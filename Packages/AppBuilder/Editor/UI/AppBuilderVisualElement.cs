using UnityEditor;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public abstract class AppBuilderVisualElement : VisualElement
    {
        protected abstract string UXML { get; }
        protected abstract string USS { get; }

        protected AppBuilderVisualElement()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PackageInfo.GetPath(UXML));
            uxml.CloneTree(this);

            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(PackageInfo.GetPath(USS));
            styleSheets.Add(uss);
        }
    }
}
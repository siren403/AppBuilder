using Editor.Component;
using UnityEditor;
using UnityEngine;

namespace AppBuilder.UI.Window
{
    public class BuildsWindow : UIToolkitWindow
    {
        [MenuItem("AppBuilder/Builds")]
        public static void ShowWindow()
        {
            BuildsWindow wnd = GetWindow<BuildsWindow>();
            wnd.titleContent = new GUIContent("BuildsWindow");
            wnd.minSize = new Vector2(450, 600);
        }

        protected override void Render()
        {
            rootVisualElement.LoadPath(PackageInfo.GetPath($"Editor/UI/Window/{nameof(BuildsWindow)}"));
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace AppBuilder.UI.View
{
    public class AppBuilderWindow : EditorWindow
    {
        [MenuItem("AppBuilder/Builds")]
        public static void ShowWindow()
        {
            AppBuilderWindow wnd = GetWindow<AppBuilderWindow>();
            wnd.titleContent = new GUIContent("AppBuilderWindow");
            wnd.minSize = new Vector2(450, 600);
        }

        private void CreateGUI()
        {
            rootVisualElement.Add(new AppBuilderView());
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace AppBuilder
{
    public class BuildsWindow : EditorWindow
    {
        [MenuItem("Builds/Dashboard")]
        private static void ShowWindow()
        {
            var window = GetWindow<BuildsWindow>();
            window.titleContent = new GUIContent("Builds");
            window.Show();
        }

        private void OnGUI()
        {
            
        }
    }
}
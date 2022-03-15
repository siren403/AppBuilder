using Builds;
using UnityEngine;
using UnityEngine.UIElements;

public class AppSettingViewer : MonoBehaviour
{
    [SerializeField] private string path;
    [SerializeField] private UIDocument document;

    private void Awake()
    {
        var appSettings = Resources.Load<AppSettingsScriptableObject>(path);

        var container = document.rootVisualElement.Q("container");
        container.Add(new Label($"host: {appSettings.Value.Host}"));
        container.Add(new Label($"package: {appSettings.Value.Package}"));
    }
}
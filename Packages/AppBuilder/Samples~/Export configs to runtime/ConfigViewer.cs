using AppBuilderSample;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfigViewer : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private string configPath;

    private void Awake()
    {
        var config = Resources.Load<ConfigScriptableObject>(configPath);
        Debug.Log(config.Value.Host);

        var root = document.rootVisualElement;

        root.Q<Label>("label-host").text = config.Value.Host;
        root.Q<Label>("label-appid").text = config.Value.AppId;
    }
}
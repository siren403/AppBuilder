using System;
using System.Linq;
using Builds;
using UnityEngine;
using UnityEngine.UIElements;

public class AppSettingViewer : MonoBehaviour
{
    [SerializeField] private string path;
    [SerializeField] private UIDocument document;
    [SerializeField] private GameObject cube;

    [SerializeField] private float speed = 1;
    [SerializeField] private float distance = 2;
    private void Awake()
    {
        var appSettings = Resources.Load<AppSettingsScriptableObject>(path);

        speed = appSettings.Value.Speed;

        var container = document.rootVisualElement.Q("container");
        container.Add(new Label($"host: {appSettings.Value.Host}"));
        container.Add(new Label($"package: {appSettings.Value.Package}"));

        if (appSettings.TryGetSection("AppId", out string appid))
        {
            container.Add(new Label($"[AppId] {appid}"));
        }

        switch (appSettings.Value.Platform)
        {
            case Platform.OneStore:
                var products = appSettings.GetSections<string>("Products").ToArray();
                if (products.Any())
                {
                    container.Add(new Label("[Products]"));
                }

                foreach (var product in products)
                {
                    container.Add(new Label(product));
                }

                break;
        }
    }

    public void Update()
    {
        var y = Mathf.Sin(Mathf.PI * Time.realtimeSinceStartup) * speed;
        cube.transform.position = new Vector3(0, y * distance, 0);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
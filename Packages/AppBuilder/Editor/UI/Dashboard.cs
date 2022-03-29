using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

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

    public static class PackageInfo
    {
        public const string Name = "com.qkrsogusl3.appbuilder";
        public static string GetPath(string path) => $"Packages/{Name}/{path}";

        public const string Version = "0.0.1";

        public static string SamplesPath => $"Assets/Samples/AppBuilder/{Version}";
    }

    public static class BuildCache
    {
        public static string GetKey(this BuildInfo build, string key) => $"{build.FullName}_{key}";

        public static void SetString(BuildInfo build, string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                PlayerPrefs.DeleteKey(build.GetKey(key));
            }
            else
            {
                PlayerPrefs.SetString(build.GetKey(key), value);
            }
        }

        public static string GetString(BuildInfo build, string key, string defaultValue = null)
        {
            return PlayerPrefs.GetString(build.GetKey(key), defaultValue);
        }

        public static void SetString(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                PlayerPrefs.DeleteKey($"{nameof(AppBuilder)}_{key}");
            }
            else
            {
                PlayerPrefs.SetString($"{nameof(AppBuilder)}_{key}", value);
            }
        }

        public static string GetString(string key, string defaultValue = null)
        {
            return PlayerPrefs.GetString($"{nameof(AppBuilder)}_{key}", defaultValue);
        }
    }

    public static class DashboardExtensions
    {
        public static DropdownField BuildField(this Dashboard window) =>
            window.rootVisualElement.Q<DropdownField>("build-field");
    }

    // public enum BuildMode
    // {
    //     Build,
    //     Preview,
    //     Configure
    // }

    public class Dashboard : EditorWindow
    {
        public static readonly string UXML = PackageInfo.GetPath("Editor/UI/Dashboard.uxml");

        private static readonly List<string> NothingBuilds = new()
        {
            "Nothing"
        };

        private static readonly List<string> BaseVariants = new()
        {
            "Auto"
        };

        [MenuItem("AppBuilder/Dashboard")]
        public static void ShowExample()
        {
            Dashboard wnd = GetWindow<Dashboard>();
            wnd.titleContent = new GUIContent("Dashboard");
            wnd.minSize = new Vector2(450, 600);
        }


        private readonly BuildController _controller = new();
        private readonly ArgumentsRenderer _argumentsRenderer = new();
        private string _selectedBuild;

        private static void Load(VisualElement visualElement)
        {
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML);
            visualTree.CloneTree(visualElement);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            Load(root);

            // _builds = BuildPlayer.GetBuilds().ToDictionary(build => build.Name, build => build);
            _controller.Initialize();

            var buildNames = _controller.Any() ? _controller.BuildNames : NothingBuilds;

            var builds = this.BuildField();
            builds.Initialize(buildNames, 0, e =>
                {
                    _selectedBuild = e.newValue;
                    ExecuteAndRender(BuildController.BuildMode.Preview);
                },
                _ => BuildCache.SetString(builds.name, _),
                _ => BuildCache.GetString(builds.name, _),
                NothingBuilds
            );

            root.Q<Button>("btn-arguments-viewer").clicked += () =>
            {
                var (build, report) = ExecuteAndRender(BuildController.BuildMode.Preview);
                var viewer = root.Q<ArgumentsViewer>();
                viewer.Args = report.Args;
                viewer.CommandLineArgs = report.ToCommandLineArgs(build);
                viewer.Show();
            };
            root.Q<Button>("btn-cache-clear").clicked += PlayerPrefs.DeleteAll;
            root.Q<Button>("btn-refresh").clicked += () => { ExecuteAndRender(BuildController.BuildMode.Preview); };
            root.Q<Button>("btn-run").clicked += () =>
            {
                ExecuteAndRender(BuildController.BuildMode.Configure);
                EditorApplication.EnterPlaymode();
            };
            root.Q<Button>("btn-apply").clicked += () => { ExecuteAndRender(BuildController.BuildMode.Configure); };
            root.Q<Button>("btn-build").clicked += () => { ExecuteAndRender(BuildController.BuildMode.Build); };
        }

        public (BuildInfo, BuildPlayer.Report) ExecuteAndRender(BuildController.BuildMode mode)
        {
            if (string.IsNullOrEmpty(_selectedBuild) || NothingBuilds.Contains(_selectedBuild)) return (null, null);
            var result = _controller.ExecuteBuild(_selectedBuild, mode);
            RenderReport(result.build, result.report);
            return result;
        }

        private void RenderReport(BuildInfo build, BuildPlayer.Report report)
        {
            var preview = rootVisualElement.Q("preview");
            preview.Clear();
            if (report != null)
            {
                VisualElement section = null;
                foreach (var property in report.Properties)
                {
                    switch (property.Options)
                    {
                        case BuildPropertyOptions.SectionBegin:
                            section = new VisualElement();
                            section.AddToClassList("section");

                            var head = new Label(property.Name);
                            head.AddToClassList("section-head-2");
                            section.Add(head);

                            var content = new VisualElement();
                            content.AddToClassList("section-content-2");
                            section.Add(content);

                            preview.Add(section);

                            section = content;
                            break;
                        case BuildPropertyOptions.SectionEnd:
                            section = null;
                            break;
                        default:
                            var parent = section ?? preview;
                            // parent.Add(CreateItem(property.Name, property.Value));
                            parent.Add(new SettingProperty()
                            {
                                Key = property.Name,
                                Value = property.Value
                            });
                            break;
                    }
                }
            }

            _argumentsRenderer.Render(this, build, report);
        }
    }
}
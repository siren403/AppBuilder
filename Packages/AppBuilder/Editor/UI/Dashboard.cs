using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public static class PackageInfo
    {
        public const string Name = "com.qkrsogusl3.appbuilder";
        public static string GetPath(string path) => $"Packages/{Name}/{path}";

        public const string Version = "0.0.1";

        public static string SamplesPath => $"Assets/Samples/AppBuilder/{Version}";
    }

    public static class DashboardExtensions
    {
        public static DropdownField BuildField(this Dashboard window) =>
            window.rootVisualElement.Q<DropdownField>("build-field");
    }

    public class Dashboard : EditorWindow
    {
        public static readonly string UXML = PackageInfo.GetPath("Editor/UI/Dashboard.uxml");
        public static readonly string USS = PackageInfo.GetPath("Editor/UI/Dashboard.uss");

        private static readonly List<string> NothingBuilds = new()
        {
            "Nothing"
        };

        // [MenuItem("AppBuilder/Dashboard")]
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
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML);
            tree.CloneTree(visualElement);

            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS);
            visualElement.styleSheets.Add(style);
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
            root.Q<Button>("btn-build").clicked += () =>
            {
                var (build, report) = ExecuteAndRender(BuildController.BuildMode.Build);
                var local = report.UnityReport.summary.outputPath;
                var remote = "/build";

                var args = new StringBuilder();
                args.Append("./Tools/AppBuilderConsoleExtension/Publish/win-x64/AppBuilderConsoleExtension.exe");
                args.Append(" ftp upload");
                args.Append(" --host localhost");
                args.Append(" --user appbuilder");
                args.Append(" --passwd 0000");
                args.Append($" --local {local}");
                args.Append(" --remote /publish");
                using var process = new Process()
                {
                    StartInfo = new ProcessStartInfo("powershell.exe")
                    {
                        Arguments = args.ToString(),
                    }
                };
                process.Start();
            };
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
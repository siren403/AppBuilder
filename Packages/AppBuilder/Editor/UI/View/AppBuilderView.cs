using System;
using System.IO;
using System.Linq;
using Editor.Component;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class AppBuilderView : VisualElement
    {
        public interface ICache
        {
            string Get(string key, string defaultValue = null);
            void Set(string key, string value);
        }

        public class BuildInfoCache : ICache
        {
            private readonly BuildInfo _buildInfo;

            public BuildInfoCache(BuildInfo buildInfo)
            {
                _buildInfo = buildInfo;
            }

            public string Get(string key, string defaultValue = null)
            {
                return BuildCache.GetString(_buildInfo, key, defaultValue);
            }

            public void Set(string key, string value)
            {
                BuildCache.SetString(_buildInfo, key, value);
            }
        }

        public static readonly string ResourcesPath = PackageInfo.GetPath("Editor/UI/UIResources");

        private readonly InputNode _inputNode;
        private readonly BuildNode _buildNode;
        private readonly ScrollView _scrollView;

        private readonly BuildController _controller = new();

        private string _selectedBuild;

        private string SelectedBuild
        {
            get
            {
                if (string.IsNullOrEmpty(_selectedBuild))
                {
                    EnableNoBuild();
                    throw new ArgumentNullException(nameof(SelectedBuild));
                }

                return _selectedBuild;
            }
        }

        private IPostBuildJob[] _postJobs;

        public AppBuilderView()
        {
            this.AddClassByType<AppBuilderView>();
            this.AddResource($"{nameof(AppBuilderView)}");

            _inputNode = this.Q<InputNode>();
            _buildNode = this.Q<BuildNode>();
            _scrollView = this.Q<ScrollView>();

            _controller.Initialize();

            var noBuild = this.Q("no-build");
            noBuild.style.display = _controller.Any() ? DisplayStyle.None : DisplayStyle.Flex;

            if (!_controller.Any()) return;

            var buildsMenu = this.Q<ToolbarMenu>("builds-menu");

            var buildNames = _controller.BuildNames;
            foreach (var buildName in buildNames)
            {
                buildsMenu.menu.AppendAction(buildName, _ => { SelectBuild(buildsMenu, _.name); });
            }

            var toolbarArgumentsViewer = this.Q<ToolbarButton>("arguments-viewer-button");
            toolbarArgumentsViewer.clicked += () =>
            {
                var (build, report) = ExecuteAndRender(BuildController.BuildMode.Preview);
                var viewer = this.Q<ArgumentsViewer>();
                viewer.Args = report.Args;
                viewer.CommandLineArgs = report.ToCommandLineArgs(build);
                viewer.Show();
            };

            var toolbarClear = this.Q<ToolbarButton>("clear-button");
            toolbarClear.clicked += PlayerPrefs.DeleteAll;

            var toolbarReload = this.Q<ToolbarButton>("reload-button");
            toolbarReload.clicked += () => ExecuteAndRender(BuildController.BuildMode.Preview);

            var toolbarConfigure = this.Q<ToolbarButton>("configure-button");
            toolbarConfigure.clicked += () => ExecuteAndRender(BuildController.BuildMode.Configure);

            var toolbarPlay = this.Q<ToolbarButton>("play-button");
            toolbarPlay.clicked += () =>
            {
                ExecuteAndRender(BuildController.BuildMode.Configure);
                EditorApplication.EnterPlaymode();
            };

            var toolbarBuild = this.Q<ToolbarButton>("build-button");
            toolbarBuild.clicked += () => { var (build, report) = ExecuteAndRender(BuildController.BuildMode.Build); };

            SelectBuild(buildsMenu, BuildCache.GetString(buildsMenu.name, buildNames.First()));
        }

        private void EnableNoBuild()
        {
            var noBuild = this.Q("no-build");
            noBuild.style.display = DisplayStyle.Flex;
        }

        private void SelectBuild(ToolbarMenu buildsMenu, string buildName)
        {
            try
            {
                var (build, report) = Build(buildName, BuildController.BuildMode.Preview);

                _selectedBuild = buildName;

                BuildCache.SetString(buildsMenu.name, buildName);
                buildsMenu.text = $"Select: {build.Name}";

                RenderNode(build, report);
            }
            catch (Exception e)
            {
                if (_controller.Any())
                {
                    SelectBuild(buildsMenu, _controller.BuildNames.First());
                }
                else
                {
                    EnableNoBuild();
                    Debug.LogError(e);
                    PlayerPrefs.DeleteAll();
                }
            }
        }

        public (BuildInfo, BuildPlayer.Report) ExecuteAndRender(BuildController.BuildMode mode)
        {
            var (build, report) = Build(SelectedBuild, mode);
            RenderNode(build, report);
            return (build, report);
        }

        private void RenderNode(BuildInfo build, [CanBeNull] BuildPlayer.Report report)
        {
            if (report == null) return;

            var inputs = build.Inputs.ToDictionary(_ => _.Name, _ => _);

            var sortedArgs = report.Args
                .OrderByDescending(pair => inputs.ContainsKey(pair.Key))
                .Select(_ => _.Value)
                .ToList();

            _inputNode.Render(sortedArgs, inputs, new BuildInfoCache(build));
            _buildNode.Render(report.Properties);

            _postJobs = build.PostBuildJobs.ToArray();

            var container = this.Q("post-jobs");
            container.Clear();
            foreach (var job in _postJobs)
            {
                var node = new JobNode()
                {
                    Job = job,
                    Depth = job.IsEnabled ? 3 : 0
                };
                if (job is IBuildJobRenderer renderer)
                {
                    renderer.Render(node.contentContainer);
                }
  
                container.Add(node);
            }
        }

        private (BuildInfo, BuildPlayer.Report) Build(string buildName, BuildController.BuildMode mode)
        {
            //pre
            var (build, report) = _controller.ExecuteBuild(buildName, mode);

            if (mode == BuildController.BuildMode.Build)
            {
                //post
                foreach (var job in _postJobs)
                {
                    job.Run(report);
                }
            }

            return (build, report);
        }
    }


    public static class AppBuilderViewExtensions
    {
        public static void AddResource(this VisualElement element, string path)
        {
            if (Path.HasExtension(path))
            {
                switch (Path.GetExtension(path))
                {
                    case ".uxml":
                        element.AddVisualTree($"{AppBuilderView.ResourcesPath}/uxml/{path}");
                        return;
                    case ".uss":
                        element.AddStyleSheet($"{AppBuilderView.ResourcesPath}/uss/{path}");
                        return;
                }
            }

            element.AddVisualTree(Path.ChangeExtension($"{AppBuilderView.ResourcesPath}/uxml/{path}", "uxml"));
            element.AddStyleSheet(Path.ChangeExtension($"{AppBuilderView.ResourcesPath}/uss/{path}", "uss"));
        }

        public static void AddClassByType<T>(this VisualElement element) where T : VisualElement
        {
            element.AddToClassList(typeof(T).Name.PascalToKebabCase());
        }
    }
}
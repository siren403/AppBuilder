using System;
using System.IO;
using System.Linq;
using Editor.Component;
using JetBrains.Annotations;
using UnityEditor.UIElements;
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
        private readonly JobNode _jobNode;

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

        public AppBuilderView()
        {
            this.AddClassByType<AppBuilderView>();
            this.AddResource($"{nameof(AppBuilderView)}");

            _inputNode = this.Q<InputNode>();
            _buildNode = this.Q<BuildNode>();
            _jobNode = this.Q<JobNode>();

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
                var (build, report) = _controller.ExecuteBuild(buildName, BuildController.BuildMode.Preview);

                _selectedBuild = buildName;

                BuildCache.SetString(buildsMenu.name, buildName);
                buildsMenu.text = $"Select: {build.Name}";

                RenderNode(build, report);
            }
            catch
            {
                EnableNoBuild();
            }
        }

        public (BuildInfo, BuildPlayer.Report) ExecuteAndRender(BuildController.BuildMode mode)
        {
            var (build, report) = _controller.ExecuteBuild(SelectedBuild, mode);
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

            // foreach (var input in build.Inputs)
            // {
            //     var field = new DynamicInputField()
            //     {
            //         Key = input.Name
            //     };
            //
            //     switch (input.Options)
            //     {
            //         case InputOptions.Directory:
            //             field.Type = DynamicInputField.InputType.Directory;
            //             field.Value = BuildCache.GetString(build, input.Name)
            //             break;
            //         case InputOptions.File:
            //             break;
            //         case InputOptions.Dropdown:
            //             break;
            //         default:
            //             break;
            //     }
            //
            //     _inputNode.contentContainer.Add(field);
            // }
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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
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
    }

    public static class DashboardExtensions
    {
        public static DropdownField BuildField(this Dashboard window) =>
            window.rootVisualElement.Q<DropdownField>("build-field");
    }

    public enum BuildMode
    {
        Build,
        Preview,
        Configure
    }

    public class Dashboard : EditorWindow
    {
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

        private Dictionary<string, BuildInfo> _builds;

        private PreviewContext _context;

        private static void Load(VisualElement visualElement)
        {
            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AppBuilder/Editor/UI/Dashboard.uxml");
            VisualElement uxml = visualTree.Instantiate();
            visualElement.Add(uxml);
            // visualTree.CloneTree(visualElement);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/AppBuilder/Editor/UI/Dashboard.uss");
            // VisualElement labelWithStyle = new Label("Hello World! With Style");
            // labelWithStyle.styleSheets.Add(styleSheet);
            // visualElement.Add(labelWithStyle);
            visualElement.styleSheets.Add(styleSheet);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            Load(root);

            _builds = BuildPlayer.GetBuilds().ToDictionary(build => build.Name, build => build);
            var buildNames = _builds.Any() ? _builds.Keys.ToList() : NothingBuilds;

            var builds = root.Q<DropdownField>("build-field");
            builds.RegisterCallback<ChangeEvent<string>>(e =>
            {
                Debug.Log($"Build: {e.newValue}");
                ExecuteBuild(BuildMode.Preview);
            });
            if (buildNames.Any())
            {
                builds.choices = buildNames;
                builds.value = buildNames.First();
            }
            else
            {
                builds.choices = NothingBuilds;
                builds.value = NothingBuilds.First();
            }

            root.Q<Button>("btn-cache-clear").clicked += PlayerPrefs.DeleteAll;
            root.Q<Button>("btn-refresh").clicked += () => { ExecuteBuild(BuildMode.Preview); };
            root.Q<Button>("btn-run").clicked += () =>
            {
                ExecuteBuild(BuildMode.Configure);
                EditorApplication.EnterPlaymode();
            };
            root.Q<Button>("btn-apply").clicked += () => { ExecuteBuild(BuildMode.Configure); };
            root.Q<Button>("btn-build").clicked += () => { ExecuteBuild(); };
            // root.Bind(new SerializedObject(this));
        }

        private bool TryGetSelectedBuild(out BuildInfo build)
        {
            var buildName = this.BuildField().value;
            if (_builds.TryGetValue(buildName, out build))
            {
                return true;
            }

            return false;
        }


        private void ExecuteBuild(BuildMode mode = BuildMode.Build)
        {
            if (!TryGetSelectedBuild(out var build)) return;

            var inputArgs = new Arguments();

            var inputs = build.Inputs;
            foreach (var input in inputs)
            {
                var cachedValue = BuildCache.GetString(build, input.Name);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    inputArgs[input.Name] = ArgumentValue.Empty(input.Name, ArgumentCategory.Input);
                }
                else
                {
                    inputArgs[input.Name] = new ArgumentValue(input.Name, cachedValue, ArgumentCategory.Input);
                }
            }

            switch (mode)
            {
                case BuildMode.Preview:
                    inputArgs["mode"] = new ArgumentValue("mode", "preview", ArgumentCategory.Custom);
                    break;
                case BuildMode.Configure:
                    inputArgs["mode"] = new ArgumentValue("mode", "configure", ArgumentCategory.Custom);
                    break;
            }

            var report = BuildPlayer.Execute(build, inputArgs);
            RenderReport(build, report);
            if (mode == BuildMode.Configure)
            {
                ExecuteBuild(BuildMode.Preview);
            }
        }

        private void RenderReport(BuildInfo build, BuildPlayer.Report report)
        {
            var preview = rootVisualElement.Q("preview");
            preview.RemoveChildren();

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

                            preview.Add(section);
                            break;
                        case BuildPropertyOptions.SectionEnd:
                            section = null;
                            break;
                        default:
                            var parent = section ?? preview;
                            parent.Add(CreateItem(property.Name, property.Value));
                            break;
                    }
                }
            }

            #region Args

            RenderArgs(build, report);

            #endregion

            VisualElement CreateItem(string label, string text)
            {
                var itemContainer = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween
                    }
                };
                itemContainer.Add(new Label(label));
                itemContainer.Add(new Label(text.Replace("\\", "/")).EnableTextTooltip());
                return itemContainer;
            }
        }

        private void RenderArgs(BuildInfo build, BuildPlayer.Report report)
        {
            var argsContainer = rootVisualElement.Q("args");
            argsContainer.RemoveChildren();

            var customContainer = rootVisualElement.Q("custom");
            customContainer.RemoveChildren();

            var inputContainer = rootVisualElement.Q("input");
            inputContainer.RemoveChildren();

            if (report == null) return;

            var inputs = build.Inputs.ToDictionary(i => i.Name, i => i);

            report.Args.RemoveUnityArgs();
            foreach (var arg in report.Args)
            {
                switch (arg.Value.Category)
                {
                    case ArgumentCategory.Custom:
                        customContainer.Add(new Argument(arg.Key)
                        {
                            IsValue = true,
                            Value = arg.Value
                        });
                        break;
                    case ArgumentCategory.Input:
                        if (!inputs.TryGetValue(arg.Key, out var input)) continue;
                        var inputComponent = new Argument(arg.Key);
                        switch (input.Options)
                        {
                            case InputOptions.Directory:
                                inputComponent.IsInput = true;
                                inputComponent.IsFolder = true;
                                inputComponent.Value = BuildCache.GetString(build, arg.Key, arg.Value);
                                inputComponent.RegisterInputChangedCallback(e =>
                                {
                                    BuildCache.SetString(build, arg.Key, e.newValue);
                                    ExecuteBuild(BuildMode.Preview);
                                });
                                break;
                            case InputOptions.File:
                                inputComponent.IsInput = true;
                                inputComponent.IsFile = true;
                                inputComponent.FileExtension = input.Extension;
                                inputComponent.Value = BuildCache.GetString(build, arg.Key, arg.Value);
                                inputComponent.RegisterInputChangedCallback(e =>
                                {
                                    BuildCache.SetString(build, arg.Key, e.newValue);
                                    ExecuteBuild(BuildMode.Preview);
                                });
                                break;
                            case InputOptions.Dropdown:
                                inputComponent.IsDropdown = true;
                                inputComponent.Choices = new List<string>()
                                {
                                    "None",
                                    // "Auto"
                                }.Concat(input.Values).ToList();
                                inputComponent.Value = BuildCache.GetString(build, arg.Key, "None");
                                inputComponent.RegisterDropdownChangedCallback(e =>
                                {
                                    BuildCache.SetString(build, arg.Key, e.newValue);
                                    ExecuteBuild(BuildMode.Preview);
                                });
                                break;
                            default:
                                inputComponent.IsInput = true;
                                inputComponent.Value = BuildCache.GetString(build, arg.Key, input.Value);
                                inputComponent.RegisterInputChangedCallback(e =>
                                {
                                    BuildCache.SetString(build, arg.Key, e.newValue);
                                    ExecuteBuild(BuildMode.Preview);
                                });
                                break;
                        }

                        inputContainer.Add(inputComponent);
                        break;
                    default:
                        argsContainer.Add(new Argument(arg.Key)
                        {
                            IsValue = true,
                            Value = arg.Value
                        });
                        break;
                }
            }
        }
    }
}
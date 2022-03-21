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
            PlayerPrefs.SetString(build.GetKey(key), value);
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

        public static DropdownField VariantField(this Dashboard window) =>
            window.rootVisualElement.Q<DropdownField>("variant-field");
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

            RenderVariants(build);

            var args = BuildPlayer.GetReservedArguments();
            var inputArgs = new Arguments();

            var variantName = BuildCache.GetString(build, "variant");
            if (!string.IsNullOrEmpty(variantName))
            {
                args["variant"] = new ArgumentValue("variant", variantName, ArgumentCategory.Custom);
                inputArgs["variant"] = args["variant"];
            }

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
                    inputArgs[input.Name] = new ArgumentValue(input.Name, Smart.Format(cachedValue, args),
                        ArgumentCategory.Input);
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
            if (mode == BuildMode.Configure)
            {
                ExecuteBuild(BuildMode.Preview);
            }
            else
            {
                RenderReport(build, report);
            }
        }

        private void RenderVariants(BuildInfo build)
        {
            var variantField = this.VariantField();

            if (string.IsNullOrEmpty(variantField.value))
            {
                variantField.choices = BaseVariants.Concat(build.Variants).ToList();
                variantField.SetValueWithoutNotify(variantField.choices.First());
                variantField.RegisterValueChangedCallback(e => { ExecuteBuild(BuildMode.Preview); });
            }

            var selectedVariant = variantField.value;
            if (selectedVariant == "Auto")
            {
                selectedVariant = build.Name;
            }

            BuildCache.SetString(build, "variant", selectedVariant);
        }

        private void RenderReport(BuildInfo build, BuildPlayer.Report report)
        {
            var preview = rootVisualElement.Q("preview");
            preview.RemoveChildren();

            if (report != null)
            {
                foreach (var property in report.Properties)
                {
                    preview.Add(CreateItem(property.Name, property.Value));
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
                itemContainer.Add(new Label(text.Replace("\\", "/")));
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


            report.Args.RemoveUnityArgs();
            foreach (var arg in report.Args)
            {
                switch (arg.Value.Category)
                {
                    case ArgumentCategory.Custom:
                        customContainer.Add(new Argument(arg.Key)
                        {
                            Value = arg.Value
                        });
                        break;
                    case ArgumentCategory.Input:
                        var inputComponent = new Argument(arg.Key)
                        {
                            IsInput = true,
                            IsFolder = true,
                            Input = BuildCache.GetString(build, arg.Key),
                            Value = arg.Value
                        };
                        inputComponent.RegisterValueChangedCallback(e =>
                        {
                            BuildCache.SetString(build, arg.Key, e.newValue);
                            ExecuteBuild(BuildMode.Preview);
                        });
                        inputContainer.Add(inputComponent);
                        break;
                    default:
                        argsContainer.Add(new Argument(arg.Key)
                        {
                            Value = arg.Value
                        });
                        break;
                }
            }
        }
    }
}
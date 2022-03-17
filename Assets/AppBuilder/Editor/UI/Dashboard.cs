using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
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

    public class Dashboard : EditorWindow
    {
        private static readonly List<string> NothingBuilds = new()
        {
            "Nothing"
        };

        [MenuItem("AppBuilder/Dashboard")]
        public static void ShowExample()
        {
            Dashboard wnd = GetWindow<Dashboard>();
            wnd.titleContent = new GUIContent("Dashboard");
            wnd.minSize = new Vector2(450, 600);
        }

        private Dictionary<string, BuildInfo> _builds;
        private string _selectedBuild;
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

            var builds = root.Q<DropdownField>("build-field");
            builds.RegisterCallback<ChangeEvent<string>>(e =>
            {
                Debug.Log($"Build: {e.newValue}");
                if (!_builds.TryGetValue(e.newValue, out var build)) return;

                var args = BuildPlayer.GetReservedArguments();
                var cachedVariant = BuildCache.GetString(build, "variant");
                if (!string.IsNullOrEmpty(cachedVariant))
                {
                    args["variant"] = new ArgumentValue("variant", cachedVariant, ArgumentCategory.Custom);
                }

                var inputs = build.Inputs;
                var inputArgs = new Arguments();
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

                inputArgs["mode"] = new ArgumentValue("mode", "preview", ArgumentCategory.Custom);

                var report = BuildPlayer.Execute(build, inputArgs);
                RenderReport(build, report);
            });
            var buildNames = _builds.Keys.ToList();
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
            root.Q<Button>("btn-refresh").clicked += () => { RefreshPreview(); };
            root.Q<Button>("btn-run").clicked += () =>
            {
                RefreshPreview();
                EditorApplication.EnterPlaymode();
            };
            root.Q<Button>("btn-apply").clicked += () =>
            {
                if (_builds.TryGetValue(_selectedBuild, out var method) && _context != null)
                {
                    _context.Args.EnableArg("CONFIGURE_ONLY", true);
                    BuildPlayer.BuildPreview(method, _context.Args);
                    _context.Args.EnableArg("CONFIGURE_ONLY", false);
                }
            };
            root.Q<Button>("btn-build").clicked += () =>
            {
                if (_builds.TryGetValue(_selectedBuild, out var method) && _context != null)
                {
                    //todo: validate
                    //ex) output empty -> open folder panel
                    BuildPlayer.BuildPreview(method, _context.Args);
                }
            };
            root.Bind(new SerializedObject(this));
        }


        private void RenderReport(BuildInfo build, BuildPlayer.Report report)
        {
            var preview = rootVisualElement.Q("preview");
            preview.RemoveChildren();

            foreach (var property in report.Properties)
            {
                preview.Add(CreateItem(property.Name, property.Value));
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
            var inputs = build.Inputs
                .Select(attr => attr.Name)
                .ToDictionary(key => key, key => string.Empty);

            var argsContainer = rootVisualElement.Q("args");
            argsContainer.RemoveChildren();

            var customContainer = rootVisualElement.Q("custom");
            customContainer.RemoveChildren();

            var inputContainer = rootVisualElement.Q("input");
            inputContainer.RemoveChildren();

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
                        inputComponent.RegisterValueChangedCallback(e => { });
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


        private void SelectBuild(string buildName)
        {
            if (!_builds.TryGetValue(buildName, out var build)) return;
            _selectedBuild = buildName;

            var inputs = build.Inputs;

            string GetPrefsKey(MethodInfo method, InputAttribute arg)
            {
                return $"SAVED_{method.Name}_{arg.Name}";
            }

            var savedArgs = inputs.Select(arg =>
                {
                    return (key: arg.Name, value: PlayerPrefs.GetString(GetPrefsKey(build.Method, arg)));
                })
                .Where(arg => !string.IsNullOrEmpty(arg.value))
                .ToDictionary(arg => arg.key, arg => arg.value);

            var variants = build.Variants;

            var variantField = rootVisualElement.Q<DropdownField>("variant-field");
            if (!variants.Any())
            {
                variantField.visible = false;
            }
            else
            {
                variantField.visible = true;
                var choices = new List<string>() {"Auto"};
                choices.AddRange(variants);
                variantField.choices = choices;
                // variantField.value = variantField.choices.First(); //todo: or default or saved

                variantField.RegisterValueChangedCallback(e =>
                {
                    if (_context != null)
                    {
                        var variant = e.newValue;
                        if (string.IsNullOrEmpty(e.newValue))
                        {
                            variant = variantField.choices.First();
                            variantField.SetValueWithoutNotify(variant);
                        }

                        if (variant.Equals("Auto"))
                        {
                            variant = build.Name;
                        }

                        _context.Args["variant"] = variant;
                        _context.Args["outputPath"] =
                            Smart.Format(PlayerPrefs.GetString($"{build.Name}_outputPath"), _context.Args);
                        RefreshPreview();
                    }
                });
                // if (variantField.value.Equals("Auto"))
                // {
                //     savedArgs["variant"] = build.Name;
                // }
                // else
                // {
                //     savedArgs["variant"] = variantField.value;
                // }
            }

            // ApplyPreview(build, BuildPlayer.Preview(build, savedArgs));
        }

        private void RefreshPreview(BuildInfo build = null, PreviewContext context = null)
        {
            if (build == null && !_builds.TryGetValue(_selectedBuild, out build)) return;
            context ??= _context;
            if (context == null) return;

            using (BuildPlayer.Preview(build, context.Args, out var newContext))
            {
                ApplyPreview(build, newContext);
            }
        }

        private void ApplyPreview(BuildInfo build, PreviewContext context)
        {
            _context = context;

            var preview = rootVisualElement.Q("preview");
            preview.RemoveChildren();

            foreach (var property in context.Properties)
            {
                preview.Add(CreateItem(property.Name, property.Value));
            }

            #region Args

            RenderArgs(build, context);

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

        private void RenderArgs(BuildInfo build, PreviewContext context)
        {
            var inputs = build.Inputs
                .Select(attr => attr.Name)
                .ToDictionary(key => key, key => string.Empty);

            var argsContainer = rootVisualElement.Q("args");
            argsContainer.RemoveChildren();

            // context.Args.RemoveUnityArgs();

            // context.Args.Merge(inputs);

            foreach (var arg in context.Args)
            {
                if (inputs.ContainsKey(arg.Key))
                {
                    var prefsKey = $"{build.Name}_{arg.Key}";

                    var savedInput = PlayerPrefs.GetString(prefsKey, string.Empty);

                    string GetInputToArg(string input)
                    {
                        string str = string.Empty;
                        try
                        {
                            str = Smart.Format(input, context.Args);
                        }
                        catch (FormattingException e)
                        {
                            str = "formatting error! check error log";
                            Debug.LogError(e);
                        }

                        return str;
                    }

                    var value = GetInputToArg(savedInput);
                    var argComponent = new Argument(arg.Key)
                    {
                        IsInput = true,
                        IsFolder = true,
                        Input = savedInput,
                        Value = value
                    };
                    argComponent.RegisterValueChangedCallback(e =>
                    {
                        PlayerPrefs.SetString(prefsKey, e.newValue);
                        context.Args[arg.Key] = GetInputToArg(e.newValue);
                        RefreshPreview(build, context);
                    });
                    argsContainer.Add(argComponent);
                    inputs[arg.Key] = value;
                }
                else
                {
                    argsContainer.Add(new Argument(arg.Key)
                    {
                        Value = arg.Value
                    });
                }
            }

            foreach (var input in inputs)
            {
                context.Args[input.Key] = input.Value;
            }
        }
    }
}
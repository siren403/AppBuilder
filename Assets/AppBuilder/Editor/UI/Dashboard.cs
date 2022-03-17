using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
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
        }

        private Dictionary<string, MethodInfo> _buildMethods;
        private string _selectedBuild;
        private PreviewContext _context;

        private static void Load(VisualElement visualElement)
        {
            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AppBuilder/Editor/UI/Dashboard.uxml");
            VisualElement uxml = visualTree.Instantiate();
            visualElement.Add(uxml);

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

            _buildMethods = BuildPlayer.CollectMethods();

            var builds = root.Q<DropdownField>("build-field");
            builds.viewDataKey = "selected-build";
            var buildNames = _buildMethods.Keys.ToList();
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

            builds.RegisterCallback<ChangeEvent<string>>(e => { SelectBuild(e.newValue); });

            root.Q<Button>("btn-cache-clear").clicked += PlayerPrefs.DeleteAll;
            root.Q<Button>("btn-refresh").clicked += () => { RefreshPreview(); };
            root.Q<Button>("btn-run").clicked += () =>
            {
                RefreshPreview();
                EditorApplication.EnterPlaymode();
            };
            root.Q<Button>("btn-apply").clicked += () =>
            {
                if (_buildMethods.TryGetValue(_selectedBuild, out var method) && _context != null)
                {
                    _context.Args.EnableArg("CONFIGURE_ONLY", true);
                    BuildPlayer.BuildPreview(method, _context.Args);
                    _context.Args.EnableArg("CONFIGURE_ONLY", false);
                }
            };
            root.Q<Button>("btn-build").clicked += () =>
            {
                if (_buildMethods.TryGetValue(_selectedBuild, out var method) && _context != null)
                {
                    //todo: validate
                    //ex) output empty -> open folder panel
                    BuildPlayer.BuildPreview(method, _context.Args);
                }
            };

            SelectBuild(builds.value);

            root.Add(new Button(() =>
            {
                var path = PlayerPrefs.GetString("GooglePlay_outputPath", "not");
                Debug.Log(path);
                var source = new
                {
                    productName = "in"
                };
                var pattern = new Regex(@"\{(.*?)\}", RegexOptions.Multiline);
                // var pattern = new Regex(@"(?<=\{).*?(?=\})", RegexOptions.Multiline);
                var result = pattern.Matches(path);
                foreach (Match match in result)
                {
                    Debug.Log(match);
                }

                foreach (var fieldInfo in source.GetType().GetProperties())
                {
                    Debug.Log(fieldInfo.Name);
                }

                var properties = source.GetType().GetProperties()
                    .ToDictionary(p => { return p.Name; }, p => { return p.GetValue(source); });

                var replaced = pattern.Replace(path, match =>
                {
                    var key = match.Value.Substring(1, match.Value.Length - 2);
                    return properties.TryGetValue(key, out var value) ? value.ToString() : match.Value;
                });
                Debug.Log(replaced);
            })
            {
                text = "format"
            });
        }

        private void RefreshPreview(MethodInfo method = null, PreviewContext context = null)
        {
            if (method == null && !_buildMethods.TryGetValue(_selectedBuild, out method)) return;
            context ??= _context;
            if (context == null) return;

            using (BuildPlayer.Preview(method, context.Args, out var newContext))
            {
                ApplyPreview(method, newContext);
            }
        }

        private void ApplyPreview(MethodInfo method, PreviewContext context)
        {
            _context = context;

            var preview = rootVisualElement.Q("preview");
            preview.RemoveChildren();

            foreach (var property in context.Properties)
            {
                preview.Add(CreateItem(property.Name, property.Value));
            }

            #region Args

            RenderArgs(method, context);

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

        private void RenderArgs(MethodInfo method, PreviewContext context)
        {
            var inputs = method.GetCustomAttributes<InputAttribute>()
                .Select(attr => attr.Name)
                .ToDictionary(key => key, key => string.Empty);

            var argsContainer = rootVisualElement.Q("args");
            argsContainer.RemoveChildren();

            context.Args.RemoveUnityArgs();

            context.Args.Merge(inputs);

            foreach (var arg in context.Args)
            {
                if (inputs.ContainsKey(arg.Key))
                {
                    var prefsKey = $"{method.Name}_{arg.Key}";

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
                        RefreshPreview(method, context);
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

        private void SelectBuild(string buildName)
        {
            if (!_buildMethods.TryGetValue(buildName, out var buildMethod)) return;
            _selectedBuild = buildName;

            var inputs = buildMethod.GetCustomAttributes<InputAttribute>();

            string GetPrefsKey(MethodInfo method, InputAttribute arg)
            {
                return $"SAVED_{method.Name}_{arg.Name}";
            }

            var savedArgs = inputs.Select(arg =>
                {
                    return (key: arg.Name, value: PlayerPrefs.GetString(GetPrefsKey(buildMethod, arg)));
                })
                .Where(arg => !string.IsNullOrEmpty(arg.value))
                .ToDictionary(arg => arg.key, arg => arg.value);

            var buildVariants = buildMethod.GetCustomAttribute<VariantsAttribute>();

            var field = rootVisualElement.Q<DropdownField>("variant-field");
            if (buildVariants == null || !buildVariants.Keys.Any())
            {
                field.visible = false;
            }
            else
            {
                field.visible = true;
                var choices = new List<string>() {"Auto"};
                choices.AddRange(buildVariants.Keys);
                field.choices = choices;
                field.value = field.choices.First(); //todo: or default or saved

                field.RegisterValueChangedCallback(e =>
                {
                    if (_context != null)
                    {
                        var variant = e.newValue;
                        if (e.newValue.Equals("Auto"))
                        {
                            variant = buildMethod.Name;
                        }

                        _context.Args["variant"] = variant;
                        _context.Args["outputPath"] =
                            Smart.Format(PlayerPrefs.GetString($"{buildMethod.Name}_outputPath"), _context.Args);
                        RefreshPreview();
                    }
                });
                if (field.value.Equals("Auto"))
                {
                    savedArgs["variant"] = buildMethod.Name;
                }
                else
                {
                    savedArgs["variant"] = field.value;
                }
            }

            ApplyPreview(buildMethod, BuildPlayer.Preview(buildMethod, savedArgs));
        }
    }
}
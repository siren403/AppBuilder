using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using AppBuilder;
using AppBuilder.Window;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Localization.SmartFormat;
using Object = UnityEngine.Object;

namespace AppBuilder.UI
{
    public class Dashboard : EditorWindow
    {
        private static readonly List<string> NothingBuilds = new List<string>()
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

        public void CreateGUI()
        {
            void Load(VisualElement visualElement)
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

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            // VisualElement label = new Label("Hello World! From C#");
            // root.Add(label);

            Load(root);

            _buildMethods = CollectMethods();

            var builds = root.Q<DropdownField>("build-field");
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


            var btnRefresh = root.Q<Button>("btn-refresh");
            btnRefresh.clicked += () => { RefreshPreview(); };
            var btnApply = root.Q<Button>("btn-apply");
            btnApply.clicked += () =>
            {
                if (_buildMethods.TryGetValue(_selectedBuild, out var method) && _context != null)
                {
                    _context.Args["CONFIGURE_ONLY"] = string.Empty;
                    BuildPlayer.BuildPreview(method, _context.Args);
                }
            };
            var btnBuild = root.Q<Button>("btn-build");
            btnBuild.clicked += () =>
            {
                if (_buildMethods.TryGetValue(_selectedBuild, out var method) && _context != null)
                {
                    BuildPlayer.BuildPreview(method, _context.Args);
                }
            };

            SelectBuild(builds.choices.First());

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

            var args = rootVisualElement.Q("args");
            args.RemoveChildren();

            var requireArgs = method.GetCustomAttributes<ArgumentAttribute>();
            foreach (var arg in requireArgs)
            {
                var prefsKey = $"{method.Name}_{arg.Name}";

                var item = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row
                    }
                };

                var isSmart = arg.Options.HasFlag(ArgumentOptions.Smart);
                var field = new TextField(arg.Name)
                {
                    style =
                    {
                        flexGrow = 1,
                        marginRight = 0,
                        maxWidth = new StyleLength(new Length(85, LengthUnit.Percent))
                    },
                    isDelayed = true
                };
                field.Q<Label>().style.minWidth = new StyleLength(new Length(100, LengthUnit.Pixel));
                field.RegisterCallback<ChangeEvent<string>>(e =>
                {
                    var value = isSmart ? Smart.Format(e.newValue, context.Args) : e.newValue;
                    context.Args[arg.Name] = value;
                    PlayerPrefs.SetString(prefsKey, e.newValue);
                    PlayerPrefs.SetString($"SAVED_{prefsKey}", value);
                    RefreshPreview(method, context);
                });
                field.value = PlayerPrefs.GetString(prefsKey, string.Empty);
                item.Add(field);

                if (arg.Options.HasFlag(ArgumentOptions.Directory))
                {
                    var btn = new Button(() =>
                    {
                        field.value = EditorUtility.OpenFolderPanel(arg.Name, Directory.GetCurrentDirectory(),
                            string.Empty);
                        RefreshPreview(method, context);
                    })
                    {
                        style =
                        {
                            backgroundImage = new StyleBackground(
                                Background.FromTexture2D(
                                    EditorGUIUtility.IconContent("d_Folder Icon").image as Texture2D)),
                            width = 20
                        }
                    };
                    item.Add(btn);
                }

                if (isSmart)
                {
                    item.Add(new Label("Smart"));
                }

                args.Add(item);
            }

            #endregion

            #region Reserved Args

            var reservedArgs = rootVisualElement.Q("reserved-args");
            reservedArgs.RemoveChildren();

            foreach (var arg in context.Args.WhereReserve())
            {
                reservedArgs.Add(CreateItem(arg.Key, arg.Value));
            }

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

        private void SelectBuild(string buildName)
        {
            if (!_buildMethods.TryGetValue(buildName, out var buildMethod)) return;
            _selectedBuild = buildName;

            var buildArgs = buildMethod.GetCustomAttributes<ArgumentAttribute>();

            string GetPrefsKey(MethodInfo method, ArgumentAttribute arg)
            {
                return $"SAVED_{method.Name}_{arg.Name}";
            }

            var savedArgs = buildArgs.Select(arg =>
                {
                    return (key: arg.Name, value: PlayerPrefs.GetString(GetPrefsKey(buildMethod, arg)));
                })
                .Where(arg => !string.IsNullOrEmpty(arg.value))
                .ToDictionary(arg => arg.key, arg => arg.value);

            ApplyPreview(buildMethod, BuildPlayer.Preview(buildMethod, savedArgs));
        }

        private Dictionary<string, MethodInfo> CollectMethods()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(_ => _.GetReferencedAssemblies().Any(_ => _.Name == "AppBuilder.Editor"))
                .SelectMany(_ =>
                {
                    return _.GetTypes()
                        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                        .Where(m => m.GetCustomAttribute<BuildAttribute>() != null);
                }).ToDictionary(info => info.Name);
        }

        private void Build()
        {
        }
    }
}
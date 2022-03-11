using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.Window
{
    public class BuildsWindow : EditorWindow
    {
        private Dictionary<string, MethodInfo> _buildMethods;
        private int _selectedBuildIndex = 0;
        private string _buildPreview;
        private (string item, string message)[] _buildMessages;

        [MenuItem("Builds/Dashboard")]
        private static void ShowWindow()
        {
            var window = GetWindow<BuildsWindow>();
            window.titleContent = new GUIContent("Builds");
            window.Show();
        }

        private void CreateGUI()
        {
            //layout
            rootVisualElement.Add(new VisualElement()
            {
                name = "builds"
            });
            rootVisualElement.Add(new VisualElement()
            {
                name = "preview"
            });
            rootVisualElement.Add(new VisualElement()
            {
                name = "args",
                style =
                {
                    marginTop = 20
                }
            });
            var info = new VisualElement();
            info.Add(new Button(() => { Debug.Log(Directory.GetCurrentDirectory()); })
            {
                text = "Current Directory"
            });
            rootVisualElement.Add(info);
            //update
            CollectMethods();

            var builds = rootVisualElement.Q("builds");
            foreach (var method in _buildMethods)
            {
                var btn = new Button(() => { PreviewBuild(method.Key); })
                {
                    name = method.Key,
                    text = method.Key
                };
                builds.Add(btn);

                var argAttrs = method.Value.GetCustomAttributes<ArgumentAttribute>().ToArray();
                if (argAttrs.Any())
                {
                    var args = rootVisualElement.Q("args");
                    args.RemoveChildren();
                    foreach (var attr in argAttrs)
                    {
                        var item = new VisualElement()
                        {
                            name = attr.Name,
                            style =
                            {
                                flexDirection = FlexDirection.Row,
                                justifyContent = Justify.SpaceBetween
                            }
                        };
                        item.Add(new Label(attr.Name));
                        var field = new TextField()
                        {
                            value = EditorPrefs.GetString(attr.Name, ""),
                            style =
                            {
                                minWidth = 300
                            }
                        };
                        field.RegisterCallback<ChangeEvent<string>>((e) =>
                        {
                            Debug.Log(e.newValue);
                            EditorPrefs.SetString(item.name, e.newValue);
                            PreviewBuild(method.Key);
                        });
                        field.isDelayed = true;
                        item.Add(field);

                        args.Add(item);
                    }
                }
            }


            if (_buildMethods.Any())
            {
                PreviewBuild(_buildMethods.First().Key);
            }
        }

        private void CollectMethods()
        {
            // var executingAssembly = Assembly.GetExecutingAssembly();
            // foreach (var type in executingAssembly.GetTypes())
            // {
            //     foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            //     {
            //         Debug.Log(method.Name);
            //     }
            // }
            //
            // Debug.Log("-------------------------");
            // var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            //
            // foreach (var assembly in assemblies)
            // {
            //     if (assembly.assemblyReferences.Any(_ => _.name == "AppBuilder"))
            //     {
            //         Debug.Log($"ref: {assembly.name}");
            //     }
            // }

            _buildMethods = AppDomain.CurrentDomain.GetAssemblies()
                .Where(_ => _.GetReferencedAssemblies().Any(_ => _.Name == "AppBuilder"))
                .SelectMany(_ =>
                {
                    return _.GetTypes()
                        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                        .Where(m => m.GetCustomAttribute<BuildAttribute>() != null);
                }).ToDictionary(info => info.Name);
        }

        private void PreviewBuild(string buildName)
        {
            if (!_buildMethods.TryGetValue(buildName, out var method)) return;
            var container = rootVisualElement.Q("preview");
            container.RemoveChildren();
            Debug.Log("remove");

            if (!BuildPlayer.Preview(method, out var preview, out var messages))
            {
                return;
            }

            foreach (var (item, msg) in messages)
            {
                container.Add(CreateItem(item, msg));
            }

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
                itemContainer.Add(new Label(text));
                return itemContainer;
            }


            container.Add(new Button(() =>
            {
                method.Invoke(null, null);
            })
            {
                text = "Build"
            });
        }
    }

    public static class BuildPlayer
    {
        public static bool Preview(MethodInfo method, out string preview, out (string, string)[] messages)
        {
            using (AppBuilder.BuildPlayer.Preview())
            {
                method.Invoke(null, null);
                preview = AppBuilder.BuildPlayer.BuildPreview;
                messages = AppBuilder.BuildPlayer.BuildMessages;
            }

            return !string.IsNullOrEmpty(preview) && messages != null;
        }
    }

    public static class VisualElementExtensions
    {
        public static void RemoveChildren(this VisualElement element)
        {
            for (int i = element.childCount - 1; i >= 0; i--)
            {
                element.RemoveAt(i);
            }
        }
    }
}
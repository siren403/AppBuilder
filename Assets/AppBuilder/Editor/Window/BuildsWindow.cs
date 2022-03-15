using System;
using System.Collections.Generic;
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
        private PreviewContext _context;

        [MenuItem("AppBuilder/Builds")]
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
            // rootVisualElement.Add(new VisualElement()
            // {
            //     name = "reserved",
            //     style =
            //     {
            //         marginTop = 10
            //     }
            // });
            // var info = new VisualElement();
            // info.Add(new Button(() => { Debug.Log(Directory.GetCurrentDirectory()); })
            // {
            //     text = "Current Directory"
            // });
            // rootVisualElement.Add(info);
            //update
            CollectMethods();

            #region Builds

            var builds = rootVisualElement.Q("builds");
            foreach (var method in _buildMethods)
            {
                var btn = new Button(() => { PreviewBuild(method.Key); })
                {
                    name = method.Key,
                    text = method.Key
                };
                builds.Add(btn);

                // var argAttrs = method.Value.GetCustomAttributes<ArgumentAttribute>().ToArray();
                // if (argAttrs.Any())
                // {
                //     var args = rootVisualElement.Q("args");
                //     args.RemoveChildren();
                //     foreach (var attr in argAttrs)
                //     {
                //         var item = new VisualElement()
                //         {
                //             name = attr.Name,
                //             style =
                //             {
                //                 flexDirection = FlexDirection.Row,
                //                 justifyContent = Justify.SpaceBetween
                //             }
                //         };
                //         item.Add(new Label(attr.Name));
                //         var field = new TextField()
                //         {
                //             value = PlayerPrefs.GetString(attr.Name, ""),
                //             style =
                //             {
                //                 minWidth = 300
                //             }
                //         };
                //         field.RegisterCallback<ChangeEvent<string>>((e) =>
                //         {
                //             Debug.Log(e.newValue);
                //             PlayerPrefs.SetString(item.name, e.newValue);
                //             PreviewBuild(method.Key);
                //         });
                //         field.isDelayed = true;
                //         item.Add(field);
                //
                //         args.Add(item);
                //     }
                // }
            }

            #endregion


            if (_buildMethods.Any())
            {
                PreviewBuild(_buildMethods.First().Key);
            }
        }

        private void CollectMethods()
        {
            _buildMethods = AppDomain.CurrentDomain.GetAssemblies()
                .Where(_ => _.GetReferencedAssemblies().Any(_ => _.Name == "AppBuilder.Editor"))
                .SelectMany(_ =>
                {
                    return _.GetTypes()
                        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                        .Where(m => m.GetCustomAttribute<BuildAttribute>() != null);
                }).ToDictionary(info => info.Name);

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
        }

        private void PreviewBuild(string buildName)
        {
            if (!_buildMethods.TryGetValue(buildName, out var method)) return;
            var container = rootVisualElement.Q("preview");
            container.RemoveChildren();

            var context = BuildPlayer.Preview(method);

            foreach (var property in context.Properties)
            {
                container.Add(CreateItem(property.Name, property.Value));
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

            #region Args

            var args = rootVisualElement.Q("args");
            args.RemoveChildren();

            foreach (var arg in context.Args.WhereReserve())
            {
                args.Add(CreateItem(arg.Key, arg.Value.Replace("\\","\\\\")));
            }

            #endregion
        }
    }

    // public static class BuildPlayer
    // {
    //     public static bool Preview(MethodInfo method, out BuildProperty[] properties)
    //     {
    //         using (AppBuilder.BuildPlayer.Preview(method, out var context))
    //         {
    //             properties = context.Properties;
    //         }
    //
    //         return properties != null;
    //     }
    // }

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
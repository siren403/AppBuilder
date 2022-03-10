using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AppBuilder.Window
{
    public class BuildsWindow : EditorWindow
    {
        private MethodInfo[] _buildMethods;
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
                }).ToArray();
        }

        private void OnEnable()
        {
            CollectMethods();
            if (_buildMethods.Any())
            {
                _selectedBuildIndex = 0;
                PreviewBuild(_buildMethods[0]);
            }
        }

        private void OnFocus()
        {
        }

        private void OnGUI()
        {
            if (_buildMethods != null)
            {
                DrawBuilds();
                DrawSelectedBuild();
            }
        }

        private void DrawBuilds()
        {
            for (int i = 0; i < _buildMethods.Length; i++)
            {
                var method = _buildMethods[i];
                if (GUILayout.Button(method.Name))
                {
                    _selectedBuildIndex = i;
                    PreviewBuild(method);
                }
            }
        }


        private void DrawSelectedBuild()
        {
            if (_selectedBuildIndex >= _buildMethods.Length) return;
            if (string.IsNullOrEmpty(_buildPreview)) return;
            if (_buildMessages == null) return;

            // GUILayout.Label(_buildPreview);
            foreach (var tuple in _buildMessages)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(tuple.item);

                GUILayout.Label(tuple.message, new GUIStyle()
                {
                    normal = new GUIStyleState()
                    {
                        textColor = Color.white
                    },
                    padding = new RectOffset()
                    {
                        right = 10
                    },
                    alignment = TextAnchor.MiddleRight
                });
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Build"))
            {
                ExecuteBuild(_buildMethods[_selectedBuildIndex]);
            }
        }

        private void PreviewBuild(MethodInfo method)
        {
            using (BuildPlayer.Preview())
            {
                method.Invoke(null, null);
                _buildPreview = BuildPlayer.BuildPreview;
                _buildMessages = BuildPlayer.BuildMessages;
            }
        }

        private void ExecuteBuild(MethodInfo method)
        {
            method.Invoke(null, null);
        }
    }
}
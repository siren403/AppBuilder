using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;
using PackageInfo = AppBuilder.UI.PackageInfo;

namespace AppBuilder
{
    public interface IUnityPlayerBuilder
    {
        string ProjectName { get; }
        string[] Scenes { set; }
        string OutputPath { set; }
        string ProductName { set; }
        BuildTarget Target { set; get; }
        BuildTargetGroup TargetGroup { set; }
        void ConfigureAndroid(Action<AndroidConfigureBuilder> configuration);
        void Display(params (string key, string value)[] pairs);
        ConfigureSection Display(out Action<string, string> add);

        void ApplyPreset(string presetPath, string targetPath);
    }


    public static class UnityPlayerBuilderExtensions
    {
        public static void UseEnableEditorScenes(this IUnityPlayerBuilder builder)
        {
            builder.Scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }


        /// <summary>
        /// * Scenes - Use Enable Editor Scenes
        /// * Target - active Build Target
        /// </summary>
        public static void UseCurrentEditorSettings(this IUnityPlayerBuilder builder)
        {
            builder.UseEnableEditorScenes();
            builder.Target = EditorUserBuildSettings.activeBuildTarget;
            builder.TargetGroup = BuildPipeline.GetBuildTargetGroup(builder.Target);
        }

        public static void UseVariantProductName(this IUnityPlayerBuilder builder, IBuildContext context,
            string productName = null, string format = null)
        {
            if (string.IsNullOrEmpty(productName))
            {
                context.TryGetSection("ProductName", out productName);
            }

            context.TryGetArgument("variant", out var variant);

            if (!string.IsNullOrEmpty(productName) && !string.IsNullOrEmpty(variant))
            {
                if (string.IsNullOrEmpty(format))
                {
                    format = "{0}-{1}";
                }

                builder.ProductName = string.Format(format, productName, variant);
            }
            else if (!string.IsNullOrEmpty(variant))
            {
                builder.ProductName = variant;
            }
            else
            {
                builder.ProductName = productName;
            }
        }

        public static void ResetPlayerSettings(this IUnityPlayerBuilder builder)
        {
            var presetPath =
                PackageInfo.GetPath($"Editor/Presets/PlayerSettings/{Application.unityVersion}/PlayerSettings.preset");
            var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
            if (preset == null)
            {
                presetPath = PackageInfo.GetPath($"Editor/Presets/PlayerSettings/PlayerSettings.preset");
            }

            builder.ApplyPreset(presetPath, "ProjectSettings/ProjectSettings.asset");
        }
    }
}
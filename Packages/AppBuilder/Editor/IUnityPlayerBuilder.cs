using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AppBuilder
{
    public interface IUnityPlayerBuilder
    {
        string[] Scenes { set; }
        string OutputPath { set; }
        string ProductName { set; }
        BuildTarget Target { set; get; }
        BuildTargetGroup TargetGroup { set; }
        void ConfigureAndroid(Action<AndroidConfigureBuilder> configuration);
        void Display(params (string key, string value)[] pairs);
        ConfigureSection Display(out Action<string, string> add);
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
        public static void ConfigureCurrentSettings(this IUnityPlayerBuilder builder)
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
    }
}
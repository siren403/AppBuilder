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
        void ConfigureAndroid(Action<AndroidSettingsBuilder> configuration);
    }

    public static class UnityPlayerBuilderExtensions
    {
        public static void UsingEnableEditorScenes(this IUnityPlayerBuilder builder)
        {
            builder.Scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }

        public static void ConfigureCurrentSettings(this IUnityPlayerBuilder builder)
        {
            builder.UsingEnableEditorScenes();
            builder.Target = EditorUserBuildSettings.activeBuildTarget;
            builder.TargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            builder.OutputPath = Path.Combine(Directory.GetCurrentDirectory(), builder.Target.ToString(),
                Application.productName);
        }

    }
}
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
        /// * OutputPath - {CurrentDirectory}/Build/{Target}/{productName}
        /// </summary>
        public static void ConfigureCurrentSettings(this IUnityPlayerBuilder builder)
        {
            builder.UseEnableEditorScenes();
            builder.Target = EditorUserBuildSettings.activeBuildTarget;
            builder.TargetGroup = BuildPipeline.GetBuildTargetGroup(builder.Target);
        }
    }
}
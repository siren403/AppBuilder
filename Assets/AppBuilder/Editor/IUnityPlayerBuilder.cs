using System;
using System.Linq;
using UnityEditor;

namespace AppBuilder
{
    public interface IUnityPlayerBuilder
    {
        string[] Scenes { set; }
        string OutputPath { set; }
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
    }
}
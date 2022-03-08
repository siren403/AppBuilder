using System;
using System.Linq;
using UnityEditor;

namespace AppBuilder
{
    public interface IUnityPlayerBuilder
    {
        string[] Scenes { set; }
        string OutPutDirectory { set; }
        IOptions<TConfig> Configure<TConfig>(string settingsDirectory) where TConfig : class;

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
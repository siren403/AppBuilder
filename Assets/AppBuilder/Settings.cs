using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace AppBuilder
{
    public static partial class BuildPlayer
    {
        public static void Build(Action<IUnityPlayerBuilder> configuration)
        {
            var args = Environment.GetCommandLineArgs();
            var builder = new UnityPlayerBuilder(args);
            configuration(builder);

            var options = builder.BuildOptions;
            try
            {
                options.Validate();
            }
            catch (Exception e)
            {
                Debug.Log($"[AppBuilder] Build Failed {e.Message}");
                EditorApplication.Exit(0);
            }

            Debug.Log($"[AppBuilder] {JsonConvert.SerializeObject(options)}");

            if (!Directory.Exists(builder.OutPutDirectory))
            {
                Directory.CreateDirectory(builder.OutPutDirectory);
            }
            var report = BuildPipeline.BuildPlayer(builder.BuildOptions);
            //todo: BatchMode -> Revert? builder.Revert()
        }
    }

    public static class BuildPlayerOptionsExtensions
    {
        public static void Validate(this BuildPlayerOptions options)
        {
            if (options.scenes == null)
            {
                throw new ValidationException("empty build scenes");
            }
        }
    }
}
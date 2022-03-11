using System;
using System.IO;
using System.Text;
using AppBuilder;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Builds
{
    [Serializable]
    public class AppSettings
    {
        public string Host { get; set; }
        public string[] Scenes { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"host: {Host}");
            return builder.ToString();
        }
    }

    public static partial class Android
    {
        [Build]
        [Argument("customBuildPath")]
        public static void GooglePlay()
        {
            BuildPlayer.Build(builder =>
            {
                // builder.OutPutDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Build");

                var settingsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Builds");
                var config = builder.Configure<AppSettings>(settingsDirectory).Value;

                if (config.Scenes != null)
                {
                    builder.Scenes = config.Scenes;
                }
                else
                {
                    builder.UsingEnableEditorScenes();
                }

                builder.ConfigureAndroid(_ =>
                {
                    _.IL2CPP();
                    _.Architectures(AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64);
                    _.SupportEmulator();
                });
            });
        }
    }
}
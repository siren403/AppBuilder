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
        public string host;
        public string[] scenes;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"host: {host}");
            return builder.ToString();
        }
    }

    public static partial class Android
    {
        [Build]
        public static void GooglePlay()
        {
            BuildPlayer.Build(builder =>
            {
                builder.OutPutDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Build");

                var settingsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Builds");
                var config = builder.Configure<AppSettings>(settingsDirectory).Value;

                if (config.scenes != null)
                {
                    builder.Scenes = config.scenes;
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
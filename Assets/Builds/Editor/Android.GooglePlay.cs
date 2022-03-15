using System.IO;
using System.Linq;
using AppBuilder;
using UnityEditor;
using UnityEngine;

namespace Builds
{
    public static partial class Android
    {
        [Build]
        [Argument("outputPath", ArgumentOptions.Directory | ArgumentOptions.Smart)]
        [Variant("Development")]
        public static void GooglePlay()
        {
            BuildPlayer.Build((ctx, builder) =>
            {
                ctx.GetConfiguration<AppSettings>()
                    .WriteScriptable("AppSettings");

                builder.OutputPath = ctx.GetArgument("outputPath",
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        BuildTarget.Android.ToString(),
                        Application.productName)
                );

                var host = ctx.GetSection<string>("Host");
                var scenes = ctx.GetSections<string>("Scenes").ToArray();

                if (scenes.Any()) builder.Scenes = scenes;
                else builder.UsingEnableEditorScenes();

                builder.ConfigureAndroid(_ =>
                {
                    _.PackageName(ctx.GetSection<string>("Package"));
                    _.IL2CPP();
                    _.Architectures(AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64);
                    _.SupportEmulator();
                });
            }, false);
        }
    }
}
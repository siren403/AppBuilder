using System.Linq;
using AppBuilder;
using UnityEditor;

namespace Builds
{
    public static partial class Android
    {
        [Build]
        [Input("variant", new[] {"GooglePlay", "GooglePlay.dev"})]
        [Input("outputPath", InputOptions.Directory)]
        [InputFile("keystore", "keystore")]
        [Input("keystore.passwd")]
        [InputString("keystore.alias", "alias")]
        [Input("keystore.alias.passwd")]
        public static void GooglePlay()
        {
            BuildPlayer.Build(args =>
            {
                args.Add("arg1", "test");
                args.Add("arg2", "test2");
            }, (ctx, builder) =>
            {
                ctx.GetConfiguration<AppSettings>()
                    .WriteScriptable("AppSettings");


                builder.OutputPath = ctx.GetArgument("outputPath");
                builder.ProductName = $"{ctx.GetSection<string>("ProductName")}-{ctx.GetArgument("variant")}";

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
                    _.EnableKeystore(ctx.TryGetArgument("keystore", out var keystorePath),
                        keystorePath,
                        ctx.GetArgument("keystore.passwd"),
                        ctx.GetArgument("keystore.alias"),
                        ctx.GetArgument("keystore.alias.passwd"));
                });
            });
        }
    }
}
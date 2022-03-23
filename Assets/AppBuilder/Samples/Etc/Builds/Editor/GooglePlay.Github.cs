using AppBuilder;
using UnityEditor;

namespace Builds
{
    public static class GooglePlay
    {
        // [Build]
        [Input("customBuildPath", InputOptions.Directory)]
        [AppSettings("{projectpath}/Assets/AppBuilder/Samples/Etc/Builds")]
        public static void Github()
        {
            BuildPlayer.Build((ctx, builder) =>
            {
                ctx.GetConfiguration<AppSettings>()
                    .WriteScriptable("AppSettings");

                builder.OutputPath = ctx.GetArgument("customBuildPath", string.Empty);

                builder.UseEnableEditorScenes();

                builder.ConfigureAndroid(_ =>
                {
                    _.PackageName(ctx.GetSection<string>("Package"));
                    _.IL2CPP();
                    _.Architectures(AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64);
                    _.SupportEmulator();
                });
            });
        }
    }
}
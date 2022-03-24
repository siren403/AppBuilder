using UnityEditor;

namespace AppBuilder.Samples
{
    public static partial class Builds
    {
        [Build("Basic Android", -4)]
        [File("keystore", "{projectPath}/Assets/Samples/AppBuilder/0.0.1/Basic Android/user.keystore", "keystore")]
        [Input("keystore.passwd", "111111")]
        [Input("keystore.alias", "appbuilder")]
        [Input("keystore.alias.passwd","111111")]
        public static void BasicAndroid()
        {
            BuildPlayer.Build((ctx, builder) =>
            {
                
                builder.UseEnableEditorScenes();
                builder.ConfigureAndroid(android =>
                {
                    android.IL2CPP();
                    android.Architectures(AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64);
                    android.SupportEmulator();
                    android.EnableKeystore(ctx.TryGetArgument("keystore", out var keystorePath),
                        keystorePath,
                        ctx.GetArgument("keystore.passwd"),
                        ctx.GetArgument("keystore.alias"),
                        ctx.GetArgument("keystore.alias.passwd"));
                });
            });
        }
    }
}
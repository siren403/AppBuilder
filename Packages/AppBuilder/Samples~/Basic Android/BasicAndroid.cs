using AppBuilder;
using AppBuilder.Helper.Android;
using KeyStore = AppBuilder.Helper.Android.KeyStore;
using UnityEditor;
using PackageInfo = AppBuilder.UI.PackageInfo;

public static partial class Builds
{
    [Build("Basic Android", -4)]
    [AppSettings("{samples}/Basic Android")]
    [KeyStore("{samples}/Basic Android/user.keystore")]
    [KeyStore.Password("111111")]
    [KeyStore.Alias("appbuilder")]
    [KeyStore.Alias.Password("111111")]
    public static void BasicAndroid()
    {
        BuildPlayer.Build((args) =>
        {
            args.Add("samples", PackageInfo.SamplesPath);
        }, (ctx, builder) =>
        {
            builder.Display(("packageName",ctx.GetSection<string>("PackageName")));
            builder.UseEnableEditorScenes();
            builder.ConfigureAndroid(android =>
            {
                android.IL2CPP();
                android.Architectures(AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64);
                android.SupportEmulator();
                android.UseKeyStore(ctx);
            });
        });
    }
}

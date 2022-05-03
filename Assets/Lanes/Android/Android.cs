using AppBuilder;
using AppBuilder.Deploy;

public static class Android
{
    [Build(nameof(Android))]
    [AppSettings("Assets/Lanes/Android")]
    [Variant("Debug", "Release")]
    public static void Build()
    {
        BuildPlayer.Build((ctx, builder) => builder.UseDeploy(ctx));
    }
}
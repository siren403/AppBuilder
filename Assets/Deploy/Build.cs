using AppBuilder;
using Samples.Deploy.Editor;
using UnityEditor;

public static class Build
{
    [AppSettings("Assets/Deploy/Android")]
    [Variant("Apk")]
    [Build]
    public static void Android()
    {
        BuildPlayer.Build((ctx, builder) => { builder.UseDeploy(ctx); });
    }

    [AppSettings("Assets/Deploy/iOS")]
    [Build]
    public static void iOS()
    {
        
    }
}
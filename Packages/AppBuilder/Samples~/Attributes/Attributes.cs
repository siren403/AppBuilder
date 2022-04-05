using AppBuilder;
using UnityEditor;
using UnityEngine;
using PackageInfo = AppBuilder.UI.PackageInfo;

namespace AppBuilderSample
{
    public static class Attributes
    {
        [Build("AppBuilder.Samples.Attributes", -2)]
        [AppSettings("Assets/Samples/AppBuilder/0.0.1/Attributes/AppSettings")]
        [Variant("Development", "Production", "GooglePlay", "GooglePlay.Dev")]
        [Directory("outputPath")]
        [File("keystore", "keystore")]
        [Input("keystore.passwd")]
        [Input("keystore.alias", "alias")]
        [Input("keystore.alias.passwd")]
        public static void Build()
        {
            BuildPlayer.Build((ctx, builder) => { builder.ConfigureCurrentSettings(); });
        }
    }
}
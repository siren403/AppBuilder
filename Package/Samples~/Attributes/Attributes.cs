using AppBuilder;
using UnityEditor;
using UnityEngine;
using PackageInfo = AppBuilder.UI.PackageInfo;

namespace AppBuilderSamples
{
    public static partial class Builds
    {
        [Build("AppBuilder.Samples.Attributes", -2)]
        [AppSettings("Assets/Samples/AppBuilder/0.0.1/Attributes/AppSettings")]
        [Variant("Development", "Production", "GooglePlay", "GooglePlay.Dev")]
        [Directory("outputPath")]
        [File("keystore", "keystore")]
        [Input("keystore.passwd")]
        [Input("keystore.alias", "alias")]
        [Input("keystore.alias.passwd")]
        public static void Attributes()
        {
            BuildPlayer.Build((ctx, builder) => { builder.ConfigureCurrentSettings(); });
        }
    }
}
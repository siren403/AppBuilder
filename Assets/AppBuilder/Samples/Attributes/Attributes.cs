namespace AppBuilder.Samples
{
    public static partial class Builds
    {
        [Build("AppBuilder.Samples.Attributes", -2)]
        [AppSettings("{projectpath}/Assets/AppBuilder/Samples/AppSettings")]
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
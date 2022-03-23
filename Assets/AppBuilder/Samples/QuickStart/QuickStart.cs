namespace AppBuilder.Samples
{
    public static partial class Builds
    {
        [Build(order: -1)]
        public static void QuickStart()
        {
            BuildPlayer.Build((ctx, builder) => { builder.ConfigureCurrentSettings(); });
        }
    }
}
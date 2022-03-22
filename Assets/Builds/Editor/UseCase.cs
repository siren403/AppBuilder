using AppBuilder;

namespace Builds
{
    public static class UseCase
    {
        [Build]
        public static void QuickStart()
        {
            BuildPlayer.Build((ctx, builder) => { builder.ConfigureCurrentSettings(); });
        }

        [Build]
        [AppBuilder.AppSettings]
        public static void UsingAppSettingsJson()
        {
            BuildPlayer.Build((ctx, builder) =>
            {
                builder.ConfigureCurrentSettings();
                
            });
        }
    }
}
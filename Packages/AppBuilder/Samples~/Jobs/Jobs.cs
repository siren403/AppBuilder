using AppBuilder;
using AppBuilder.ConsoleApp;
using UnityEditor;
using UnityEngine;
using PackageInfo = AppBuilder.UI.PackageInfo;

namespace AppBuilderSample
{
    public static class UnityPlayerBuilderExtensions
    {
        public static void DefaultSettings(this IUnityPlayerBuilder builder)
        {
            builder.ProductName = builder.ProjectName;
            builder.UseEnableEditorScenes();
        }

        public static void AndroidDefaultSettings(this IUnityPlayerBuilder builder)
        {
            builder.ConfigureAndroid(android =>
            {
                android.UseDebugKeystore();
                android.IL2CPP();
                android.SupportEmulator();
            });
        }

        public static void Default(this IUnityPlayerBuilder builder, BuildTarget buildTarget)
        {
            builder.DefaultSettings();
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    builder.AndroidDefaultSettings();
                    break;
            }
        }
    }

    public static class Jobs
    {
        private class LocalFtpProfile : FtpConsole.IProfile
        {
            public string Host => "127.0.0.1";
            public string User => "appbuilder";
            public string Password => "0000";
        }

        private class Profile1 : JsonProfile
        {
            public Profile1() : base(
                AssetDatabase.LoadAssetAtPath<TextAsset>($"{PackageInfo.SamplesPath}/Jobs/profile-1.json").text)
            {
            }
        }

        private class BlueStackProfile : IEmulatorProfile
        {
            public string Name => "127.0.0.1:5585";
        }

        [Build(nameof(Jobs))]
        [Job(typeof(UploadFtpJob<Profile1>))]
        [Job(typeof(InstallEmulatorJob<BlueStackProfile>))]
        public static void Build()
        {
            BuildPlayer.Build((ctx, builder) =>
            {
                builder.ResetPlayerSettings();
                builder.UseCurrentEditorSettings();
            });
        }
    }
}
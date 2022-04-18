using System.Linq;
using AppBuilder;
using UnityEditor;

namespace Samples.Deploy.Editor
{
    public class AndroidConfig
    {
        public string PackageName;
        public int VersionCode = 1;

        public bool IL2CPP = true;

        public AndroidArchitecture[] Architectures = new[]
        {
            AndroidArchitecture.ARMv7
        };

        public AndroidKeystore Keystore;

        public bool SupportEmulator = true;
    }

    public class AndroidKeystore
    {
        public string Path;
        public string Password;
        public string Alias;
        public string AliasPassword;
    }

    public class iOSConfig
    {
        public string PackageName;
    }

    public static class UnityPlayerBuilderExtensions
    {
        public static void UseDeploy(this IUnityPlayerBuilder builder, IBuildContext context)
        {
            builder.ResetPlayerSettings();

            builder.CompanyName = context.TryGetSection("CompanyName", out string company) ? company : null;
            builder.ProductName =
                context.TryGetSection("ProductName", out string product) ? product : builder.ProjectName;

            if (context.TryGetSection("Version", out string version))
            {
                builder.Version = version;
            }

            builder.UseEnableEditorScenes();
            if (context.TryGetSection("Android", out AndroidConfig androidConfig))
            {
                builder.ConfigureAndroid(android =>
                {
                    android.PackageName(androidConfig.PackageName);

                    android.VersionCode = androidConfig.VersionCode;

                    if (androidConfig.IL2CPP) android.IL2CPP();
                    else android.Mono();

                    AndroidArchitecture architecture = androidConfig.Architectures.Aggregate(AndroidArchitecture.None,
                        (acc, current) => acc | current);
                    android.Architectures(architecture);

                    android.EnableKeystore(androidConfig.Keystore != null,
                        androidConfig.Keystore?.Path,
                        androidConfig.Keystore?.Password,
                        androidConfig.Keystore?.Alias,
                        androidConfig.Keystore?.AliasPassword
                    );

                    if (androidConfig.SupportEmulator)
                    {
                        android.SupportEmulator();
                    }

                    android.EnableAppBundle(true);
                });
            }
            else if (context.TryGetSection("iOS", out iOSConfig iosConfig))
            {
            }
        }
    }
}
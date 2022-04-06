using System.IO;
using UnityEditor;

namespace AppBuilder
{
    public static class PlaybackEngines
    {
        public static string Directory
        {
            get
            {
#if UNITY_EDITOR_OSX
                return Path.Combine(Path.GetDirectoryName(EditorApplication.applicationPath) ?? string.Empty,
                    "PlaybackEngines");
#else
                return Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines");
#endif
            }
        }

        public static string Android => Path.Combine(Directory, "AndroidPlayer");
        
        public static string AdbPath => Path.Combine(Android, "SDK", "platform-tools", "adb.exe");

        // ReSharper disable once InconsistentNaming
        public static string iOS => Path.Combine(Directory, "iOSSupport");
    }
}
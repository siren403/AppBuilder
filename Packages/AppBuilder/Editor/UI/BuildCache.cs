using UnityEngine;

namespace AppBuilder.UI
{
    public static class BuildCache
    {
        public static string GetKey(this BuildInfo build, string key) => $"{build.FullName}_{key}";

        public static void SetString(BuildInfo build, string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                PlayerPrefs.DeleteKey(build.GetKey(key));
            }
            else
            {
                PlayerPrefs.SetString(build.GetKey(key), value);
            }
        }

        public static string GetString(BuildInfo build, string key, string defaultValue = null)
        {
            return PlayerPrefs.GetString(build.GetKey(key), defaultValue);
        }

        public static void SetString(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                PlayerPrefs.DeleteKey($"{nameof(AppBuilder)}_{key}");
            }
            else
            {
                PlayerPrefs.SetString($"{nameof(AppBuilder)}_{key}", value);
            }
        }

        public static string GetString(string key, string defaultValue = null)
        {
            return PlayerPrefs.GetString($"{nameof(AppBuilder)}_{key}", defaultValue);
        }
    }
}
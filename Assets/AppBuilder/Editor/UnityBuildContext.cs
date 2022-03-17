using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AppBuilder
{
    public class UnityBuildContext : IBuildContext
    {
        public readonly Dictionary<string, string> Args;
        private readonly JObject _appSettings;

        public UnityBuildContext(Dictionary<string, string> args)
        {
            Args = args;
            var variant = args.ContainsKey("variant") ? args["variant"] : string.Empty;
            _appSettings = LoadAppSettings(AppSettingsDirectory, variant);
        }

        private JObject LoadAppSettings(string directory, string variant = null)
        {
            var baseSettingsPath = Path.Combine(directory, "appsettings.json");

            JObject settings = null;

            if (File.Exists(baseSettingsPath))
            {
                using var baseSettingReader = new StreamReader(baseSettingsPath);
                settings = JObject.Parse(baseSettingReader.ReadToEnd());
            }

            if (!string.IsNullOrEmpty(variant))
            {
                var overwriteSettingsPath = Path.Combine(directory, $"appsettings.{variant}.json");
                if (File.Exists(overwriteSettingsPath))
                {
                    using var overwriteSettingsReader = new StreamReader(overwriteSettingsPath);
                    var overwriteSettings = JObject.Parse(overwriteSettingsReader.ReadToEnd());

                    if (settings == null)
                    {
                        settings = overwriteSettings;
                    }
                    else
                    {
                        settings.Merge(overwriteSettings);
                    }
                }
            }

            return settings ?? new JObject();
        }

        public string AppSettingsDirectory
        {
            get
            {
                //todo: args, player prefs
                return Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Builds");
            }
        }

        public IOptions<T> GetConfiguration<T>() where T : class
        {
            return new JObjectProvider<T>(_appSettings);
        }

        public IOptions GetConfiguration()
        {
            return new JObjectProvider(_appSettings);
        }

        public T GetSection<T>(string key)
        {
            if (_appSettings.TryGetValue(key, out var token))
            {
                return token.Value<T>();
            }

            return default;
        }

        public IEnumerable<T> GetSections<T>(string key)
        {
            if (_appSettings.TryGetValue(key, out var token))
            {
                return token.Values<T>();
            }

            return Array.Empty<T>();
        }

        public string GetArgument(string key, string defaultValue = null)
        {
            if (Args.TryGetValue(key, out var arg))
            {
                return arg;
            }

            return defaultValue ?? string.Empty;
        }
    }
}
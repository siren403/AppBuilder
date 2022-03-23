using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AppBuilder
{
    public class UnityBuildContext : IBuildContext
    {
        private static string DefaultAppSettingsDirectory =>
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Builds");

        public readonly Arguments Args;
        private readonly JObject _appSettings;

        public UnityBuildContext(Arguments args)
        {
            Args = args;
            var variant = args.ContainsKey("variant") ? args["variant"] : string.Empty;
            variant = variant switch
            {
                "None" => string.Empty,
                // "Auto" => args.TryGetValue("BuildName", out var buildName) ? buildName : string.Empty,
                _ => variant
            };
            _appSettings = LoadAppSettings(GetAppSettingsDirectory(args), variant);
        }

        private JObject LoadAppSettings(string directory, string variant = null)
        {
            JObject settings = null;

            if (string.IsNullOrEmpty(directory)) return new JObject();

            var baseSettingsPath = Path.Combine(directory, "appsettings.json");

            if (File.Exists(baseSettingsPath))
            {
                using var baseSettingReader = new StreamReader(baseSettingsPath);
                settings = JObject.Parse(baseSettingReader.ReadToEnd());
            }
            else
            {
                Debug.LogError("not found appsettings.json directory");
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

        private string GetAppSettingsDirectory(Arguments arguments)
        {
            if (arguments.TryGetValue("appsettings", out var value))
            {
                return value;
            }

            return string.Empty;
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
            if (TryGetSection<T>(key, out var result))
            {
                return result;
            }

            return default;
        }

        public bool TryGetSection<T>(string key, out T result)
        {
            if (_appSettings.TryGetValue(key, out var token))
            {
                if (token.Type == JTokenType.Object)
                {
                    result = token.ToObject<T>();
                }
                else
                {
                    result = token.Value<T>();
                }

                return true;
            }

            result = default;
            return false;
        }

        public IEnumerable<T> GetSections<T>(string key)
        {
            if (TryGetSections<T>(key, out var result))
            {
                return result;
            }

            return Array.Empty<T>();
        }

        public bool TryGetSections<T>(string key, out IEnumerable<T> result)
        {
            if (_appSettings.TryGetValue(key, out var token))
            {
                if (token.First().Type == JTokenType.Object)
                {
                    result = token.Select(t => t.ToObject<T>());
                }
                else
                {
                    result = token.Values<T>();
                }

                return true;
            }

            result = null;
            return false;
        }

        public string GetArgument(string key, string defaultValue = null)
        {
            if (Args.TryGetValue(key, out var arg))
            {
                return arg;
            }

            return defaultValue ?? string.Empty;
        }

        public bool TryGetArgument(string key, out string arg)
        {
            if (Args.TryGetValue(key, out var value))
            {
                arg = value.Value;
                return true;
            }

            arg = null;
            return false;
        }
    }
}
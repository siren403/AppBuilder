using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace AppBuilder
{
    public partial class UnityPlayerBuilder : IUnityPlayerBuilder
    {
        private BuildPlayerOptions _buildOptions;

        private readonly Dictionary<string, string> _commandArgs;
        public BuildConfigureRecorder Recorder { get; } = new();

        public UnityPlayerBuilder(Dictionary<string, string> commandArgs)
        {
            _commandArgs = commandArgs;
        }

        public string[] Scenes
        {
            set
            {
                _buildOptions.scenes = value;
                if (value.Length == 0) return;

                Recorder.Write("Scenes", value[0]);
                for (int i = 1; i < value.Length; i++)
                {
                    Recorder.Write(string.Empty, value[i]);
                }
            }
        }

        public string OutPutDirectory { get; set; }

        public IOptions<TConfig> Configure<TConfig>(string settingsDirectory)
            where TConfig : class
        {
            var baseSettingsPath = Path.Combine(settingsDirectory, "appsettings.json");

            JObject settings = null;

            if (File.Exists(baseSettingsPath))
            {
                using var baseSettingReader = new StreamReader(baseSettingsPath);
                settings = JObject.Parse(baseSettingReader.ReadToEnd());
            }

            if (_commandArgs.TryGetValue("mode", out var mode))
            {
                var overwriteSettingsPath = Path.Combine(settingsDirectory, $"appsettings.{mode}.json");
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

            if (settings != null)
            {
                return new JObjectProvider<TConfig>(settings);
            }
            else
            {
                return new EmptyProvider<TConfig>();
            }
        }

        public override string ToString()
        {
            return Recorder.ToString();
        }

        public BuildPlayerOptions Build()
        {
            _buildOptions.locationPathName = Path.Combine(
                OutPutDirectory,
                _buildOptions.target.ToString(),
                $"{PlayerSettings.productName}_{DateTime.Now.Ticks}");

            //add extension
            switch (_buildOptions.target)
            {
                case BuildTarget.Android:
                    _buildOptions.locationPathName =
                        Path.ChangeExtension(_buildOptions.locationPathName, "apk");
                    break;
            }

            Recorder.Write("Output", _buildOptions.locationPathName);
            return _buildOptions;
        }
    }

    public partial class UnityPlayerBuilder
    {
        public void ConfigureAndroid(Action<AndroidSettingsBuilder> configuration)
        {
            _buildOptions.target = BuildTarget.Android;
            _buildOptions.targetGroup = BuildTargetGroup.Android;
            var builder = new AndroidSettingsBuilder(Recorder);
            configuration(builder);

            Recorder.Write("BuildTarget", _buildOptions.target.ToString());
            Recorder.Write("BuildTargetGroup", _buildOptions.targetGroup.ToString());
        }
    }
}
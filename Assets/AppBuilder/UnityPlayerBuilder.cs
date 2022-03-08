using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace AppBuilder
{
    public partial class UnityPlayerBuilder : IUnityPlayerBuilder
    {
        private BuildPlayerOptions _buildOptions;

        public BuildPlayerOptions BuildOptions
        {
            get
            {
                _buildOptions.locationPathName = Path.Combine(OutPutDirectory, PlayerSettings.productName);
                switch (_buildOptions.target)
                {
                    case BuildTarget.Android:
                        _buildOptions.locationPathName = Path.ChangeExtension(_buildOptions.locationPathName, "apk");
                        break;
                }

                return _buildOptions;
            }
        }

        private readonly Dictionary<string, string> _commandArgs;

        public UnityPlayerBuilder(Dictionary<string, string> commandArgs)
        {
            _commandArgs = commandArgs;
        }

        public string[] Scenes
        {
            set => _buildOptions.scenes = value;
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
    }

    public partial class UnityPlayerBuilder
    {
        public void ConfigureAndroid(Action<AndroidSettingsBuilder> configuration)
        {
            if (!Application.isBatchMode)
            {
                var isSuccess =
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                if (!isSuccess)
                {
                    throw new Exception("[AppBuilder] SwitchPlatform Failed!");
                }
            }

            _buildOptions.target = BuildTarget.Android;
            _buildOptions.targetGroup = BuildTargetGroup.Android;
            var builder = new AndroidSettingsBuilder();
            configuration(builder);
        }
    }
}
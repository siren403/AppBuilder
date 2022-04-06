using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AppBuilder
{
    public partial class UnityPlayerBuilder : IUnityPlayerBuilder
    {
        private BuildPlayerOptions _buildOptions;
        public BuildConfigureRecorder Recorder { get; } = new();

        public string ProjectName => Path.GetFileName(Directory.GetCurrentDirectory());

        public string[] Scenes
        {
            set => _buildOptions.scenes = value;
        }

        private string _outputDirectory;

        public string OutputPath
        {
            set => _outputDirectory = value.Replace("\\", "/");
            get => _outputDirectory;
        }

        private string _productName;

        public string ProductName
        {
            set
            {
                _productName = value;
                Recorder.Enqueue(() => PlayerSettings.productName = value,
                    new BuildProperty("ProductName", value));
            }
        }

        public BuildTarget Target
        {
            get => _buildOptions.target;
            set => _buildOptions.target = value;
        }

        public BuildTargetGroup TargetGroup
        {
            set => _buildOptions.targetGroup = value;
        }

        public void Display(params (string key, string value)[] pairs)
        {
            using (Recorder.Section("Display"))
            {
                foreach (var (key, value) in pairs)
                {
                    Recorder.Write(new BuildProperty(key, value));
                }
            }
        }

        public ConfigureSection Display(out Action<string, string> add)
        {
            add = (key, value) => { Recorder.Write(new BuildProperty(key, value)); };
            return Recorder.Section("Display");
        }

        public override string ToString()
        {
            return Recorder.ToString();
        }

        public BuildExecutor Build()
        {
            if (string.IsNullOrEmpty(_outputDirectory))
            {
                _outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Build", Target.ToString(),
                    _productName ?? Application.productName).Replace("\\", "/");
            }

            if (!Path.HasExtension(_outputDirectory))
            {
                _buildOptions.locationPathName = Path.ChangeExtension(
                    _outputDirectory,
                    _buildOptions.target switch
                    {
                        BuildTarget.StandaloneWindows64 => "exe",
                        BuildTarget.Android => "apk",
                        _ => string.Empty
                    });
            }
            else
            {
                _buildOptions.locationPathName = _outputDirectory;
            }

            if (_buildOptions.scenes == null || !_buildOptions.scenes.Any())
            {
                Recorder.Write(new BuildProperty("Scenes", string.Empty));
            }
            else
            {
                Recorder.Write(new BuildProperty("Scenes", BuildPropertyOptions.SectionBegin));
                for (int i = 0; i < _buildOptions.scenes.Length; i++)
                {
                    Recorder.Write(new BuildProperty($"[{i}]", _buildOptions.scenes[i]));
                }

                Recorder.Write(new BuildProperty("Scenes", BuildPropertyOptions.SectionEnd));
            }

            using (Recorder.Section("Target"))
            {
                Recorder.Write("BuildTarget", _buildOptions.target.ToString());
                Recorder.Write("BuildTargetGroup", _buildOptions.targetGroup.ToString());
            }

            Recorder.Write("OutputPath", _buildOptions.locationPathName);

            return new BuildExecutor(_buildOptions, Recorder.Export());
        }
    }

    public class BuildExecutor
    {
        private readonly Action[] _configurations;
        public BuildPlayerOptions Options { get; }


        public BuildExecutor(BuildPlayerOptions options, Action[] configurations)
        {
            _configurations = configurations;
            Options = options;
        }

        public void Validate()
        {
            Options.Validate();
        }

        public void Configure()
        {
            if (!Application.isBatchMode)
            {
                switch (Options.target)
                {
                    case BuildTarget.Android:
                        var isSuccess =
                            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,
                                BuildTarget.Android);
                        if (!isSuccess)
                        {
                            throw new Exception("[AppBuilder] SwitchPlatform Failed!");
                        }

                        break;
                }
            }

            foreach (var configuration in _configurations)
            {
                configuration.Invoke();
            }
        }

        public BuildReport Execute()
        {
            Configure();

            Debug.Log($"[AppBuilder] {JsonConvert.SerializeObject(Options)}");
            return BuildPipeline.BuildPlayer(Options);
        }
    }

    public partial class UnityPlayerBuilder
    {
        public void ConfigureAndroid(Action<AndroidConfigureBuilder> configuration)
        {
            using (Recorder.Section("Android"))
            {
                Target = BuildTarget.Android;
                TargetGroup = BuildTargetGroup.Android;
                var builder = new AndroidConfigureBuilder(Recorder);
                configuration(builder);
            }
        }
    }

    public static class OptionsExtensions
    {
        public static void WriteScriptable<TConfig>(this IOptions<TConfig> source, string path, string withJson = null)
            where TConfig : class
        {
            var output = Resources.Load<OptionsScriptableObject<TConfig>>(path);
            output.Value = source.Value;
            output.Json = !string.IsNullOrEmpty(withJson) ? withJson : null;
            output.Save();
        }
    }
}
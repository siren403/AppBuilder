using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

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

        public string CompanyName
        {
            set
            {
                var name = !string.IsNullOrEmpty(value) ? value : "DefaultCompany";
                Recorder.Enqueue(() => PlayerSettings.companyName = name,
                    new BuildProperty("CompanyName", name));
            }
        }

        private string _productName;

        public string ProductName
        {
            set
            {
                _productName = value;
                Recorder.Enqueue(() => PlayerSettings.productName = value,
                    new BuildProperty(nameof(ProductName), value));
            }
        }

        public string Version
        {
            set
            {
                Recorder.Enqueue(() => PlayerSettings.bundleVersion = value,
                    new BuildProperty(nameof(Version), value));
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

        public void ApplyPreset(string presetPath, string targetPath)
        {
            Recorder.Enqueue(() =>
            {
                var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
                var target = AssetDatabase.LoadAssetAtPath<Object>(targetPath);
                preset.ApplyTo(target);
            }, new BuildProperty(Path.GetFileName(presetPath), Path.GetFileName(targetPath)));
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
                switch (_buildOptions.target)
                {
                    case BuildTarget.StandaloneWindows64:
                        _buildOptions.locationPathName = Path.ChangeExtension(_outputDirectory, "exe");
                        break;
                    case BuildTarget.Android:
                        var androidExt = "apk";
                        if (EditorUserBuildSettings.buildAppBundle)
                        {
                            androidExt = "aab";
                        }

                        _buildOptions.locationPathName = Path.ChangeExtension(_outputDirectory, androidExt);
                        break;
                    default:
                        _buildOptions.locationPathName = _outputDirectory;
                        break;
                }
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

            return new BuildExecutor(_buildOptions, Recorder);
        }
    }

    public class BuildExecutor
    {
        private readonly BuildConfigureRecorder _configurations;
        public BuildPlayerOptions Options { get; }


        public BuildExecutor(BuildPlayerOptions options, BuildConfigureRecorder configurations)
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
                var isSuccess = false;
                switch (Options.target)
                {
                    case BuildTarget.Android:
                        isSuccess =
                            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,
                                BuildTarget.Android);
                        break;
                    case BuildTarget.iOS:
                        isSuccess =
                            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS,
                                BuildTarget.iOS);
                        break;
                }

                if (!isSuccess)
                {
                    throw new Exception("[AppBuilder] SwitchPlatform Failed!");
                }
            }

            var configurations = _configurations.Export();
            foreach (var configuration in configurations)
            {
                configuration.Invoke();
            }
        }

        public BuildReport Execute()
        {
            Configure();
            var configurations = _configurations.Export(ConfigureTiming.ExecuteBuild);
            foreach (var configuration in configurations)
            {
                configuration.Invoke();
            }

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

    public partial class UnityPlayerBuilder
    {
        public void ConfigureiOS(Action<iOSConfigureBuilder> configuration)
        {
            using (Recorder.Section("iOS"))
            {
                // _buildOptions.options |= BuildOptions.AcceptExternalModificationsToPlayer;

                Target = BuildTarget.iOS;
                TargetGroup = BuildTargetGroup.iOS;

                var builder = new iOSConfigureBuilder(Recorder);
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
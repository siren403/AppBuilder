using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBuilder.ConsoleApp;
using AppBuilder.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder
{
    [Serializable]
    public class AdbDevices
    {
        public List<string> devices;
    }

    public interface IEmulatorProfile
    {
        string Name { get; }
    }

    public class InstallEmulatorJob<TProfile> : IPostBuildJob, IBuildJobRenderer
        where TProfile : IEmulatorProfile
    {
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && _isEnabled;
            set => _isEnabled = value;
        }

        public string Name => nameof(InstallEmulatorJob<TProfile>);

        private readonly IEmulatorProfile _profile;
        private readonly string _packageName;

        public InstallEmulatorJob()
        {
            _profile = Activator.CreateInstance(typeof(TProfile)) as IEmulatorProfile;
            _packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        }

        public void Render(VisualElement content)
        {
            content.Add(new Label(_profile.Name));
            content.Add(new Label(_packageName));
        }

        public void Run(BuildPlayer.Report report)
        {
            var device = BuildCache.GetString("device");
            if (string.IsNullOrEmpty(device))
            {
                Debug.Log($"{_profile.Name}: not found device");
                return;
            }

            using var scope = new DisplayProgressBarScope(Name);
            using var process = new AdbConsole();

            var apk = report.UnityReport.summary.outputPath;
            process.Install(device, apk, _packageName);
            scope.Progress = 1;
        }
    }
}
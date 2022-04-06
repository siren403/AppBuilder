using System;
using System.IO;
using AppBuilder.ConsoleApp;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder
{
    public abstract class JsonProfile : FtpConsole.IProfile
    {
        [Serializable]
        private class Data
        {
            public string host;
            public string user;
            public string password;
        }

        public string Host => _loadedData?.host;
        public string User => _loadedData?.user;
        public string Password => _loadedData?.password;

        private readonly Data _loadedData;

        protected JsonProfile(string json)
        {
            _loadedData = JsonUtility.FromJson<Data>(json);
        }
    }

    public class UploadFtpJob<TProfile> : IPostBuildJob, IBuildJobRenderer where TProfile : FtpConsole.IProfile
    {
        public bool IsEnabled { get; set; } = true;
        public string Name => "Upload Ftp";

        private readonly FtpConsole.IProfile _profile;

        public UploadFtpJob()
        {
            _profile = Activator.CreateInstance(typeof(TProfile)) as FtpConsole.IProfile;
        }

        public void Run(BuildPlayer.Report report)
        {
            using var scope = new DisplayProgressBarScope(Name);
            using var process = new FtpConsole();

            var local = report.UnityReport.summary.outputPath;
            process.Upload(_profile, local, $"/publish/{Path.GetFileName(local)}");
            
            scope.Progress = 1;
        }

        public void Render(VisualElement content)
        {
            content.Add(new Label($"{_profile.Host} | {_profile.User}:{_profile.Password}"));
        }
    }
}
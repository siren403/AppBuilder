using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AppBuilder.ConsoleApp
{
    public abstract class AppBuilderConsole : IDisposable
    {
        private const string Tool = "./Tools/AppBuilderConsoleExtension/Publish/win-x64/AppBuilderConsoleExtension.exe";
        protected readonly ProcessExecutor Process;

        protected AppBuilderConsole()
        {
            Process = new(Tool);
        }

        public void Dispose()
        {
            Process?.Dispose();
        }
    }

    public class FtpConsole : AppBuilderConsole
    {
        public interface IProfile
        {
            string Host { get; }
            string User { get; }
            string Password { get; }
        }

        public void Upload(IProfile profile, string local, string remote)
        {
            var args = new Dictionary<string, string>
            {
                ["host"] = profile.Host,
                ["user"] = profile.User,
                ["passwd"] = profile.Password,
                ["local"] = local,
                ["remote"] = remote
            };
            Debug.Log($"[Upload] \n{profile.Host}, {profile.User}, {profile.Password}, {local}, {remote}");
            Process.Execute("ftp upload", args);
        }
    }

    public class AdbConsole : AppBuilderConsole
    {
        public List<string> Devices()
        {
            Process.Execute("adb devices");
            return JsonUtility.FromJson<AdbDevices>(Process.ReadToEnd()).devices;
        }

        public void Install(string device, string apk, [CanBeNull] string package = null)
        {
            var args = new Dictionary<string, string>
            {
                ["device"] = device,
                ["apk"] = apk,
                ["package"] = package
            };
            Process.Execute("adb install", args);
            Debug.Log($"[Adb Install] {device}, {apk}, {package}");
        }
    }

    public class ProcessExecutor : IDisposable
    {
        private readonly ProcessStartInfo _startInfo;
        private readonly StringBuilder _builder = new();
        private readonly string _program;
        private Process _process;

        public ProcessExecutor(string program)
        {
            _program = program;


            var shell = string.Empty;
#if UNITY_EDITOR_WIN
            shell = "powershell.exe";
            _program = _program.Replace(" ", "` ");
#elif UNITY_EDITOR_OSX
            shell = "zsh";
#elif UNITY_EDITOR_LINUX
            shell = "bash";
#endif
            if (string.IsNullOrEmpty(shell))
            {
                throw new ArgumentNullException(nameof(shell));
            }

            _startInfo = new(shell)
            {
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };
        }


        private string GetArgumentsString(Dictionary<string, string> args)
        {
            if (!args.Any()) return string.Empty;

            _builder.Clear();

            foreach (var arg in args)
            {
                _builder.Append($"--{arg.Key} \"{arg.Value}\" ");
            }

            return _builder.ToString();
        }

        public void Execute(string command, Dictionary<string, string> args)
        {
            _startInfo.Arguments = $"{_program} {command} {GetArgumentsString(args)}";
            _process = new()
            {
                StartInfo = _startInfo,
            };

            _process.Start();
        }

        public void Execute(string command)
        {
            _startInfo.Arguments = $"{_program} {command}";
            _process = new()
            {
                StartInfo = _startInfo,
            };

            _process.Start();
        }

        public IEnumerable<string> ReadLines()
        {
            if (_process == null) yield break;

            yield return _startInfo.Arguments;

            while (!_process.StandardOutput.EndOfStream)
            {
                yield return _process.StandardOutput.ReadLine();
            }
        }

        public string ReadToEnd()
        {
            return _process?.StandardOutput.ReadToEnd();
        }

        public void Dispose()
        {
            _process?.Dispose();
        }
    }
}
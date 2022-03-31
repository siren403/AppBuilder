using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;

namespace AppBuilder.ConsoleApp
{
    public class ProcessExecutor : IDisposable
    {
        private readonly ProcessStartInfo _startInfo;
        private readonly StringBuilder _builder = new();
        private readonly string _program;
        private Process _process;

        public ProcessExecutor(string program)
        {
            _program = program;

            var shell = "powershell.exe";
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
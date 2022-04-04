using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace AppBuilder
{
    /// <summary>
    /// Preview 
    /// </summary>
    public static partial class BuildPlayer
    {
        public class Report
        {
            private readonly BuildProperty[] _properties;
            private readonly string _recorderLog;
            private readonly BuildPlayerOptions _options;
            public BuildReport UnityReport { get; }

            public BuildProperty[] Properties => _properties ?? Array.Empty<BuildProperty>();
            public Arguments Args { get; }

            public Report(UnityBuildContext context, UnityPlayerBuilder builder, BuildPlayerOptions options)
            {
                Args = context.Args;
                _properties = builder.Recorder.GetProperties();
                _recorderLog = builder.Recorder.ToString();
                _options = options;
            }

            public Report(UnityBuildContext context, UnityPlayerBuilder builder, BuildReport unityReport,
                BuildPlayerOptions options)
            {
                Args = context.Args;
                _properties = builder.Recorder.GetProperties();
                _recorderLog = builder.Recorder.ToString();
                UnityReport = unityReport;
                _options = options;
            }

            public override string ToString()
            {
                return _recorderLog ?? string.Empty;
            }

            public string ToCommandLineArgs(BuildInfo build)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("-batchmode ");
                stringBuilder.Append("-quit ");
                stringBuilder.Append($"-executeMethod \"{build.FullName}\" ");
                stringBuilder.Append($"-buildTarget \"{_options.target.ToString()}\"");
                if (Args.TryGetValue("projectPath", out var arg))
                {
                    stringBuilder.Append($"-{arg.Key} \"{arg.Value}\"");
                }

                return Args.Where(pair => pair.Value.Category == ArgumentCategory.Input)
                    .Aggregate(stringBuilder.ToString(), (acc, arg) => $"{acc} -{arg.Key} \"{arg.Value.Value}\"");
            }
        }

        public static Report Execute(BuildInfo build, Arguments inputArgs)
        {
            using var scope = new BuildScope(build, inputArgs);
            return scope.Report;
        }

        public class BuildScope : IDisposable
        {
            public Arguments InputArgs { get; }
            public Report Report;

            public BuildScope(BuildInfo build, Arguments inputArgs)
            {
                InputArgs = inputArgs;

                _buildScope = this;

                build.Method.Invoke(null, null);
            }

            public void Dispose()
            {
                _buildScope = null;
            }
        }
    }

    /// <summary>
    /// Methods
    /// </summary>
    public static partial class BuildPlayer
    {
        public static Dictionary<string, MethodInfo> CollectBuilds()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(_ => _.GetReferencedAssemblies().Any(_ => _.Name == "AppBuilder.Editor"))
                .SelectMany(_ =>
                {
                    return _.GetTypes()
                        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                        .Where(m => m.GetCustomAttribute<BuildAttribute>() != null);
                }).ToDictionary(info => info.Name);
        }

        public static IEnumerable<BuildInfo> GetBuilds()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(_ => _.GetReferencedAssemblies().Any(_ => _.Name == "AppBuilder.Editor"))
                .SelectMany(_ =>
                {
                    return _.GetTypes()
                        .SelectMany(t =>
                            t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .Select(method => (classType: t, method,
                                    attr: method.GetCustomAttribute<BuildAttribute>())))
                        .Where(_ => _.attr != null)
                        .Select(_ => (order: _.attr.Order,
                            info: new BuildInfo(_.classType.FullName, _.method, _.attr.DisplayName)));
                })
                .OrderByDescending(_ => _.order)
                .Select(_ => _.info);
        }
    }

    public class BuildInfo
    {
        private readonly string _className;
        private readonly string _name;
        public string Name => string.IsNullOrEmpty(_name) ? Method.Name : _name;
        public string FullName => $"{_className}.{Method.Name}";

        /// <summary>
        /// [Argument("key", "value")]
        /// static void GooglePlay(){}
        /// public Args {get;}
        /// </summary>
        public IEnumerable<InputAttribute> Inputs => Method.GetCustomAttributes<InputAttribute>();

        // public string[] Variants => Method.GetCustomAttribute<VariantsAttribute>()?.Keys ?? Array.Empty<string>();

        public MethodInfo Method { get; }

        public BuildInfo(string className, MethodInfo method)
        {
            _className = className;
            Method = method;
        }

        public BuildInfo(string className, MethodInfo method, string name)
        {
            _className = className;
            Method = method;
            _name = name;
        }
    }
}
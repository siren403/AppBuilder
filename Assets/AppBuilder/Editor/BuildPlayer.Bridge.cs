using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Build.Reporting;

namespace AppBuilder
{
    /// <summary>
    /// Preview 
    /// </summary>
    public static partial class BuildPlayer
    {
        private static PreviewScope _previewScope;

        private class PreviewScope : IDisposable
        {
            public PreviewContext Context { get; private set; }

            public PreviewScope()
            {
                Context = new PreviewContext();
            }

            public void Dispose()
            {
                Context = null;
                _previewScope = null;
            }
        }

        public class Report
        {
            public BuildReport UnityReport { get; }
            private readonly BuildProperty[] _properties;
            private readonly string _recorderLog;
            public BuildProperty[] Properties => _properties ?? Array.Empty<BuildProperty>();
            public Arguments Args { get; }

            public Report(UnityBuildContext context, UnityPlayerBuilder builder)
            {
                Args = context.Args;
                _properties = builder.Recorder.GetProperties();
                _recorderLog = builder.Recorder.ToString();
            }

            public Report(UnityBuildContext context, UnityPlayerBuilder builder, BuildReport unityReport)
            {
                Args = context.Args;
                _properties = builder.Recorder.GetProperties();
                _recorderLog = builder.Recorder.ToString();
                UnityReport = unityReport;
            }

            public override string ToString()
            {
                return _recorderLog ?? string.Empty;
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
        public static Dictionary<string, MethodInfo> CollectMethods()
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
                        .Select(_ => new BuildInfo(_.classType.FullName, _.method, _.attr.Name));
                });
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
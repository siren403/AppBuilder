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

        public static IDisposable Preview(MethodInfo build, out PreviewContext context)
        {
            _previewScope = new PreviewScope();
            build.Invoke(null, null);
            context = _previewScope.Context;
            return _previewScope;
        }

        public static IDisposable Preview(BuildInfo build, Dictionary<string, string> args,
            out PreviewContext context)
        {
            _previewScope = new PreviewScope();
            using (_inputScope = new InputScope(args))
            {
                build.Method.Invoke(null, null);
            }

            context = _previewScope.Context;
            return _previewScope;
        }

        public static PreviewContext Preview(MethodInfo build)
        {
            using (Preview(build, out var context))
            {
                return context;
            }
        }

        public static PreviewContext Preview(BuildInfo build, Dictionary<string, string> args)
        {
            using (Preview(build, args, out var context))
            {
                return context;
            }
        }

        public static void BuildPreview(BuildInfo build, Dictionary<string, string> args)
        {
            using (_inputScope = new InputScope(args))
            {
                build.Method.Invoke(null, null);
            }
        }

        public class Report
        {
            public BuildReport UnityReport { get; }
            private readonly BuildProperty[] _properties;
            private readonly string _recorderLog;
            public BuildProperty[] Properties => _properties ?? Array.Empty<BuildProperty>();

            private Dictionary<string, string> _args;

            public Dictionary<string, string> Args { get; }

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

        public static Report Execute(BuildInfo build, Dictionary<string, string> inputArgs)
        {
            using var scope = new BuildScope(build, inputArgs);
            return scope.Report;
        }

        public class BuildScope : IDisposable
        {
            public Dictionary<string, string> InputArgs { get; }
            public Report Report;

            public BuildScope(BuildInfo build, Dictionary<string, string> inputArgs)
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
                                .Select(method => (classType: t, method)))
                        .Where(_ => _.method.GetCustomAttribute<BuildAttribute>() != null)
                        .Select(_ => new BuildInfo(_.classType.FullName, _.method));
                });
        }
    }

    public class BuildInfo
    {
        private readonly string _className;
        public string Name => Method.Name;
        public string FullName => $"{_className}.{Name}";

        /// <summary>
        /// [Argument("key", "value")]
        /// static void GooglePlay(){}
        /// public Args {get;}
        /// </summary>
        public IEnumerable<InputAttribute> Inputs => Method.GetCustomAttributes<InputAttribute>();

        public string[] Variants => Method.GetCustomAttribute<VariantsAttribute>()?.Keys ?? Array.Empty<string>();

        public MethodInfo Method { get; }

        public BuildInfo(string className, MethodInfo method)
        {
            _className = className;
            Method = method;
        }
    }
}
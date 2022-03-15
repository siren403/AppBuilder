using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace AppBuilder
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BuildAttribute : Attribute
    {
    }

    [Flags]
    public enum ArgumentOptions
    {
        None = 0,
        Smart = 1,
        Directory = 2
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ArgumentAttribute : Attribute
    {
        public string Name { get; }
        public ArgumentOptions Options { get; }

        public ArgumentAttribute(string name, ArgumentOptions options = ArgumentOptions.None)
        {
            Name = name;
            Options = options;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class VariantAttribute : Attribute
    {
        public string Key { get; }

        public VariantAttribute(string key)
        {
            Key = key;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandLineArgsAttribute : Attribute
    {
    }

    public class PreviewContext
    {
        public BuildConfigureRecorder Recorder
        {
            set
            {
                _properties = value.GetProperties();
                _recorderLog = value.ToString();
            }
        }

        private BuildProperty[] _properties;
        private string _recorderLog;
        public BuildProperty[] Properties => _properties ?? Array.Empty<BuildProperty>();

        private Dictionary<string, string> _args;

        public Dictionary<string, string> Args
        {
            get => _args ?? new Dictionary<string, string>();
            set => _args = value;
        }

        public override string ToString()
        {
            return _recorderLog ?? string.Empty;
        }
    }

    public class ArgumentScope : IDisposable
    {
        public Dictionary<string, string> Args { get; }

        public ArgumentScope(Dictionary<string, string> args)
        {
            Args = args;
        }

        public void Dispose()
        {
        }
    }

    public static partial class BuildPlayer
    {
        private static ArgumentScope _argumentScope;

        public static void Build(Action<IBuildContext, IUnityPlayerBuilder> configuration, bool isTest = false)
        {
            var args = new Dictionary<string, string>();
            if (_argumentScope != null)
            {
                args.Merge(_argumentScope.Args);
            }

            args.Merge(Environment
                .GetCommandLineArgs()
                .AddReserveArguments());

            Debug.Log(args.ToArgsString());
            var context = new UnityBuildContext(args);

            var builder = new UnityPlayerBuilder();
            configuration(context, builder);

            var executor = builder.Build();

            //todo: isPreview or Execute(Editor, Batch)
            if (_previewScope != null)
            {
                _previewScope.Context.Recorder = builder.Recorder;
                _previewScope.Context.Args = context.Args;
                return;
            }

            if (args.ContainsKey("CONFIGURE_ONLY"))
            {
                executor.Configure();
                Debug.Log("Configure Completed");
                return;
            }

            try
            {
                executor.Validate();
            }
            catch (Exception e)
            {
                Debug.Log($"[AppBuilder] Build Failed {e.Message}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }

            var report = executor.Execute();
            //todo: BatchMode -> Revert? builder.Revert()
            EditorUtility.RevealInFinder(Path.GetDirectoryName(report.summary.outputPath));
        }
    }

    public static class BuildPlayerOptionsExtensions
    {
        public static void Validate(this BuildPlayerOptions options)
        {
            if (options.scenes == null || !options.scenes.Any())
            {
                throw new ValidationException("empty build scenes");
            }

            if (string.IsNullOrEmpty(options.locationPathName))
            {
                throw new ValidationException("empty output path");
            }
        }
    }

    public static class CommandArgsExtensions
    {
        public static string ToArgsString(this Dictionary<string, string> args, StringBuilder builder = null)
        {
            builder ??= new StringBuilder();

            builder.AppendLine("[CommandArgs]");
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg.Value))
                {
                    builder.AppendLine($"-{arg.Key}");
                }
                else
                {
                    builder.AppendLine($"-{arg.Key} {arg.Value}");
                }
            }

            return builder.ToString();
        }

        private static void Reserve(this Dictionary<string, string> args, string key, string value)
        {
            if (args.ContainsKey(key))
            {
                throw new ArgumentException("Contains Reserve Args");
            }

            args.Add(key, value);
        }

        public static Dictionary<string, string> AddReserveArguments(this Dictionary<string, string> args)
        {
            args.Reserve("AB_CURRENT_DIRECTORY", Directory.GetCurrentDirectory());
            args.Reserve("AB_PRODUCT_NAME", Application.productName);

            return args;
        }

        public static void Merge(this Dictionary<string, string> args, Dictionary<string, string> other)
        {
            if (other == null) throw new ArgumentNullException();
            if (other.Count == 0) return;

            foreach (var arg in other)
            {
                args[arg.Key] = arg.Value;
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> WhereReserve(this Dictionary<string, string> args)
        {
            return args.Where(pair => pair.Key.Contains("AB_"));
        }
    }

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

        public static IDisposable Preview(MethodInfo buildMethod, out PreviewContext context)
        {
            _previewScope = new PreviewScope();
            buildMethod.Invoke(null, null);
            context = _previewScope.Context;
            return _previewScope;
        }

        public static IDisposable Preview(MethodInfo buildMethod, Dictionary<string, string> args,
            out PreviewContext context)
        {
            _previewScope = new PreviewScope();
            using (_argumentScope = new ArgumentScope(args))
            {
                buildMethod.Invoke(null, null);
            }

            context = _previewScope.Context;
            return _previewScope;
        }

        public static PreviewContext Preview(MethodInfo buildMethod)
        {
            using (Preview(buildMethod, out var context))
            {
                return context;
            }
        }

        public static PreviewContext Preview(MethodInfo buildMethod, Dictionary<string, string> args)
        {
            using (Preview(buildMethod, args, out var context))
            {
                return context;
            }
        }

        public static void BuildPreview(MethodInfo buildMethod, Dictionary<string, string> args)
        {
            using (_argumentScope = new ArgumentScope(args))
            {
                buildMethod.Invoke(null, null);
            }
        }
    }
}
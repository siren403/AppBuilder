using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AppBuilder
{
    public static partial class BuildPlayer
    {
        private static InputScope _inputScope;

        public static Dictionary<string, string> GetReservedArguments()
        {
            return Environment.GetCommandLineArgs()
                .AddReserveArguments();
        }

        private static BuildScope _buildScope;

        public static Report Build(Action<IBuildContext, IUnityPlayerBuilder> configuration, bool isTest = false)
        {
            var args = new Dictionary<string, string>();
            args.Merge(GetReservedArguments());
            if (_buildScope != null) //from editor
            {
                args.Merge(_buildScope.InputArgs);
            }

            Debug.Log(args.ToArgsString());
            var context = new UnityBuildContext(args);
            var builder = new UnityPlayerBuilder();
            configuration(context, builder);

            var executor = builder.Build();

            if (!args.TryGetValue("mode", out var mode))
            {
                mode = string.Empty;
            }

            if (mode == "preview")
            {
                return Complete(new Report(context, builder));
            }

            if (mode == "configure")
            {
                executor.Configure();
                return Complete(new Report(context, builder));
            }

            //todo: isPreview or Execute(Editor, Batch)
            // if (_previewScope != null)
            // {
            //     _previewScope.Context.Recorder = builder.Recorder;
            //     _previewScope.Context.Args = context.Args;
            //     return;
            // }

            // if (args.ContainsKey("CONFIGURE_ONLY"))
            // {
            //     executor.Configure();
            //     Debug.Log("Configure Completed");
            //     return;
            // }

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

            var unityReport = executor.Execute();
            //todo: BatchMode -> Revert? builder.Revert()
            EditorUtility.RevealInFinder(Path.GetDirectoryName(unityReport.summary.outputPath));

            return Complete(new Report(context, builder, unityReport));

            Report Complete(Report report)
            {
                if (_buildScope != null)
                {
                    _buildScope.Report = report;
                }

                return report;
            }
        }
    }
}
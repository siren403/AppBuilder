using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace AppBuilder
{
    public static partial class BuildPlayer
    {
        private static InputScope _inputScope;

        public static void Build(Action<IBuildContext, IUnityPlayerBuilder> configuration, bool isTest = false)
        {
            var args = new Dictionary<string, string>();
            if (_inputScope != null)
            {
                args.Merge(_inputScope.Args);
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

}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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

    public static partial class BuildPlayer
    {
        private static bool IsPreview = false;
        public static string BuildPreview;
        public static (string item, string message)[] BuildMessages;

        private class PreviewScope : IDisposable
        {
            public PreviewScope()
            {
                IsPreview = true;
            }

            public void Dispose()
            {
                IsPreview = false;
                BuildPreview = string.Empty;
                _currentPreviewScope = null;
            }
        }

        private static PreviewScope _currentPreviewScope;

        public static IDisposable Preview()
        {
            if (_currentPreviewScope == null)
            {
                _currentPreviewScope = new PreviewScope();
            }

            return _currentPreviewScope;
        }

        public static void Build(Action<IUnityPlayerBuilder> configuration)
        {
            var args = Environment.GetCommandLineArgs();
            var commandArgs = new StringBuilder();
            commandArgs.AppendLine("[CommandArgs]");
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg.Value))
                {
                    commandArgs.AppendLine($"-{arg.Key}");
                }
                else
                {
                    commandArgs.AppendLine($"-{arg.Key} {arg.Value}");
                }
            }

            Debug.Log(commandArgs.ToString());

            var builder = new UnityPlayerBuilder(args);
            configuration(builder);
            var options = builder.Build();

            //todo: isPreview or Execute(Editor, Batch)
            if (IsPreview)
            {
                BuildMessages = builder.Recorder.GetMessages();
                BuildPreview = builder.ToString();
            }
            else
            {
                try
                {
                    options.Validate();
                }
                catch (Exception e)
                {
                    Debug.Log($"[AppBuilder] Build Failed {e.Message}");
                    if (Application.isBatchMode)
                    {
                        EditorApplication.Exit(0);
                    }
                }

                Debug.Log($"isEditor: {Application.isEditor}");
                Debug.Log($"isBatch: {Application.isBatchMode}");
                if (!Application.isBatchMode)
                {
                    switch (options.target)
                    {
                        case BuildTarget.Android:
                            var isSuccess =
                                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,
                                    BuildTarget.Android);
                            if (!isSuccess)
                            {
                                throw new Exception("[AppBuilder] SwitchPlatform Failed!");
                            }

                            break;
                    }
                }

                Debug.Log($"[AppBuilder] {JsonConvert.SerializeObject(options)}");
                Debug.Log($"{builder}");

                if (!Directory.Exists(builder.OutPutDirectory))
                {
                    Directory.CreateDirectory(builder.OutPutDirectory);
                }

                var report = BuildPipeline.BuildPlayer(options);
                //todo: BatchMode -> Revert? builder.Revert()
            }
        }
    }

    public static class BuildPlayerOptionsExtensions
    {
        public static void Validate(this BuildPlayerOptions options)
        {
            if (options.scenes == null)
            {
                throw new ValidationException("empty build scenes");
            }
        }
    }
}
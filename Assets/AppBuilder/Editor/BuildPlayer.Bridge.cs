using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            using (_inputScope = new InputScope(args))
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
            using (_inputScope = new InputScope(args))
            {
                buildMethod.Invoke(null, null);
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
    }
}
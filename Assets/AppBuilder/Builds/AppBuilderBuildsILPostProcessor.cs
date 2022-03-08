using System.IO;
using System.Linq;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace AppBuilder.Builds
{
    public class AppBuilderBuildsILPostProcessor : ILPostProcessor
    {
        private const string LOGFile = "D:/workspace/unity/AppBuilder/Assets/il.log";

        private static StreamWriter Writer()
        {
            return new StreamWriter(LOGFile);
        }

        private static void Log(StreamWriter writer, string message)
        {
            writer.WriteLine(message);
        }

        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            var referenceDlls = compiledAssembly.References
                .Select(Path.GetFileNameWithoutExtension);

            return referenceDlls.Any(x => x == "AppBuilder") &&
                   referenceDlls.Any(x => x == "Unity.AppBuilder.CodeGen");
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            using var writer = Writer();
            Log(writer, compiledAssembly.Name);
            Log(writer, compiledAssembly.Name + 1);

            return null;
        }
    }
}
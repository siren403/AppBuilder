using System;
using System.IO;
using System.Linq;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEngine;

namespace AppBuilder.Builds
{
    public class AppBuilderBuildsILPostProcessor : ILPostProcessor
    {
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
            
            Console.Write(compiledAssembly.Name);
            return null;
        }
    }
}
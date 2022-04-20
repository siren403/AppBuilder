using AppBuilder;
using Samples.Deploy.Editor;
using UnityEditor;

namespace Samples.DeployUseCase
{
    public static class Build
    {
        [Build("Deploy-UseCase")]
        [AppSettings("Assets/DeployUseCase")]
        [Deploy(BuildTarget.Android)]
        public static void Deploy()
        {
            BuildPlayer.Build((ctx, builder) => { builder.UseDeploy(ctx); });
        }
    }
}
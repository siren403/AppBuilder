using AppBuilder;
using AppBuilder.Deploy;

namespace Samples.DeployUseCase
{
    public static class Build
    {
        [Build("Deploy-UseCase")]
        [AppSettings("Assets/DeployUseCase")]
        public static void Android()
        {
            BuildPlayer.Build((ctx, builder) =>
            {
                builder.UseDeploy(ctx);
            });
        }
    }
}
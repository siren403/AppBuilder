namespace AppBuilder
{
    public interface IPostBuildJob : IBuildJob
    {
        void Run(BuildPlayer.Report report);
    }
}
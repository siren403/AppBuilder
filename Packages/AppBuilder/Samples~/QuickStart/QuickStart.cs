using AppBuilder;

namespace AppBuilderSample
{
    public static class QuickStart
    {
        [Build(order: -1)]
        public static void Build()
        {
            BuildPlayer.Build((ctx, builder) => { builder.UseCurrentEditorSettings(); });
        }
    }
}
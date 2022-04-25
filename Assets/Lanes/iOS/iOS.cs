using AppBuilder;
using AppBuilder.Deploy;

public static class iOS 
{
   [Build(nameof(iOS))]
   [AppSettings("Assets/Lanes/iOS")]
   public static void Build()
   {
      BuildPlayer.Build((ctx, builder) => builder.UseDeploy(ctx));
   }
}

using AppBuilder;
using AppBuilder.Deploy;

public static class iOS 
{
   [Build(nameof(iOS))]
   [AppSettings("Assets/Lanes/iOS")]
   [Variant("Development", "AdHoc", "Testflight")]
   public static void Build()
   {
      BuildPlayer.Build((ctx, builder) => builder.UseDeploy(ctx));
   }
}

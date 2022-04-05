using System.Linq;
using AppBuilder;

namespace AppBuilderSample
{
    public static class UseAppSettingsJson
    {
        public class TitleConfig
        {
            public string logo;

            public override string ToString()
            {
                return $"title.logo: {logo}";
            }
        }

        [Build(order: -3)]
        [AppSettings("Assets/Samples/AppBuilder/0.0.1/Use appsettings.json/AppSettings")]
        [Variant("Development", "GooglePlay", "GooglePlay.Dev")]
        public static void Build()
        {
            BuildPlayer.Build((ctx, builder) =>
            {
                builder.ConfigureCurrentSettings();

                var host = ctx.GetSection<string>("host");
                var title = ctx.GetSection<TitleConfig>("title");
                var titles = ctx.GetSections<TitleConfig>("titles");
                var number = ctx.GetSection<int>("number");
                var numbers = ctx.GetSections<int>("numbers");
                var strings = ctx.GetSections<string>("strings");

                builder.Display(
                    ("host", host),
                    ("title", title.ToString()),
                    ("titles",
                        titles.Aggregate(string.Empty,
                            (acc, n) => string.IsNullOrEmpty(acc) ? $"{n}" : $"{acc}\n{n}")),
                    ("number", number.ToString()),
                    ("numbers",
                        numbers.Aggregate(string.Empty,
                            (acc, n) => string.IsNullOrEmpty(acc) ? $"{n}" : $"{acc}, {n}")),
                    ("strings",
                        strings.Aggregate(string.Empty,
                            (acc, n) => string.IsNullOrEmpty(acc) ? $"{n}" : $"{acc}, {n}"))
                );
            });
        }
    }
}
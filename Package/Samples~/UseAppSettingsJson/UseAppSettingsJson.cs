using System.Linq;

namespace AppBuilder.Samples
{
    public static partial class Builds
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
        [AppSettings("{projectpath}/Assets/Samples/UseAppSettingsJson/0.0.1/Attributes/AppSettings")]
        [Variant("Development", "GooglePlay", "GooglePlay.Dev")]
        public static void UseAppSettingsJson()
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
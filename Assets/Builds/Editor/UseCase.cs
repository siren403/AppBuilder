using System;
using System.Collections.Generic;
using System.Linq;
using AppBuilder;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace Builds
{
    public static class UseCase
    {
        [Build]
        public static void QuickStart()
        {
            BuildPlayer.Build((ctx, builder) => { builder.ConfigureCurrentSettings(); });
        }

        [Build("Attributes Example")]
        [AppSettings]
        [Variant("Development", "Production", "GooglePlay", "GooglePlay.Dev")]
        [Directory("outputPath")]
        [File("keystore", "keystore")]
        [Input("keystore.passwd")]
        [Input("keystore.alias", "alias")]
        [Input("keystore.alias.passwd")]
        public static void Attributes()
        {
            BuildPlayer.Build((ctx, builder) => { builder.ConfigureCurrentSettings(); });
        }

        public class TitleConfig
        {
            public string logo;

            public override string ToString()
            {
                return $"title.logo: {logo}";
            }
        }

        [Build]
        [AppSettings]
        [Variant("Development", "GooglePlay", "GooglePlay.Dev")]
        public static void UsingAppSettingsJson()
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
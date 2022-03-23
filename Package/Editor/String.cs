using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AppBuilder.UI
{
    public static class String
    {
        /// <summary>
        /// in -> a/b/{c}
        /// match -> {c}
        /// </summary>
        private static readonly Regex Pattern = new Regex(@"\{(.*?)\}", RegexOptions.Multiline);

        /// <summary>
        /// in -> a/b/{c}
        /// match -> c
        /// </summary>
        // private static readonly Regex Pattern = new Regex(@"(?<=\{).*?(?=\})", RegexOptions.Multiline);
        public static string Format(string format, Arguments args)
        {
            if (string.IsNullOrEmpty(format)) return format;

            return Pattern.Replace(format, match =>
            {
                var key = match.Value.Substring(1, match.Value.Length - 2);
                return args.TryGetValue(key, out var value) ? value : throw new FormattingException(key);
            });
        }

        public static string Format(string format, object args)
        {
            if (string.IsNullOrEmpty(format)) return format;

            // var result = Pattern.Matches(format);
            // if (!result.Any()) return format;

            var properties = args.GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(args));

            var replaced = Pattern.Replace(format, match =>
            {
                var key = match.Value.Substring(1, match.Value.Length - 2);
                return properties.TryGetValue(key, out var value)
                    ? value.ToString()
                    : throw new FormattingException(key);
            });
            return replaced;
        }
    }

    public class FormattingException : Exception
    {
        private readonly string _key;

        public FormattingException(string key)
        {
            _key = key;
        }

        public override string Message => $"Error parsing format string: {_key}";
    }
}
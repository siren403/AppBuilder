using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AppBuilder
{
    public enum ArgumentCategory
    {
        None,
        Input,
        Require,
        Unity,
        Command,
        Custom
    }

    public readonly struct ArgumentValue
    {
        public readonly string Key;
        public readonly string Value;
        public readonly ArgumentCategory Category;

        public ArgumentValue(string key, string value, ArgumentCategory category = ArgumentCategory.None)
        {
            Key = key;
            Value = value;
            Category = category;
        }

        public static implicit operator string(ArgumentValue arg) => arg.Value;

        public static ArgumentValue Empty(string key, ArgumentCategory category = ArgumentCategory.None)
        {
            return new ArgumentValue(key, string.Empty, category);
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class Arguments : Dictionary<string, ArgumentValue>
    {
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("[Arguments]");
            foreach (var arg in this)
            {
                if (string.IsNullOrEmpty(arg.Value))
                {
                    builder.AppendLine($"-{arg.Key}");
                }
                else
                {
                    builder.AppendLine($"-{arg.Key} {arg.Value.ToString()}");
                }
            }

            return builder.ToString();
        }
    }

    public static class Environment
    {
        public static Arguments GetCommandLineArgs()
        {
            var args = new Arguments();

            var original = System.Environment.GetCommandLineArgs();
            var commandArgs = new StringBuilder();
            foreach (var s in original)
            {
                commandArgs.AppendLine(s);
            }

            for (int i = 0; i < original.Length; i++)
            {
                if (string.IsNullOrEmpty(original[i]))
                {
                    continue;
                }

                if (original[i][0].Equals('-'))
                {
                    var key = original[i].Substring(1);
                    if (i + 1 < original.Length && !string.IsNullOrEmpty(original[i + 1]))
                    {
                        var value = original[i + 1];
                        if (!value.StartsWith('-'))
                        {
                            if (!Application.isBatchMode)
                            {
                                value = value.Replace("\\", "/");
                            }

                            args.Add(key, new ArgumentValue(key, value, ArgumentCategory.Command));
                            i++;
                            continue;
                        }
                    }

                    args.Add(key, ArgumentValue.Empty(key, ArgumentCategory.Command));
                }
            }

            return args;
        }
    }
}
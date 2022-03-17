using System.Collections.Generic;
using System.Text;

namespace AppBuilder
{
    public enum ArgumentCategory
    {
        None,
        Input,
        Require,
        Unity,
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
    }

    public class Arguments : Dictionary<string, string>
    {
    }

    public static class Environment
    {
        public static Dictionary<string, string> GetCommandLineArgs()
        {
            var args = new Dictionary<string, string>();

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
                        if (!original[i + 1][0].Equals('-'))
                        {
                            args.Add(key, original[i + 1]);
                            i++;
                            continue;
                        }
                    }

                    args.Add(key, string.Empty);
                }
            }

            return args;
        }
    }
}
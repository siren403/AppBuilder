using System.Collections.Generic;

namespace AppBuilder
{
    public static class Environment
    {
        public static Dictionary<string, string> GetCommandLineArgs()
        {
            var args = new Dictionary<string, string>();

            var original = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < original.Length; i++)
            {
                if (original[i][0].Equals('-'))
                {
                    if (i + 1 < original.Length && !original[i + 1][0].Equals('-'))
                    {
                        args.Add(original[i].Substring(1), original[i + 1]);
                        i++;
                    }
                    else
                    {
                        args.Add(original[i], string.Empty);
                    }
                }
            }

            return args;
        }
    }
}
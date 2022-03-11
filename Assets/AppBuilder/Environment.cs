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
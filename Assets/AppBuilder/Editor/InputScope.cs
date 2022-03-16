using System;
using System.Collections.Generic;

namespace AppBuilder
{
    public class InputScope : IDisposable
    {
        public Dictionary<string, string> Args { get; }

        public InputScope(Dictionary<string, string> args)
        {
            Args = args;
        }

        public void Dispose()
        {
        }
    }
}
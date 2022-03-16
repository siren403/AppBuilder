using System;

namespace AppBuilder
{
    
    [Flags]
    public enum ArgumentOptions
    {
        None = 0,
        Smart = 1,
        Directory = 2
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class InputAttribute : Attribute
    {
        public string Name { get; }
        public ArgumentOptions Options { get; }

        public InputAttribute(string name, ArgumentOptions options = ArgumentOptions.None)
        {
            Name = name;
            Options = options;
        }
    }
}
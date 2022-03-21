using System;

namespace AppBuilder
{
    public enum ArgumentOptions
    {
        None = 0,
        Dropdown = 1,
        Directory = 2,
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InputAttribute : Attribute
    {
        public string[] Values { get; }
        public string Name { get; }
        public ArgumentOptions Options { get; }

        public InputAttribute(string name, ArgumentOptions options = ArgumentOptions.None)
        {
            Name = name;
            Options = options;
        }

        public InputAttribute(string name, string[] values)
        {
            Name = name;
            Values = values;
            Options = ArgumentOptions.Dropdown;
        }
    }
}
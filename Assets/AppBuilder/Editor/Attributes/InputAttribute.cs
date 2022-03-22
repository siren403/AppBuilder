using System;

namespace AppBuilder
{
    public enum InputOptions
    {
        None = 0,
        Dropdown = 1,
        Directory = 2,
        File
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InputAttribute : Attribute
    {
        public string Value { get; protected set; }
        public string[] Values { get; protected set; }
        public string Name { get; protected set; }
        public InputOptions Options { get; protected set; }
        public string Extension { get; protected set; }

        public InputAttribute(string name, InputOptions options = InputOptions.None)
        {
            Name = name;
            Options = options;
        }

        public InputAttribute(string name, string[] values)
        {
            Name = name;
            Values = values;
            Options = InputOptions.Dropdown;
        }

        public InputAttribute(string name)
        {
            Name = name;
            Options = InputOptions.None;
        }
    }

    public class InputFileAttribute : InputAttribute
    {
        public InputFileAttribute(string name, string extension = "*") : base(name)
        {
            Name = name;
            Extension = extension;
            Options = InputOptions.File;
        }
    }

    public class InputStringAttribute : InputAttribute
    {
        public InputStringAttribute(string name, string defaultValue = null) : base(name)
        {
            Value = defaultValue ?? string.Empty;
            Options = InputOptions.None;
        }
    }
}
using System;
using System.IO;

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

        public InputAttribute(string name)
        {
            Name = name;
            Options = InputOptions.None;
        }

        public InputAttribute(string name, string defaultValue = null)
        {
            Name = name;
            Value = defaultValue ?? string.Empty;
            Options = InputOptions.None;
        }
    }

    public class FileAttribute : InputAttribute
    {
        public FileAttribute(string name, string extension = "*") : base(name)
        {
            Name = name;
            Extension = extension;
            Options = InputOptions.File;
        }
    }

    public class DirectoryAttribute : InputAttribute
    {
        public DirectoryAttribute(string name) : base(name)
        {
            Name = name;
            Options = InputOptions.Directory;
        }
    }

    public class VariantAttribute : InputAttribute
    {
        public VariantAttribute(params string[] values) : base("variant")
        {
            Values = values;
            Options = InputOptions.Dropdown;
        }
    }

    /// <summary>
    /// key: appsettings
    /// </summary>
    public class AppSettingsAttribute : InputAttribute
    {
        public AppSettingsAttribute() : base("appsettings")
        {
            Options = InputOptions.Directory;
        }
    }
}
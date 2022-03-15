using System;
using System.Collections.Generic;
using System.Text;

namespace AppBuilder
{
    public readonly struct BuildProperty
    {
        public BuildProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }
    }

    public class BuildConfigureRecorder
    {
        private readonly StringBuilder _builder = new();

        private readonly List<Action> _configureActions = new();

        private readonly List<BuildProperty> _configureMessages = new();

        public BuildProperty[] GetProperties() => _configureMessages.ToArray();

        public void Enqueue(Action execute, BuildProperty property)
        {
            _configureActions.Add(execute);
            Write(property);
        }

        public void Write(BuildProperty property)
        {
            _configureMessages.Add(property);
        }

        public void Write(string name, string value)
        {
            _configureMessages.Add(new BuildProperty(name, value));
        }

        public Action[] Export()
        {
            return _configureActions.ToArray();
        }
        
        public override string ToString()
        {
            _builder.Clear();
            foreach (var property in _configureMessages)
            {
                _builder.AppendLine($"{property.Name,-50}{property.Value,-50}");
            }

            return _builder.ToString();
        }
    }
}
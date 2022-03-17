using System;

namespace AppBuilder
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BuildAttribute : Attribute
    {
        public string Name { get; }

        public BuildAttribute(string name = null)
        {
            Name = name;
        }
    }
}
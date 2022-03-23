using System;

namespace AppBuilder
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BuildAttribute : Attribute
    {
        public string DisplayName { get; }

        public BuildAttribute(string displayName = null)
        {
            DisplayName = displayName;
        }
    }
}
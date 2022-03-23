using System;

namespace AppBuilder
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BuildAttribute : Attribute
    {
        public int Order { get; }
        public string DisplayName { get; }

        public BuildAttribute(string displayName = null, int order = 0)
        {
            DisplayName = displayName;
            Order = order;
        }
    }
}
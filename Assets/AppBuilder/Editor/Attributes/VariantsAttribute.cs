using System;

namespace AppBuilder
{
    [AttributeUsage(AttributeTargets.Method)]
    public class VariantsAttribute : Attribute
    {
        public string[] Keys { get; }

        public VariantsAttribute(params string[] keys)
        {
            Keys = keys;
        }
    }
}
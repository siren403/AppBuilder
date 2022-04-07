using System;
using System.Collections.Generic;

namespace AppBuilder
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class JobAttribute : Attribute
    {
        public Type JobType { get; }

        public JobAttribute(Type jobType)
        {
            JobType = jobType;
        }
    }
}
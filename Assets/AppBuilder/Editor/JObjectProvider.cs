using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AppBuilder
{
    public class EmptyProvider<TOptions> : IOptions<TOptions> where TOptions : class
    {
        public TOptions Value { get; } = null;
    }

    public class JObjectProvider<TOptions> : IOptions<TOptions> where TOptions : class
    {
        public TOptions Value { get; }

        public JObjectProvider(JObject source)
        {
            Value = source.ToObject<TOptions>();
        }
    }

}
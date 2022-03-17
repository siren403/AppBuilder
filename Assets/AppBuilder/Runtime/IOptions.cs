using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AppBuilder
{
    public interface IOptions<out TOptions> where TOptions : class
    {
        TOptions Value { get; }
    }

    public interface IOptions
    {
        T GetSection<T>(string key);
        IEnumerable<T> GetSections<T>(string key);

        bool TryGetSection<T>(string key, out T value);
        bool TryGetSections<T>(string key, out IEnumerable<T> values);

        string ToJson();
    }


    public class JObjectProvider : IOptions
    {
        private readonly JObject _source;

        public JObjectProvider(JObject source)
        {
            _source = source;
        }

        public JObjectProvider(string json)
        {
            _source = JObject.Parse(json);
        }

        public T GetSection<T>(string key)
        {
            return (_source.GetValue(key) ?? throw new NullReferenceException(key)).Value<T>();
        }

        public IEnumerable<T> GetSections<T>(string key)
        {
            return (_source.GetValue(key) ?? throw new NullReferenceException(key)).Values<T>();
        }

        public bool TryGetSection<T>(string key, out T value)
        {
            if (_source.TryGetValue(key, out var token))
            {
                value = token.Value<T>();
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetSections<T>(string key, out IEnumerable<T> values)
        {
            if (_source.TryGetValue(key, out var token))
            {
                values = token.Values<T>();
                return true;
            }

            values = null;
            return false;
        }

        public string ToJson()
        {
            return _source.ToString(Formatting.None);
        }
    }
}
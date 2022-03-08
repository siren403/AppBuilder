using Newtonsoft.Json.Linq;

namespace AppBuilder
{
    public interface IOptions<out TOptions> where TOptions : class
    {
        TOptions Value { get; }
    }

    public class JObjectProvider<TOptions> : IOptions<TOptions> where TOptions : class
    {
        public TOptions Value { get; }

        public JObjectProvider(JObject source)
        {
            Value = source.ToObject<TOptions>();
        }
    }

    public class EmptyProvider<TOptions> : IOptions<TOptions> where TOptions : class
    {
        public TOptions Value { get; } = null;
    }
}
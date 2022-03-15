using System.Collections.Generic;

namespace AppBuilder
{
    public interface IBuildContext
    {
        IOptions<T> GetConfiguration<T>() where T : class;
        T GetSection<T>(string key);
        IEnumerable<T> GetSections<T>(string key);

        string GetArgument(string key, string defaultValue = null);
    }
}
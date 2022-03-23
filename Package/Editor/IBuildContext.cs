using System.Collections.Generic;

namespace AppBuilder
{
    public interface IBuildContext
    {
        IOptions<T> GetConfiguration<T>() where T : class;
        
        IOptions GetConfiguration();
        
        T GetSection<T>(string key);
        bool TryGetSection<T>(string key, out T result);
        
        IEnumerable<T> GetSections<T>(string key);
        bool TryGetSections<T>(string key, out IEnumerable<T> result);

        string GetArgument(string key, string defaultValue = null);
        bool TryGetArgument(string key, out string arg);
    }
}
using System;
using System.Text;

namespace Builds
{
    [Serializable]
    public class AppSettings
    {
        public string Host;
        public string Package;
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"host: {Host}");
            return builder.ToString();
        }
    }
}
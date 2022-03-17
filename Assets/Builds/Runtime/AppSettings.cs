using System;
using System.Text;

namespace Builds
{
    public enum Platform
    {
        None = 0,
        GooglePlay,
        OneStore,
        AppStore,
    }

    [Serializable]
    public class AppSettings
    {
        public string Host;
        public string Package;
        public Platform Platform;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"host: {Host}");
            return builder.ToString();
        }
    }
}
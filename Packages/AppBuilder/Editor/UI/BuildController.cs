using System;
using System.Collections.Generic;
using System.Linq;

namespace AppBuilder.UI
{
    public class BuildController
    {
        public enum BuildMode
        {
            Build,
            Preview,
            Configure
        }

        private Dictionary<string, BuildInfo> _builds;
        public List<string> BuildNames => _builds?.Keys.ToList();

        public bool Any() => _builds.Any();

        public void Initialize()
        {
            _builds = BuildPlayer.GetBuilds().ToDictionary(build => build.FullName, build => build);
        } 

        public (BuildInfo build, BuildPlayer.Report report) ExecuteBuild(string buildName, BuildMode mode)
        {
            if (!_builds.TryGetValue(buildName, out var build))
            {
                throw new ArgumentNullException(buildName);
            }

            var inputArgs = new Arguments();

            var inputs = build.Inputs;
            foreach (var input in inputs)
            {
                var cachedValue = BuildCache.GetString(build, input.Name);
                if (cachedValue == "None")
                {
                    inputArgs[input.Name] = ArgumentValue.Empty(input.Name, ArgumentCategory.Input);
                }
                else if (string.IsNullOrEmpty(cachedValue))
                {
                    if (string.IsNullOrEmpty(input.Value))
                    {
                        inputArgs[input.Name] = ArgumentValue.Empty(input.Name, ArgumentCategory.Input);
                    }
                    else
                    {
                        inputArgs[input.Name] = new ArgumentValue(input.Name, input.Value, ArgumentCategory.Input);
                    }
                }
                else
                {
                    inputArgs[input.Name] = new ArgumentValue(input.Name, cachedValue, ArgumentCategory.Input);
                }
            }

            switch (mode)
            {
                case BuildMode.Preview:
                    inputArgs["mode"] = new ArgumentValue("mode", "preview", ArgumentCategory.Custom);
                    break;
                case BuildMode.Configure:
                    inputArgs["mode"] = new ArgumentValue("mode", "configure", ArgumentCategory.Custom);
                    break;
            }

            var report = BuildPlayer.Execute(build, inputArgs);
            return (build, report);
        }
    }
}
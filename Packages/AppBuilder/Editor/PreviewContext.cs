using System;
using System.Collections.Generic;

namespace AppBuilder
{
    public class PreviewContext
    {
        public BuildConfigureRecorder Recorder
        {
            set
            {
                _properties = value.GetProperties();
                _recorderLog = value.ToString();
            }
        }

        private BuildProperty[] _properties;
        private string _recorderLog;
        public BuildProperty[] Properties => _properties ?? Array.Empty<BuildProperty>();

        private Dictionary<string, string> _args;

        public Dictionary<string, string> Args
        {
            get => _args ?? new Dictionary<string, string>();
            set => _args = value;
        }

        public override string ToString()
        {
            return _recorderLog ?? string.Empty;
        }
    }
}
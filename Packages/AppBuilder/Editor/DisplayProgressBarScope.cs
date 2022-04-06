using System;
using UnityEditor;

namespace AppBuilder
{
    public class DisplayProgressBarScope : IDisposable
    {
        private readonly string _title;
        private readonly string _info;

        public float Progress
        {
            set => EditorUtility.DisplayProgressBar(_title, _info, value);
        }

        public void Update(string info, float progress)
        {
            EditorUtility.DisplayProgressBar(_title, info, progress);
        }

        public DisplayProgressBarScope(string title, string info, float progress)
        {
            _title = title;
            _info = info;
            Progress = progress;
        }

        public DisplayProgressBarScope(string title)
        {
            _title = title;
            _info = string.Empty;
            Progress = 0;
        }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
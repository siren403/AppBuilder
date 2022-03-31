using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Component = Editor.Component.Component;

namespace AppBuilder.UI
{
    public class PathField : Component
    {
        public enum PathType
        {
            Directory,
            File
        }

        protected override string Path => PackageInfo.GetPath($"Editor/UI/Component/{nameof(PathField)}");

        private readonly TextField _textField;
        private readonly Button _button;

        private void EnableClassByPathType(PathType type, bool enable)
        {
            switch (type)
            {
                case PathType.Directory:
                    EnableInClassList("path-directory", enable);
                    break;
                case PathType.File:
                    EnableInClassList("path-file", enable);
                    break;
            }
        }

        private void EnableClassByAllPathType(bool enable)
        {
            EnableClassByPathType(PathType.Directory, enable);
            EnableClassByPathType(PathType.File, enable);
        }

        private PathType _pathType;

        public PathType Type
        {
            set
            {
                EnableClassByAllPathType(false);
                EnableClassByPathType(value, true);
                _pathType = value;
            }
        }

        public string Value
        {
            set
            {
                _textField.value = value;
                _textField.tooltip = value;
            }
        }

        public string Extension { get; set; }

        public PathField()
        {
            _textField = this.Q<TextField>("path-text-field");
            _textField.isDelayed = true;
            _button = this.Q<Button>("path-button");

            _button.clicked += () =>
            {
                var path = _pathType switch
                {
                    PathType.Directory => GetDirectoryPath(),
                    PathType.File => GetFilePath(),
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(path))
                {
                    _textField.value = path.Replace("\\", "/");
                }
            };
        }

        protected override void Init()
        {
        }

        private string GetDirectoryPath()
        {
            var path = string.IsNullOrEmpty(_textField.value)
                ? Application.dataPath
                : _textField.value;

            return EditorUtility.OpenFolderPanel("Select Directory", path, string.Empty);
        }

        private string GetFilePath()
        {
            var path = string.IsNullOrEmpty(_textField.value)
                ? Application.dataPath
                : System.IO.Path.GetDirectoryName(_textField.value);

            return EditorUtility.OpenFilePanel("Select File", path, Extension);
        }

        public new class UxmlFactory : UxmlFactory<PathField, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            private readonly UxmlEnumAttributeDescription<PathType> _pathType = new() {name = "path-type"};
            private readonly UxmlStringAttributeDescription _extension = new() {name = "ext"};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is not PathField e) return;

                e.Type = _pathType.GetValueFromBag(bag, cc);
                e.Extension = _extension.GetValueFromBag(bag, cc);
            }
        }
    }
}
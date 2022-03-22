using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class Argument : VisualElement
    {
        private Label _labelKey;

        private TextField _inputField;
        private Button _btnFolder;

        private Label _labelValue;

        private DropdownField _dropdownField;
        private Button _btnFile;

        public bool IsValue
        {
            set => EnableInClassList("value", value);
            get => ClassListContains("value");
        }

        public bool IsInput
        {
            set => EnableInClassList("input", value);
            get => ClassListContains("input");
        }

        public bool IsFolder
        {
            set => EnableInClassList("folder", value);
            get => ClassListContains("folder");
        }

        public bool IsDropdown
        {
            set => EnableInClassList("dropdown", value);
            get => ClassListContains("dropdown");
        }

        public List<string> Choices
        {
            set => _dropdownField.choices = value;
        }

        public bool IsFile
        {
            set => EnableInClassList("file", value);
            get => ClassListContains("file");
        }

        private string _fileExtension = "*";

        public string FileExtension
        {
            set => _fileExtension = value;
        }

        public string Value
        {
            set
            {
                if (IsValue)
                {
                    _labelValue.text = value;
                    _labelValue.tooltip = _labelValue.text;
                }

                if (IsInput)
                {
                    _inputField.value = value;
                    _inputField.tooltip = _inputField.value;
                }

                if (IsDropdown)
                {
                    _dropdownField.value = value;
                    _dropdownField.tooltip = _dropdownField.value;
                }
            }
        }

        public Argument()
        {
            Load();
            Init();
        }

        public Argument(string key)
        {
            Load();
            Init();
            _labelKey.text = key;
            _labelKey.tooltip = _labelKey.text;
        }

        private void Load()
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AppBuilder/Editor/UI/Component/Argument.uxml");
            Add(visualTree.Instantiate());
        }

        private void Init()
        {
            _labelKey = this.Query("key").Children<Label>();

            _inputField = this.Query("input").Children<TextField>();
            _inputField.isDelayed = true;

            _btnFolder = this.Q<Button>("btn-folder");
            _labelValue = this.Query("value").Children<Label>();

            _btnFolder.clicked += () =>
            {
                var directory = EditorUtility.OpenFolderPanel("Select Directory",
                    Directory.GetCurrentDirectory(), string.Empty);
                if (!string.IsNullOrEmpty(directory))
                {
                    _inputField.value = directory.Replace("\\", "/");
                }
            };

            _dropdownField = this.Query("dropdown").Children<DropdownField>();

            _btnFile = this.Q<Button>("btn-file");
            _btnFile.clicked += () =>
            {
                var file = EditorUtility.OpenFilePanel("Select File", Directory.GetCurrentDirectory(), _fileExtension);
                if (!string.IsNullOrEmpty(file))
                {
                    _inputField.value = file.Replace("\\", "/");
                }
            };
        }

        public void RegisterInputChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            _inputField.RegisterValueChangedCallback(callback);
        }

        public void RegisterDropdownChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            _dropdownField.RegisterValueChangedCallback(callback);
        }

        public new class UxmlFactory : UxmlFactory<Argument>
        {
        }
    }
}
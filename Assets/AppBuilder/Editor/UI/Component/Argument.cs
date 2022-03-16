using System;
using System.IO;
using System.Linq;
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

        public bool IsInput
        {
            set => EnableInClassList("input", !value);
        }

        public bool IsFolder
        {
            set => EnableInClassList("folder", value);
        }

        public string Input
        {
            set
            {
                _inputField.value = value;
                _inputField.tooltip = _inputField.value;
            }
        }

        public string Value
        {
            set
            {
                _labelValue.text = value;
                if (ClassListContains("folder"))
                {
                    _labelValue.text = _labelValue.text.Replace("\\", "/");
                }

                _labelValue.tooltip = _labelValue.text;
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
            _labelKey = this.Query("key").Children<Label>().First();

            _inputField = this.Query("input").Children<TextField>().First();
            _inputField.isDelayed = true;

            _btnFolder = this.Q<Button>("btn-folder");
            _labelValue = this.Query("value").Children<Label>().First();

            _btnFolder.clicked += () =>
            {
                var directory = EditorUtility.OpenFolderPanel("Select Directory",
                    Directory.GetCurrentDirectory(), string.Empty);
                if (!string.IsNullOrEmpty(directory))
                {
                    _inputField.value = directory.Replace("\\", "/");
                }
            };

            IsInput = false;
        }

        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            _inputField.RegisterValueChangedCallback(callback);
        }


        public new class UxmlFactory : UxmlFactory<Argument>
        {
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Component = Editor.Component.Component;

namespace AppBuilder.UI
{
    public class DynamicInputField : Component
    {
        public enum InputType
        {
            None,
            Label,
            Directory,
            File,
            Dropdown
        }

        private InputType _inputType;

        public InputType Type
        {
            set
            {
                EnableClassByAllInputType(false);
                EnableClassByInputType(value, true);
                _inputType = value;
            }
        }

        public string Key
        {
            set
            {
                _keyLabel.text = value;
                _keyLabel.EnableTextTooltip();
            }
        }

        public string Value
        {
            set
            {
                switch (_inputType)
                {
                    case InputType.Label:
                        _valueLabel.text = value;
                        _valueLabel.EnableTextTooltip();
                        break;
                    case InputType.Directory:
                    case InputType.File:
                    case InputType.None:
                        _pathField.Value = value;
                        break;
                    case InputType.Dropdown:
                        _valueDropdown.value = value;
                        break;
                }
            }
        }

        public List<string> Choices
        {
            set => _valueDropdown.choices = value;
        }

        private readonly Label _keyLabel;
        private readonly Label _valueLabel;
        private readonly PathField _pathField;
        private readonly DropdownField _valueDropdown;

        public string Extension
        {
            set => _pathField.Extension = value;
        }

        public DynamicInputField()
        {
            this.AddResource($"{nameof(DynamicInputField)}.uxml");
            this.AddResource($"{nameof(DynamicInputField)}.uss");

            _keyLabel = this.Q<Label>("key");
            _valueLabel = this.Q<Label>("value-label");
            _pathField = this.Q<PathField>("value-path");
            _valueDropdown = this.Q<DropdownField>("value-dropdown");

            RegisterCallback<AttachToPanelEvent>(OnAttach);
        }

        private event EventCallback<ChangeEvent<string>> ChangedCallback;

        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback) =>
            ChangedCallback += callback;

        public void UnregisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback) =>
            ChangedCallback -= callback;

        private void OnAttach(AttachToPanelEvent e)
        {
            _pathField.RegisterValueChangedCallback(OnChangedPathField);
            _valueDropdown.RegisterValueChangedCallback(OnChangedDropdownField);

            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnDetach(DetachFromPanelEvent e)
        {
            _pathField.UnregisterValueChangedCallback(OnChangedPathField);
            _valueDropdown.UnregisterValueChangedCallback(OnChangedDropdownField);

            if (ChangedCallback != null)
            {
                foreach (var @delegate in ChangedCallback.GetInvocationList())
                {
                    ChangedCallback -= (EventCallback<ChangeEvent<string>>) @delegate;
                }
            }

            UnregisterCallback<AttachToPanelEvent>(OnAttach);
            UnregisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnChangedPathField(ChangeEvent<string> e)
        {
            ChangedCallback?.Invoke(e);
        }

        private void OnChangedDropdownField(ChangeEvent<string> e)
        {
            ChangedCallback?.Invoke(e);
        }

        private void EnableClassByAllInputType(bool enable)
        {
            for (InputType i = InputType.Label; i <= InputType.Dropdown; i++)
            {
                EnableClassByInputType(i, enable);
            }
        }

        private void EnableClassByInputType(InputType type, bool enable)
        {
            switch (type)
            {
                case InputType.Label:
                    EnableInClassList("enable-value-label", enable);
                    break;
                case InputType.Directory:
                    EnableInClassList("enable-value-path", enable);
                    _pathField.Type = PathField.PathType.Directory;
                    break;
                case InputType.File:
                    EnableInClassList("enable-value-path", enable);
                    _pathField.Type = PathField.PathType.File;
                    break;
                case InputType.Dropdown:
                    EnableInClassList("enable-value-dropdown", enable);
                    break;
                case InputType.None:
                    EnableInClassList("enable-value-path", enable);
                    _pathField.Type = PathField.PathType.None;
                    break;
            }
        }


        public new class UxmlFactory : UxmlFactory<DynamicInputField, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            private readonly UxmlStringAttributeDescription _key = new() {name = "key"};
            private readonly UxmlStringAttributeDescription _value = new() {name = "value"};
            private readonly UxmlEnumAttributeDescription<InputType> _inputType = new() {name = "input-type"};
            private readonly UxmlStringAttributeDescription _choices = new() {name = "choices"};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is not DynamicInputField e) return;
                e.Type = _inputType.GetValueFromBag(bag, cc);

                var choices = _choices.GetValueFromBag(bag, cc)
                    .Split(",")
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s.Trim())
                    .ToList();
                e.Choices = choices;

                e.Key = _key.GetValueFromBag(bag, cc);
                e.Value = _value.GetValueFromBag(bag, cc);
            }
        }
    }
}
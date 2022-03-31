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
            Label,
            Directory,
            File,
            Dropdown
        }

        protected override string Path => PackageInfo.GetPath($"Editor/UI/Component/{nameof(DynamicInputField)}");

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

        private readonly Label _valueLabel;
        private readonly PathField _pathField;
        private readonly DropdownField _valueDropdown;

        public DynamicInputField()
        {
            _valueLabel = this.Q<Label>("value-label");
            _pathField = this.Q<PathField>("value-path");
            _valueDropdown = this.Q<DropdownField>("value-dropdown");
        }

        protected override void Init()
        {
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
                    .Select(s=>s.Trim())
                    .ToList();
                e.Choices = choices;


                e.Value = _value.GetValueFromBag(bag, cc);
            }
        }
    }
}
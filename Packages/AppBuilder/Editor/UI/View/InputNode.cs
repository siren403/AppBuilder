using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class InputNode : Node
    {
        public InputNode()
        {
            Title = nameof(InputNode);
            contentContainer.AddResource(nameof(InputNode));
        }

        public void Render(List<ArgumentValue> args, Dictionary<string, InputAttribute> inputs,
            AppBuilderView.ICache cache)
        {
            contentContainer.Clear();
            foreach (var arg in args)
            {
                var field = new DynamicInputField()
                {
                    Key = arg.Key
                };
                switch (arg.Category)
                {
                    case ArgumentCategory.Input:
                        RenderInputArg(field, arg);
                        break;
                    case ArgumentCategory.Reserve:
                        field.Type = DynamicInputField.InputType.Label;
                        field.Key = $"* {arg.Key}";
                        field.Value = arg.Value;
                        break;
                    case ArgumentCategory.Custom:
                        field.Type = DynamicInputField.InputType.Label;
                        field.Key = $"- {arg.Key}";
                        field.Value = arg.Value;
                        break;
                    case ArgumentCategory.None:
                    default:
                        field.Type = DynamicInputField.InputType.Label;
                        field.Value = arg.Value;
                        break;
                }

                contentContainer.Add(field);
            }

            void RenderInputArg(DynamicInputField field, ArgumentValue arg)
            {
                if (!inputs.TryGetValue(arg.Key, out var input)) return;

                switch (input.Options)
                {
                    case InputOptions.Directory:
                        field.Type = DynamicInputField.InputType.Directory;
                        field.Value = cache.Get(arg.Key, arg.Value);
                        break;
                    case InputOptions.File:
                        field.Type = DynamicInputField.InputType.File;
                        field.Extension = input.Extension;
                        field.Value = cache.Get(arg.Key, arg.Value);
                        break;
                    case InputOptions.Dropdown:
                        field.Type = DynamicInputField.InputType.Dropdown;
                        field.Choices = _baseChoices.Concat(input.Values).ToList();
                        var dropdownValue = arg.Value;
                        cache.Set(arg.Key, dropdownValue);
                        if (string.IsNullOrEmpty(dropdownValue))
                        {
                            dropdownValue = "None";
                        }

                        field.Value = dropdownValue;
                        break;
                    default:
                        field.Type = DynamicInputField.InputType.None;
                        field.Value = cache.Get(arg.Key, input.Value);
                        break;
                }

                field.RegisterValueChangedCallback(e =>
                {
                    cache.Set(arg.Key, e.newValue);
                    var view = GetFirstAncestorOfType<AppBuilderView>();
                    view.ExecuteAndRender(BuildController.BuildMode.Preview);
                });
            }
        }

        private readonly string[] _baseChoices = new[] {"None"};

        public new class UxmlFactory : UxmlFactory<InputNode, UxmlTraits>
        {
        }
    }
}
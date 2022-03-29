using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace AppBuilder
{
    public static class VisualElementExtensions
    {
        public static Label EnableTextTooltip(this Label label, bool isEnable = true)
        {
            label.tooltip = isEnable ? label.text : string.Empty;
            return label;
        }

        public static void SetPickingMode(this Toggle toggle, PickingMode mode)
        {
            toggle.pickingMode = mode;
            toggle.Children().First().pickingMode = mode;
        }

        public static void Initialize(this DropdownField field,
            List<string> choices,
            int defaultIndex,
            Action<ChangeEvent<string>> callback,
            Action<string> setter = null,
            Func<string, string> getter = null,
            List<string> defaultChoices = null)
        {
            field.RegisterCallback<ChangeEvent<string>>(e =>
            {
                if (!field.choices.Contains(e.newValue))
                {
                    field.SetValueWithoutNotify(field.choices.First());
                }

                setter?.Invoke(field.value);
                callback(e);
            });
            if (choices.Any())
            {
                field.choices = choices;
                field.value = getter?.Invoke(choices[defaultIndex]);
            }
            else if (defaultChoices != null && defaultChoices.Any())
            {
                field.choices = defaultChoices;
                field.value = defaultChoices.First();
            }
        }
    }
}
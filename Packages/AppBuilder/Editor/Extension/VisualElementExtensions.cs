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
    }
}
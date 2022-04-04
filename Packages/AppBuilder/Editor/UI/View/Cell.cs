using System.Collections.Generic;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class Cell : VisualElement
    {
        private readonly Label _label;

        public string Text
        {
            set
            {
                _label.text = value;
                _label.EnableTextTooltip();
            }
        }

        public Cell()
        {
            this.AddResource(nameof(Cell));
            AddToClassList("cell");
            _label = this.Q<Label>();
        }

        public new class UxmlFactory : UxmlFactory<Cell, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            private readonly UxmlStringAttributeDescription _text = new() {name = "text"};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                if (ve is not Cell cell) return;
                cell.Text = _text.GetValueFromBag(bag, cc);
            }
        }
    }
}
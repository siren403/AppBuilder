using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class Node : VisualElement
    {
        private readonly Label _titleLabel;

        public sealed override VisualElement contentContainer { get; }

        protected string Title
        {
            set => _titleLabel.text = value;
        }

        private int _depth;

        public int Depth
        {
            set
            {
                if (_depth > 0)
                {
                    EnableInClassList($"depth-{_depth.ToString()}", false);
                }

                value = Mathf.Clamp(value, 0, 3);

                AddToClassList($"depth-{value.ToString()}");
                _depth = value;
            }
        }

        public Node()
        {
            this.AddClassByType<Node>();
            this.AddResource("Node.uxml");
            this.AddResource("Node.uss");

            _titleLabel = this.Query(classes: new[] {"container", "title"}).Children<Label>();

            contentContainer = this.Query(classes: new[] {"container", "content"});
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlIntAttributeDescription _depth = new() {name = "depth"};

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is not Node e) return;

                e.Depth = _depth.GetValueFromBag(bag, cc);
            }
        }
    }
}
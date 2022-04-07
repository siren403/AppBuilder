using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class BuildNode : Node
    {
        public BuildNode()
        {
            Title = "Build";
            this.AddClassByType<BuildNode>();
            contentContainer.AddResource(nameof(BuildNode));
        }

        public void Render(BuildProperty[] properties)
        {
            contentContainer.Clear();
            foreach (var property in properties)
            {
                if(property.Options == BuildPropertyOptions.SectionEnd) continue;
                var field = new DynamicInputField()
                {
                    Type = DynamicInputField.InputType.Label,
                    Key = property.Name,
                    Value = property.Value
                };
                contentContainer.Add(field);
                // switch (property.Options)
                // {
                //     case BuildPropertyOptions.SectionBegin:
                //         section = new VisualElement();
                //         section.AddToClassList("section");
                //
                //         var head = new Label(property.Name);
                //         head.AddToClassList("section-head-2");
                //         section.Add(head);
                //
                //         var content = new VisualElement();
                //         content.AddToClassList("section-content-2");
                //         section.Add(content);
                //
                //         preview.Add(section);
                //
                //         section = content;
                //         break;
                //     case BuildPropertyOptions.SectionEnd:
                //         section = null;
                //         break;
                //     default:
                //         var parent = section ?? preview;
                //         // parent.Add(CreateItem(property.Name, property.Value));
                //         parent.Add(new SettingProperty()
                //         {
                //             Key = property.Name,
                //             Value = property.Value
                //         });
                //         break;
                // }
            }
        }

        public new class UxmlFactory : UxmlFactory<BuildNode, UxmlTraits>
        {
        }
    }
}
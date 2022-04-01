using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class BuildNode : Node
    {
        public BuildNode()
        {
            Title = nameof(BuildNode);
            contentContainer.AddResource(nameof(BuildNode));
        }

        public new class UxmlFactory : UxmlFactory<BuildNode, UxmlTraits>
        {
        }
    }
}
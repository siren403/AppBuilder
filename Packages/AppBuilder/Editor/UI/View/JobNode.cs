using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class JobNode : Node
    {
        public JobNode()
        {
            Title = nameof(JobNode);
            contentContainer.AddResource(nameof(JobNode));
        }

        public new class UxmlFactory : UxmlFactory<JobNode, UxmlTraits>
        {
        }
    }
}
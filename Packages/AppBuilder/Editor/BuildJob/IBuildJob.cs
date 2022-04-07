using UnityEngine.UIElements;

namespace AppBuilder
{
    public interface IBuildJob
    {
        bool IsEnabled { get; set; }
        string Name { get; }
    }

    public interface IBuildJobRenderer
    {
        void Render(VisualElement content);
    }
}
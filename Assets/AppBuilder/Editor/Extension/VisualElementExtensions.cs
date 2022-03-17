using UnityEngine.UIElements;

namespace AppBuilder
{
    public static class VisualElementExtensions
    {
        public static void RemoveChildren(this VisualElement element)
        {
            for (int i = element.childCount - 1; i >= 0; i--)
            {
                element.RemoveAt(i);
            }
        }
    }
}
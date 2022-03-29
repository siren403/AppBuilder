using System;
using AppBuilder;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.Component
{
    public abstract class UIToolkitWindow : EditorWindow
    {
        protected void Add(Component component)
        {
            // new Component.Initializer().Initialize(component, this);
            rootVisualElement.Add(component);
        }

        private void CreateGUI()
        {
            Render();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var children = rootVisualElement.Query<Component>().ToList();
            children.Reverse();

            var initializer = new Component.Initializer();
            foreach (var child in children)
            {
                initializer.Initialize(child, this);
            }
        }

        protected abstract void Render();

        protected void Load(string uxml, string uss)
        {
            rootVisualElement.LoadAsset(uxml, uss);
            // var children = rootVisualElement.Query().Children<Component>().ToList();
            // var a = rootVisualElement.Query<Component>().ToList();
            // var children = rootVisualElement.Query<Component>().Descendents<Component>().ToList();
        }
    }
}
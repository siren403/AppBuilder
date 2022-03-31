using System;
using System.Collections.Generic;
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

        private T GetEvent<T>() where T : EventBase<T>, new()
        {
            var e = EventBase<T>.GetPooled();
            e.target = rootVisualElement;
            return e;
        }

        public EventProvider SendEvent<T>(out T e) where T : EventBase<T>, new()
        {
            e = GetEvent<T>();
            return new EventProvider(rootVisualElement, e);
        }

        public void RegisterCallback<TEventType>(
            EventCallback<TEventType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where TEventType : EventBase<TEventType>, new()
        {
            rootVisualElement.RegisterCallback(callback, useTrickleDown);
        }

        public readonly struct EventProvider : IDisposable
        {
            private readonly VisualElement _sender;
            private readonly EventBase _e;

            public EventProvider(VisualElement sender, EventBase e)
            {
                _sender = sender;
                _e = e;
            }

            public void Dispose()
            {
                _sender.SendEvent(_e);
                _e.Dispose();
            }
        }
    }
}
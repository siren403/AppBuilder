using System;
using System.Collections.Generic;
using System.Linq;
using AppBuilder;
using UniRx;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.Component
{
    public interface IStateStorage
    {
        int GetValue(string key, int defaultValue);
        void SetValue(string key, int value);
    }

    public interface IReactiveStateStorage : IStateStorage, IDisposable
    {
        ReactiveProperty<int> GetProperty(string key, int defaultValue = 0);
    }

    public class UniRxStorage : IReactiveStateStorage
    {
        private readonly Dictionary<string, ReactiveProperty<int>> _ints = new();

        public int GetValue(string key, int defaultValue)
        {
            if (_ints.TryGetValue(key, out var property))
            {
                return property.Value;
            }

            return defaultValue;
        }

        public void SetValue(string key, int value)
        {
            if (!_ints.ContainsKey(key))
            {
                _ints[key] = new ReactiveProperty<int>();
            }

            _ints[key].Value = value;
        }

        public ReactiveProperty<int> GetProperty(string key, int defaultValue = 0)
        {
            if (_ints.TryGetValue(key, out var property))
            {
                return property;
            }
            else
            {
                SetValue(key, defaultValue);
                return _ints[key];
            }
        }

        public void Dispose()
        {
            foreach (var property in _ints.Values)
            {
                property.Dispose();
            }

            _ints.Clear();
        }
    }

    public abstract class UIToolkitWindow : EditorWindow
    {
        public readonly IReactiveStateStorage State = new UniRxStorage();

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

        private void OnDestroy()
        {
            State.Dispose();
        }
    }
}
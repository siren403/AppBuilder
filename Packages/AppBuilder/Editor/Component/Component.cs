using System;
using UniRx;
using UnityEngine.UIElements;
using ObservableExtensions = UniRx.ObservableExtensions;

namespace Editor.Component
{
    public class Component : VisualElement
    {
        public class Initializer
        {
            public void Initialize(Component component, UIToolkitWindow window)
            {
                component.Window = window;
                component.Init();
            }
        }

        private UIToolkitWindow _window;
        protected UIToolkitWindow Window { get; set; }

        private CompositeDisposable _disposable;

        protected Component()
        {
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        protected virtual void Init()
        {
        }

        private void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            _disposable?.Dispose();
            _disposable = null;
        }

        protected void Add<T>(T child, out T element) where T : VisualElement
        {
            base.Add(child);
            element = child;
        }

        public void AddDispose(IDisposable disposable)
        {
            _disposable ??= new();
            _disposable.Add(disposable);
        }
    }

    public static class DisposableExtensions
    {
        public static void AddTo(this IDisposable disposable, Component component)
        {
            component.AddDispose(disposable);
        }
    }

    public static class ReactivePropertyExtensions
    {
        public static IDisposable SubscribeToLabel(this ReactiveProperty<int> property, Label label)
        {
            return ObservableExtensions.Subscribe(property, _ => label.text = _.ToString());
        }
    }
}
using UniRx;
using UnityEditor;
using UnityEngine.UIElements;
using PackageInfo = AppBuilder.UI.PackageInfo;

namespace Editor.Component
{
    public class BWindow : UIToolkitWindow, SharedCounter.ICounterContext
    {
        [MenuItem("AppBuilder/BWindow")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<BWindow>();
            window.Show();
        }

        protected override void Render()
        {
            Load(
                PackageInfo.GetPath("Editor/Component/BWindow.uxml"),
                PackageInfo.GetPath("Editor/Component/BWindow.uss")
            );
            Add(new SharedCounter());
        }

        ReactiveProperty<int> SharedCounter.ICounterContext.Count { get; set; } = new();
    }

    #region BWindow

    public class CounterDisposeButton : Component
    {
        private readonly Button _button;

        public CounterDisposeButton()
        {
            Add(new Button() {text = "dispose"}, out _button);
        }

        protected override void Init()
        {
            if (Window is SharedCounter.ICounterContext context)
            {
                _button.clicked += () => context.Count.Dispose();
            }
        }

        public new class UxmlFactory : UxmlFactory<CounterDisposeButton>
        {
        }
    }

    public class SharedCounter : Component
    {
        public interface ICounterContext
        {
            ReactiveProperty<int> Count { get; set; }
        }

        private readonly Label _labelCount;
        private readonly Button _button;

        public SharedCounter()
        {
            style.flexDirection = FlexDirection.Row;

            Add(new Label("0"), out _labelCount);
            Add(new Button()
            {
                text = "Add"
            }, out _button);
            Add(new Button(RemoveFromHierarchy) {name = "X"});
        }

        protected override void Init()
        {
            var property = State.GetProperty(nameof(SharedCounter), 0);
            property.SubscribeToLabel(_labelCount).AddTo(this);
            _button.clicked += () => { property.Value++; };


            // if (!(Window is ICounterContext context)) return;
            //
            // context.Count.SubscribeToLabel(_labelCount).AddTo(this);
            // _button.clicked += () => { context.Count.Value++; };
        }

        public new class UxmlFactory : UxmlFactory<SharedCounter>
        {
        }
    }

    #endregion
}
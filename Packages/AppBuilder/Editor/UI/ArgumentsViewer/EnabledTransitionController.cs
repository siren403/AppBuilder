using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class EnabledTransitionController
    {
        public enum TransitionState
        {
            None,
            In,
            Out
        }

        private readonly VisualElement _element;
        private TransitionState _state;

        public EnabledTransitionController(VisualElement element)
        {
            _state = TransitionState.None;

            _element = element;
            _element.SetEnabled(false);
            _element.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        }

        public void In()
        {
            if (_state != TransitionState.None) return;
            _state = TransitionState.In;
            _element.SetEnabled(true);
        }

        private void OnTransitionEnd(TransitionEndEvent e)
        {
            switch (_state)
            {
                case TransitionState.In:
                    _element.SetEnabled(false);
                    _state = TransitionState.Out;
                    break;
                case TransitionState.Out:
                    _state = TransitionState.None;
                    break;
            }
        }
    }
}
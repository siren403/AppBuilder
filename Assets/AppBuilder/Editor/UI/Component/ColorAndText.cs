using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class ColorAndText : VisualElement
    {
        public Color Color
        {
            get => _colorField.value;
            set
            {
                _colorField.value = value;
                _label.text = _colorField.value.ToString();
            }
        }

        private readonly Label _label;
        private readonly ColorField _colorField;

        public ColorAndText()
        {
            // 레이아웃 조정 (가로 & 위아래 중앙) 
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            style.alignItems = new StyleEnum<Align>(Align.Center);

            // ColorField를 자식 요소로 추가 
            _colorField = new ColorField();
            SetMargin(_colorField, 0); // 자식 요소의 Margin은 스크립트에서 USS로 변경 되지 않도록 정의 합니다.
            SetPadding(_colorField, 0);
            _colorField.style.width = new StyleLength(new Length(50, LengthUnit.Percent));
            Add(_colorField);

            // Label을 자식 요소로 추가 
            _label = new Label();
            SetMargin(_label, 0);
            SetPadding(_label, 0);
            _label.style.width = new StyleLength(new Length(50, LengthUnit.Percent));
            _label.text = _colorField.value.ToString();
            Add(_label);

            _colorField.RegisterValueChangedCallback(x => _label.text = x.newValue.ToString());
        }

        private static void SetMargin(VisualElement element, float px)
        {
            element.style.marginLeft = px;
            element.style.marginTop = px;
            element.style.marginRight = px;
            element.style.marginBottom = px;
        }

        private static void SetPadding(VisualElement element, float px)
        {
            element.style.paddingLeft = px;
            element.style.paddingTop = px;
            element.style.paddingRight = px;
            element.style.paddingBottom = px;
        }

        // 팩토리로 사용되는 클래스
        public new class UxmlFactory : UxmlFactory<ColorAndText, UxmlTraits>
        {
        }

        // 팩토리에 의한 ColorAndText의 초기화시에 사용하는 클래스 
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            // UXML 속성 정의 
            readonly UxmlColorAttributeDescription _initialColor = new() {name = "initial-color"};

            // 아이를 가지지 않는 경우는 이렇게 쓰는 
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            // 초기화 처리 
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is not ColorAndText colorAndText) return;
                
                // UXML의 속성에 들어있는 값을 대입한다
                colorAndText.Color = _initialColor.GetValueFromBag(bag, cc);
                colorAndText._label.text = colorAndText._colorField.value.ToString();
            }
        }
    }
}
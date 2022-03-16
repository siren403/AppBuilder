using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace AppBuilder.UI
{
    public class StatusBar : VisualElement
    {
        private string _status;

        public StatusBar()
        {
            _status = string.Empty;
        }

        public string Status { get; set; }

        public new class UxmlFactory : UxmlFactory<StatusBar, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription _status = new() {name = "status"};

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get
                {
                    yield break;
                    // yield return new UxmlChildElementDescription(typeof(VisualElement));
                }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((StatusBar) ve).Status = _status.GetValueFromBag(bag, cc);
                Debug.Log(((StatusBar) ve).Status);
            }
        }
    }
}
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace AppBuilder.UI
{
    public class SettingProperty : VisualElement
    {
        private Label _labelKey;
        private Label _labelValue;
        private Toggle _toggleValue;

        public string Key
        {
            set => _labelKey.text = value;
        }

        public string Value
        {
            set
            {
                var isBoolean = value is "true" or "false";
                EnableInClassList("boolean", isBoolean);
                EnableInClassList("string", !isBoolean);
                if (isBoolean)
                {
                    _toggleValue.value = value is "true";
                }
                else
                {
                    _labelValue.text = value;
                    _labelValue.EnableTextTooltip();
                }
            }
        }

        public SettingProperty()
        {
            Load();
            Init();
        }

        public SettingProperty(string key)
        {
            Load();
            Init();
        }

        private void Load()
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/AppBuilder/Editor/UI/Component/SettingProperty.uxml");
            Add(visualTree.Instantiate());
        }

        private void Init()
        {
            _labelKey = this.Q<Label>("key");
            _labelValue = this.Q<Label>("label-value");

            _toggleValue = this.Q<Toggle>("toggle-value");
            _toggleValue.SetPickingMode(PickingMode.Ignore);
        }

        public new class UxmlFactory : UxmlFactory<SettingProperty>
        {
        }
    }
}
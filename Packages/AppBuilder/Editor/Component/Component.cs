using System;
using System.Text.RegularExpressions;
using AppBuilder;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Component
{
    public static class StringExtensions
    {
        public static string PascalToKebabCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            //support number pattern : "(?<!^)([AZ][az]|(?<=[az])[A-Z0-9])" 
            return Regex.Replace(
                    value,
                    "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                    "-$1",
                    RegexOptions.Compiled)
                .Trim()
                .ToLower();
        }
    }

    public abstract class Component : VisualElement
    {
        protected virtual string Path { get; } = string.Empty;
        protected virtual string UXML { get; } = string.Empty;
        protected virtual string USS { get; } = string.Empty;

        protected Component()
        {
            AddToClassList(GetType().Name.PascalToKebabCase());

            // var path = Path;
            // string uxml;
            // string uss;
            //
            // if (!string.IsNullOrEmpty(path))
            // {
            //     uxml = path;
            //     uss = path;
            // }
            // else
            // {
            //     uxml = UXML;
            //     uss = USS;
            // }
            //
            // if (!string.IsNullOrEmpty(uxml)) this.LoadUXML(uxml);
            // if (!string.IsNullOrEmpty(uss)) this.LoadUSS(uss);
            // RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent e)
        {
        }

    }
}
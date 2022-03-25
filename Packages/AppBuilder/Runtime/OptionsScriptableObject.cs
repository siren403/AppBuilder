using System;
using System.Collections.Generic;
using AppBuilder;
using UnityEditor;
using UnityEngine;

namespace AppBuilder
{
    public abstract class OptionsScriptableObject<TOptions> : ScriptableObject, IOptions<TOptions>, IOptions
        where TOptions : class
    {
        [SerializeField] [InspectorName("Value")]
        private TOptions optionsValue;

        public TOptions Value
        {
            get => optionsValue;
            set => optionsValue = value;
        }

        [SerializeField] [HideInInspector] private string json;

        public string Json
        {
            set
            {
                json = value;
            }
        }

#if UNITY_EDITOR
        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
#endif

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(json))
            {
                _optionsImplementation = new JObjectProvider(json);
            }
        }

        private void OnDisable()
        {
            _optionsImplementation = null;
        }

        private IOptions _optionsImplementation;

        private IOptions Options
        {
            get
            {
                if (_optionsImplementation == null)
                {
                    throw new NullReferenceException("require WithScriptable(...,withJson)");
                }

                return _optionsImplementation;
            }
        }

        public T GetSection<T>(string key)
        {
            return Options.GetSection<T>(key);
        }

        public IEnumerable<T> GetSections<T>(string key)
        {
            return Options.GetSections<T>(key);
        }

        public bool TryGetSection<T>(string key, out T value)
        {
            try
            {
                return Options.TryGetSection(key, out value);
            }
            catch
            {
                value = default;
                return false;
            }
        }

        public bool TryGetSections<T>(string key, out IEnumerable<T> values)
        {
            try
            {
                return Options.TryGetSections(key, out values);
            }
            catch
            {
                values = null;
                return false;
            }
        }

        public string ToJson()
        {
            return Options.ToJson();
        }
    }
}
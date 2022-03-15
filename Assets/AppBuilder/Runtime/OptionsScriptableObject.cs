using System;
using AppBuilder;
using UnityEditor;
using UnityEngine;

namespace Builds
{
    public abstract class OptionsScriptableObject<TOptions> : ScriptableObject, IOptions<TOptions>
        where TOptions : class
    {
        [SerializeField] private TOptions value;

        public TOptions Value
        {
            get => value;
            set
            {
                this.value = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
#endif
            }
        }
    }
}
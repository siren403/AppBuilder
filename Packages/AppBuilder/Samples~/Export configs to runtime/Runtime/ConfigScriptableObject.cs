using AppBuilder;
using UnityEngine;

namespace AppBuilderSample
{
    [CreateAssetMenu(menuName = "AppBuilderSample/ExportConfigs", fileName = "Config", order = 0)]
    public class ConfigScriptableObject : OptionsScriptableObject<Config>
    {
        
    }
}
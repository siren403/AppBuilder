using UnityEngine;

namespace Builds
{
    [CreateAssetMenu(menuName = "Builds/AppSettings", fileName = "AppSettings", order = 0)]
    public class AppSettingsScriptableObject : OptionsScriptableObject<AppSettings>
    {
    }
}
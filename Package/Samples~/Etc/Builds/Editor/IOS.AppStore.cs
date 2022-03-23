using AppBuilder;
using UnityEngine;

namespace Builds
{
    public static partial class IOS
    {
        // [Build]
        [AppSettings("{projectpath}/Assets/AppBuilder/Samples/Etc/Builds")]
        public static void AppStore()
        {
            BuildPlayer.Build((ctx, builder) => { Debug.Log("Execute Method"); });
        }
    }
}
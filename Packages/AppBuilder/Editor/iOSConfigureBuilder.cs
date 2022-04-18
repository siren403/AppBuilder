using UnityEditor;

namespace AppBuilder
{
    public readonly struct iOSConfigureBuilder
    {
        private readonly BuildConfigureRecorder _recorder;

        public iOSConfigureBuilder(BuildConfigureRecorder recorder)
        {
            _recorder = recorder;
        }

        public void TargetSdk(iOSSdkVersion version)
        {
            _recorder.Enqueue(() => { PlayerSettings.iOS.sdkVersion = version; },
                new BuildProperty(nameof(TargetSdk), version.ToString()));
        }

        public void TargetVersion(string version)
        {
            _recorder.Enqueue(() => { PlayerSettings.iOS.targetOSVersionString = version; },
                new BuildProperty(nameof(TargetVersion), version));
        }
    }
}
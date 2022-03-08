using UnityEditor;

namespace AppBuilder
{
    public readonly struct AndroidSettingsBuilder
    {
        public void IL2CPP() =>
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        public void Mono() =>
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="architecture">ARMv7 = 32bit, ARM64 = 64bit</param>
        public void Architectures(AndroidArchitecture architecture)
        {
            PlayerSettings.Android.targetArchitectures = architecture;
        }
        // ditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle; //저는 그래들로 합니다. 인터널로 하실 수도 있어요 
        // EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release; // 디벨롭이 필요하시면 디벨롭으로 하시면 됩니다. 

        public void PackageName(string package)
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, package);
        }

        public void SupportEmulator()
        {
            PlayerSettings.Android.optimizedFramePacing = false;
        }
    }
}
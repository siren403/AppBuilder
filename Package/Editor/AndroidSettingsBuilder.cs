using System;
using System.Text;
using UnityEditor;

namespace AppBuilder
{
    //TODO: generic templetes
    /*
     * public void IL2CPP(), Mono()
     * ->
     * public void Enqueue<TAndroidConfigure>();
     * public interface IConfigure {
     *      string Name {get;}
     *      void Execute();
     * }
     * public class IL2CPP : IAndroidConfigure
     * {
     *      public string Name => "";
     *      public void Execute(){
     *          PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
     *      }
     * }
     */
    public readonly struct AndroidSettingsBuilder
    {
        private readonly BuildConfigureRecorder _recorder;

        public AndroidSettingsBuilder(BuildConfigureRecorder recorder)
        {
            _recorder = recorder;
        }

        public void IL2CPP()
        {
            _recorder.Enqueue(
                () => { PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP); },
                new BuildProperty(
                    "PlayerSettings:SetScriptingBackend",
                    "(Android, IL2CPP)"
                )
            );
        }

        public void Mono()
        {
            _recorder.Enqueue(
                () => { PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x); },
                new BuildProperty(
                    "PlayerSettings:SetScriptingBackend",
                    "(Android, Mono)"
                )
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="architecture">ARMv7 = 32bit, ARM64 = 64bit</param>
        public void Architectures(AndroidArchitecture architecture)
        {
            _recorder.Enqueue(
                () => { PlayerSettings.Android.targetArchitectures = architecture; },
                new BuildProperty(
                    "PlayerSettings:Android:targetArchitectures", architecture.ToString()
                )
            );
        }

        // ditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle; //저는 그래들로 합니다. 인터널로 하실 수도 있어요 
        // EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release; // 디벨롭이 필요하시면 디벨롭으로 하시면 됩니다. 

        public void PackageName(string package)
        {
            _recorder.Enqueue(
                () => { PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, package); },
                new BuildProperty(
                    "Android:PackageName", package
                )
            );
        }

        public void SupportEmulator()
        {
            _recorder.Enqueue(
                () => { PlayerSettings.Android.optimizedFramePacing = false; },
                new BuildProperty(
                    "PlayerSettings:Android:optimizedFramePacing", "false"
                )
            );
        }

        public void UseDebugKeystore()
        {
            _recorder.Enqueue(
                () => { PlayerSettings.Android.useCustomKeystore = false; },
                new BuildProperty(
                    "PlayerSettings:Android:useCustomKeystore", "false"
                )
            );
        }

        public void UseCustomKeystore(string path, string passwd, string alias, string aliasPasswd)
        {
            _recorder.Enqueue(
                () =>
                {
                    PlayerSettings.Android.useCustomKeystore = true;
                    PlayerSettings.Android.keystoreName = path;
                    PlayerSettings.Android.keystorePass = passwd;
                    PlayerSettings.Android.keyaliasName = alias;
                    PlayerSettings.Android.keyaliasPass = aliasPasswd;
                },
                "Keystore",
                new BuildProperty(
                    "PlayerSettings:Android:useCustomKeystore", "true"
                ),
                new BuildProperty(
                    "PlayerSettings:Android:keystoreName", path
                ),
                new BuildProperty(
                    "PlayerSettings:Android:keystorePass", passwd
                ),
                new BuildProperty(
                    "PlayerSettings:Android:keyaliasName", alias
                ),
                new BuildProperty(
                    "PlayerSettings:Android:keyaliasPass", aliasPasswd
                )
            );
        }

        public void EnableKeystore(bool isEnable, string path = null, string passwd = null, string alias = null,
            string aliasPasswd = null)
        {
            if (isEnable)
            {
                UseCustomKeystore(path, passwd, alias, aliasPasswd);
            }
            else
            {
                UseDebugKeystore();
            }
        }
    }
}
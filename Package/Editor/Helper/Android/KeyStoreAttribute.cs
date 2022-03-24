using System;

namespace AppBuilder.Helper.Android
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class KeyStoreAttribute : FileAttribute
    {
        /// <summary>
        /// 프로젝트 Root 기준으로도 가능.
        /// ex. production.keystore -> [Project]/production.keystore
        ///     Assets/production.keystore -> [Project]/Assets/production.keystore
        /// 외부는 절대 경로.
        /// </summary>
        public KeyStoreAttribute(string defaultPath) : base(AndroidConfigureBuilderExtensions.Keystore, "keystore")
        {
            Value = defaultPath;
        }
    }

    namespace KeyStore
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class PasswordAttribute : InputAttribute
        {
            public PasswordAttribute(string defaultValue = null) : base(
                AndroidConfigureBuilderExtensions.KeystorePassword, defaultValue)
            {
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class AliasAttribute : InputAttribute
        {
            public AliasAttribute(string defaultValue = null) : base(AndroidConfigureBuilderExtensions.Alias,
                defaultValue)
            {
            }
        }

        namespace Alias
        {
            [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
            public class PasswordAttribute : InputAttribute
            {
                public PasswordAttribute(string defaultValue = null) : base(
                    AndroidConfigureBuilderExtensions.AliasPassword, defaultValue)
                {
                }
            }
        }
    }
}
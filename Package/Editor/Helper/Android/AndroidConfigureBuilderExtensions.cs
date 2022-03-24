namespace AppBuilder.Helper.Android
{
    public static class AndroidConfigureBuilderExtensions
    {
        public const string Keystore = "keystore";
        public const string KeystorePassword = "keystorePasswd";
        public const string Alias = "keystoreAlias";
        public const string AliasPassword = "keystoreAliasPasswd";

        public static void UseKeyStore(this AndroidConfigureBuilder builder, IBuildContext context)
        {
            builder.EnableKeystore(context.TryGetArgument(Keystore, out var keystorePath),
                keystorePath,
                context.GetArgument(KeystorePassword),
                context.GetArgument(Alias),
                context.GetArgument(AliasPassword));
        }
    }
}
using AppBuilder.UI;

namespace AppBuilder
{
    public static class ArgumentValueExtensions
    {
        public static ArgumentValue Format(this ArgumentValue arg, Arguments args)
        {
            return new ArgumentValue(arg.Key, String.Format(arg.Value, args), arg.Category);
        }
    }
}
namespace AppBuilder
{
    public interface IOptions<out TOptions> where TOptions : class
    {
        TOptions Value { get; }
    }

 
}
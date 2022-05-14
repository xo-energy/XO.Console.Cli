namespace XO.Console.Cli;

internal sealed class DefaultTypeResolver : ITypeResolver
{
    public static readonly ITypeResolver Instance = new DefaultTypeResolver();

    private DefaultTypeResolver()
    {

    }

    public object? Get(Type type)
    {
        return Activator.CreateInstance(type);
    }

    public T? Get<T>()
    {
        return Activator.CreateInstance<T>();
    }
}

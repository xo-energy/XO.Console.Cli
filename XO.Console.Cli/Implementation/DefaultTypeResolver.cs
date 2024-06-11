using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli.Implementation;

internal sealed class DefaultTypeResolver : ITypeResolver
{
    public static readonly ITypeResolver Instance = new DefaultTypeResolver();

    private DefaultTypeResolver()
    {

    }

    public object? Get([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
    {
        return Activator.CreateInstance(type);
    }

    public T? Get<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
    {
        return Activator.CreateInstance<T>();
    }
}

using System.Collections.Immutable;

namespace XO.Console.Cli;

internal static class ImmutableArrayEqualityComparer
{
    public static bool Equals<T>(ImmutableArray<T> x, ImmutableArray<T> y)
        => ImmutableArrayEqualityComparer<T>.Default.Equals(x, y);

    public static int GetHashCode<T>(ImmutableArray<T> array)
        => ImmutableArrayEqualityComparer<T>.Default.GetHashCode(array);
}

internal sealed class ImmutableArrayEqualityComparer<T> : IEqualityComparer<ImmutableArray<T>>
{

    private readonly IEqualityComparer<T> _equalityComparer;

    public static ImmutableArrayEqualityComparer<T> Default { get; }
        = new(EqualityComparer<T>.Default);

    public ImmutableArrayEqualityComparer(IEqualityComparer<T> equalityComparer)
    {
        _equalityComparer = equalityComparer;
    }

    public bool Equals(ImmutableArray<T> x, ImmutableArray<T> y)
    {
        if (x.Equals(y))
            return true;

        if (x.IsDefaultOrEmpty || y.IsDefaultOrEmpty)
            return false;

        if (x.Length != y.Length)
            return false;

        for (int i = 0; i < x.Length; i++)
        {
            if (!_equalityComparer.Equals(x[i], y[i]))
                return false;
        }

        return true;
    }

    public int GetHashCode(ImmutableArray<T> array)
    {
        if (array.IsDefaultOrEmpty)
            return array.GetHashCode();

        var hash = HashCode.Initialize();

        for (int i = 0; i < array.Length; i++)
            hash = HashCode.Add(hash, _equalityComparer.GetHashCode(array[i]));

        return hash;
    }
}

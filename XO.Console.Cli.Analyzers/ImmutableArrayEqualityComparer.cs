using System.Collections.Immutable;

namespace XO.Console.Cli;

internal static class ImmutableArrayEqualityComparer
{
    public static bool Equals<T>(ImmutableArray<T> x, ImmutableArray<T> y)
        => ImmutableArrayEqualityComparer<T>.Default.Equals(x, y);

    public static bool Equals<T>(ImmutableList<T>? x, ImmutableList<T>? y)
    {
        if (Object.Equals(x, y))
            return true;

        if (x == null || y == null)
            return false;

        if (x.Count != y.Count)
            return false;

        for (int i = 0; i < x.Count; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(x[i], y[i]))
                return false;
        }

        return true;
    }

    public static int GetHashCode<T>(ImmutableArray<T> array)
        => ImmutableArrayEqualityComparer<T>.Default.GetHashCode(array);

    public static int GetMatchLength<T>(ImmutableArray<T> x, ImmutableArray<T> y)
    {
        if (x.IsDefault || y.IsDefault)
            return 0;

        var length = 0;

        for (int i = 0; i < x.Length && i < y.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(x[i], y[i]))
                length++;
        }

        return length;
    }
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

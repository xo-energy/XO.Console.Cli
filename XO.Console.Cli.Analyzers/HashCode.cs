namespace XO.Console.Cli;

internal static class HashCode
{
    private const int Prime1 = 5279;
    private const int Prime2 = 3010349;

    public static int Add(int hash, int addend)
    {
        hash = unchecked(hash * Prime2 + addend);
        return hash;
    }

    public static int Add<T>(int hash, T value)
        => Add(hash, value?.GetHashCode() ?? 0);

    public static int Combine<T1, T2>(T1 value1, T2 value2)
    {
        var hash = Prime1;
        hash = unchecked(hash * Prime2 + value1?.GetHashCode() ?? 0);
        hash = unchecked(hash * Prime2 + value2?.GetHashCode() ?? 0);
        return hash;
    }

    public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        var hash = Prime1;
        hash = unchecked(hash * Prime2 + value1?.GetHashCode() ?? 0);
        hash = unchecked(hash * Prime2 + value2?.GetHashCode() ?? 0);
        hash = unchecked(hash * Prime2 + value3?.GetHashCode() ?? 0);
        return hash;
    }

    public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        var hash = Prime1;
        hash = unchecked(hash * Prime2 + value1?.GetHashCode() ?? 0);
        hash = unchecked(hash * Prime2 + value2?.GetHashCode() ?? 0);
        hash = unchecked(hash * Prime2 + value3?.GetHashCode() ?? 0);
        hash = unchecked(hash * Prime2 + value4?.GetHashCode() ?? 0);
        return hash;
    }

    public static int Initialize()
        => Prime1;
}

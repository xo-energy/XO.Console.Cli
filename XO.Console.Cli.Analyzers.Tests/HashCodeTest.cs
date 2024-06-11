namespace XO.Console.Cli;

public sealed class HashCodeTest
{
    [Fact]
    public void Add_IsEqualToCombine()
    {
        var hash1 = HashCode.Combine("a", "b");
        var hash2 = HashCode.Initialize();

        hash2 = HashCode.Add(hash2, "a");
        hash2 = HashCode.Add(hash2, "b");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Combine2_IsDifferent()
    {
        var hash1 = HashCode.Combine("a", "b");
        var hash2 = HashCode.Combine("a", "z");
        var hash3 = HashCode.Combine("z", "b");

        Assert.Multiple(
            () => Assert.NotEqual(hash1, hash2),
            () => Assert.NotEqual(hash1, hash3));
    }

    [Fact]
    public void Combine2_IsSame()
    {
        var hash1 = HashCode.Combine("a", "b");
        var hash2 = HashCode.Combine("a", "b");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Combine3_IsDifferent()
    {
        var hash1 = HashCode.Combine("a", "b", "c");
        var hash2 = HashCode.Combine("z", "b", "c");
        var hash3 = HashCode.Combine("a", "z", "c");
        var hash4 = HashCode.Combine("a", "b", "z");

        Assert.Multiple(
            () => Assert.NotEqual(hash1, hash2),
            () => Assert.NotEqual(hash1, hash3),
            () => Assert.NotEqual(hash1, hash4));
    }

    [Fact]
    public void Combine3_IsSame()
    {
        var hash1 = HashCode.Combine("a", "b", "c");
        var hash2 = HashCode.Combine("a", "b", "c");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Combine4_IsDifferent()
    {
        var hash1 = HashCode.Combine("a", "b", "c", "d");
        var hash2 = HashCode.Combine("z", "b", "c", "d");
        var hash3 = HashCode.Combine("a", "z", "c", "d");
        var hash4 = HashCode.Combine("a", "b", "z", "d");
        var hash5 = HashCode.Combine("a", "b", "c", "z");

        Assert.Multiple(
            () => Assert.NotEqual(hash1, hash2),
            () => Assert.NotEqual(hash1, hash3),
            () => Assert.NotEqual(hash1, hash4),
            () => Assert.NotEqual(hash1, hash5));
    }

    [Fact]
    public void Combine4_IsSame()
    {
        var hash1 = HashCode.Combine("a", "b", "c", "d");
        var hash2 = HashCode.Combine("a", "b", "c", "d");

        Assert.Equal(hash1, hash2);
    }
}

using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

public sealed class ParametersTypeModelTest
{
    [Fact]
    public void Equals_ReturnsFalse_WhenOtherIsNull()
    {
        Assert.False(Fixtures.Empty.Equals(null));
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenOtherIsSame()
    {
        Assert.True(Fixtures.Empty.Equals(Fixtures.Empty));
    }

    [Fact]
    public void GetHashCode_ReturnsHashCodeOfName()
    {
        Assert.Equal(Fixtures.Empty.Name.GetHashCode(), Fixtures.Empty.GetHashCode());
    }

    private static class Fixtures
    {
        public static readonly ParametersTypeModel Empty
            = new ParametersTypeModel(
                "name",
                ImmutableList<CommandArgumentModel>.Empty,
                ImmutableList<CommandOptionModel>.Empty);
    }
}

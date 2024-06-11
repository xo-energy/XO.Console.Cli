using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

public sealed class CommandAttributeDataTest
{
    public static TheoryData<ImmutableArray<string>, ImmutableArray<string>, string?, bool, string?> GetConstructorParameters()
    {
        return new() {
            { ["set"], ["stuff"], "description", false, "Parameters" },
            { ["get", "things"], ["junk"], "description", false, "Parameters" },
            { ["get", "things"], ["stuff"], "cool command", false, "Parameters" },
            { ["get", "things"], ["stuff"], "description", true, "Parameters" },
            { ["get", "things"], ["stuff"], "description", false, "Args" },
        };
    }

    [Theory]
    [MemberData(nameof(GetConstructorParameters))]
    public void Equals_ReturnsFalse(
        ImmutableArray<string> path,
        ImmutableArray<string> aliases,
        string? description,
        bool isHidden,
        string? parametersType)
    {
        var data1 = new CommandAttributeData(["get", "things"], ["stuff"], "description", false, "Parameters");
        var data2 = new CommandAttributeData(path, aliases, description, isHidden, parametersType);

        Assert.NotEqual(data1, data2);
    }

    [Fact]
    public void Equals_ReturnsTrue()
    {
        var data1 = new CommandAttributeData(["get", "things"], ["stuff"], "description", false, "Parameters");
        var data2 = new CommandAttributeData(["get", "things"], ["stuff"], "description", false, "Parameters");

        Assert.Equal(data1, data2);
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameInstance()
    {
        var data = new CommandAttributeData([], [], null, false, null);

        Assert.Equal(data, data);
    }

    [Fact]
    public void GetHashCode_IsDifferent_WhenPathDiffers()
    {
        var data1 = new CommandAttributeData(["get", "things"], ["stuff"], "description", false, "Parameters");
        var data2 = new CommandAttributeData(["set", "things"], ["stuff"], "description", false, "Parameters");

        Assert.NotEqual(data1.GetHashCode(), data2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_IsSame_WhenEqual()
    {
        var data1 = new CommandAttributeData(["get", "things"], ["stuff"], "description", false, "Parameters");
        var data2 = new CommandAttributeData(["get", "things"], ["stuff"], "description", false, "Parameters");

        Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
    }
}

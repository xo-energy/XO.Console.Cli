using Microsoft.CodeAnalysis;

namespace XO.Console.Cli.Model;

public sealed class CommandModelTest
{
    [Fact]
    public void Equals_ReturnsTrue_WhenSameInstance()
    {
        var model = new CommandModel(CommandModelKind.Command, "get", Location.None, []);

        Assert.Equal(model, model);
    }

    [Fact]
    public void GetHashCode_IsDifferent_WhenKindDiffers()
    {
        var model1 = new CommandModel(CommandModelKind.Command, "get", Location.None, []);
        var model2 = new CommandModel(CommandModelKind.Branch, "get", Location.None, []);

        Assert.NotEqual(model1.GetHashCode(), model2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_IsDifferent_WhenFullNameDiffers()
    {
        var model1 = new CommandModel(CommandModelKind.Command, "get", Location.None, []);
        var model2 = new CommandModel(CommandModelKind.Command, "set", Location.None, []);

        Assert.NotEqual(model1.GetHashCode(), model2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_IsSame_WhenOtherPropertiesDiffers()
    {
        var model1 = new CommandModel(CommandModelKind.Command, "get", Location.None, []);
        var model2 = new CommandModel(CommandModelKind.Command, "get", Location.None, [
            new(DiagnosticDescriptors.CommandMayNotHaveMultipleCommandAttributes, Location.None),
        ]);

        Assert.Equal(model1.GetHashCode(), model2.GetHashCode());
    }
}

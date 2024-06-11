using static XO.Console.Cli.CompilationHelper;

namespace XO.Console.Cli.Model;

public sealed class CommandArgumentModelTest
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var property = CompileParametersAndGetSymbol("Id", "System.Guid");
        var model = new CommandArgumentModel("arg1", property!, "description");

        Assert.Multiple(
            () => Assert.Equal("arg1", model.Name),
            () => Assert.Equal("Id", model.PropertyName),
            () => Assert.Equal("description", model.Description));
    }
}

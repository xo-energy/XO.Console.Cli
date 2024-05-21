using static XO.Console.Cli.CompilationHelper;

namespace XO.Console.Cli.Model;

public sealed class CommandOptionModelTest
{
    [Fact]
    public void Equals_ReturnsFalse_WhenAliasesDiffers()
    {
        var property = CompileParametersAndGetSymbol("Id", "System.Guid");
        var model1 = new CommandOptionModel("arg1", property!, "description", []);
        var model2 = new CommandOptionModel("arg1", property!, "description", ["alias"]);

        Assert.NotEqual(model1, model2);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenBasePropertyDiffers()
    {
        var property = CompileParametersAndGetSymbol("Id", "System.Guid");
        var model1 = new CommandOptionModel("arg1", property!, "description", []);
        var model2 = new CommandOptionModel("arg2", property!, "description", []);

        Assert.NotEqual(model1, model2);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenIsHiddenDiffers()
    {
        var property = CompileParametersAndGetSymbol("Id", "System.Guid");
        var model1 = new CommandOptionModel("arg1", property!, "description", []);
        var model2 = model1 with { IsHidden = true };

        Assert.NotEqual(model1, model2);
    }

    [Fact]
    public void Equals_ReturnsTrue()
    {
        var property = CompileParametersAndGetSymbol("Id", "System.Guid");
        var model1 = new CommandOptionModel("arg1", property!, "description", ["a"]);
        var model2 = new CommandOptionModel("arg1", property!, "description", ["a"]);

        Assert.Equal(model1, model2);
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenSameInstance()
    {
        var property = CompileParametersAndGetSymbol("Id", "System.Guid");
        var model = new CommandOptionModel("arg1", property!, "description", []);

        Assert.Equal(model, model);
    }

    [Fact]
    public void GetHashCode_IsDifferent_WhenBasePropertyDiffers()
    {
        var property = CompileParametersAndGetSymbol("Id", "System.Guid");
        var model1 = new CommandOptionModel("arg1", property!, "description", []);
        var model2 = new CommandOptionModel("arg2", property!, "description", []);

        Assert.NotEqual(model1.GetHashCode(), model2.GetHashCode());
    }
}

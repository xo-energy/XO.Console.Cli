using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace XO.Console.Cli.Model;

public sealed class CommandParameterModelTest
{
    [Fact]
    public void GetParameterParsingStrategy_ReturnsChar()
    {
        var symbol = CompileParametersAndGetSymbol("Letter", "char");
        Assert.NotNull(symbol);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(symbol.Type);
        Assert.Equal(ParameterParsingStrategy.Char, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsConstructor()
    {
        var symbol = CompileParametersAndGetSymbol("Name", "System.Tuple<string>");
        Assert.NotNull(symbol);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(symbol.Type);
        Assert.Equal(ParameterParsingStrategy.Constructor, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsEnum()
    {
        var symbol = CompileParametersAndGetSymbol("Style", "CommandOptionStyle");
        Assert.NotNull(symbol);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(symbol.Type);
        Assert.Equal(ParameterParsingStrategy.Enum, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsNone()
    {
        var symbol = CompileParametersAndGetSymbol("Action", "System.Action");
        Assert.NotNull(symbol);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(symbol.Type);
        Assert.Equal(ParameterParsingStrategy.None, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsParse_ForNamedTypeSymbol()
    {
        var symbol = CompileParametersAndGetSymbol("Id", "System.Guid");
        Assert.NotNull(symbol);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(symbol.Type);
        Assert.Equal(ParameterParsingStrategy.Parse, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsParse_ForSpecialType()
    {
        var symbol = CompileParametersAndGetSymbol("Value", "decimal");
        Assert.NotNull(symbol);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(symbol.Type);
        Assert.Equal(ParameterParsingStrategy.Parse, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsString()
    {
        var symbol = CompileParametersAndGetSymbol("Name", "string");
        Assert.NotNull(symbol);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(symbol.Type);
        Assert.Equal(ParameterParsingStrategy.String, strategy);
    }

    private static CSharpCompilation CompileParameters(string propertyName, string propertyType)
    {
        return CompilationHelper.CreateCompilation(
            $$"""
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandArgument(1, "{{propertyName}}")]
                public {{propertyType}} {{propertyName}} { get; set; }
            }
            """);
    }

    private static IPropertySymbol? CompileParametersAndGetSymbol(string propertyName, string propertyType)
    {
        var compilation = CompileParameters(propertyName, propertyType);
        var parameters = compilation.GetTypeByMetadataName("Test.Parameters");
        var property = parameters?.GetMembers(propertyName).OfType<IPropertySymbol>().SingleOrDefault();

        return property;
    }
}

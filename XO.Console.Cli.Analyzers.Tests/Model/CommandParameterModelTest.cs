using Microsoft.CodeAnalysis;
using static XO.Console.Cli.CompilationHelper;

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
    public void GetParameterParsingStrategy_ReturnsNone_ForNamedTypeSymbol()
    {
        var symbol = CompileParametersAndGetSymbol("Action", "System.Action");
        Assert.NotNull(symbol);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(symbol.Type);
        Assert.Equal(ParameterParsingStrategy.None, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsNone_ForNamedTypeSymbolWithFieldParse()
    {
        var compilation = CompilationHelper.CreateCompilation(
            $$"""
            using XO.Console.Cli;

            namespace Test;

            public sealed class SampleType
            {
                public static readonly string Parse = "";
            }

            public sealed class Parameters : CommandParameters
            {
                [CommandArgument(1, "Arg")]
                public SampleType Arg { get; set; }
            }
            """);
        var parameters = compilation.GetTypeByMetadataName("Test.Parameters");
        var property = parameters?.GetMembers("Arg").OfType<IPropertySymbol>().SingleOrDefault();
        Assert.NotNull(property);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(property.Type);
        Assert.Equal(ParameterParsingStrategy.None, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsNone_ForNamedTypeSymbolWithInstanceParse()
    {
        var compilation = CompilationHelper.CreateCompilation(
            $$"""
            using XO.Console.Cli;

            namespace Test;

            public sealed class SampleType
            {
                public SampleType Parse(string value) { }

                public SampleType Parse(string value, int start) { }
            }

            public sealed class Parameters : CommandParameters
            {
                [CommandArgument(1, "Arg")]
                public SampleType Arg { get; set; }
            }
            """);
        var parameters = compilation.GetTypeByMetadataName("Test.Parameters");
        var property = parameters?.GetMembers("Arg").OfType<IPropertySymbol>().SingleOrDefault();
        Assert.NotNull(property);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(property.Type);
        Assert.Equal(ParameterParsingStrategy.None, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsNone_ForNamedTypeSymbolWithWrongParse()
    {
        var compilation = CompilationHelper.CreateCompilation(
            $$"""
            using XO.Console.Cli;

            namespace Test;

            public sealed class SampleType
            {
                public static string Parse(string value) { }

                public static SampleType Parse(string value, int start) { }
            }

            public sealed class Parameters : CommandParameters
            {
                [CommandArgument(1, "Arg")]
                public SampleType Arg { get; set; }
            }
            """);
        var parameters = compilation.GetTypeByMetadataName("Test.Parameters");
        var property = parameters?.GetMembers("Arg").OfType<IPropertySymbol>().SingleOrDefault();
        Assert.NotNull(property);

        var strategy = CommandParameterModel.GetParameterParsingStrategy(property.Type);
        Assert.Equal(ParameterParsingStrategy.None, strategy);
    }

    [Fact]
    public void GetParameterParsingStrategy_ReturnsNone_ForSpecialType()
    {
        var symbol = CompileParametersAndGetSymbol("Array", "string[]");
        Assert.NotNull(symbol);

        // this test is correct because normally we would get the strategy for the element type
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

    [Theory]
    [InlineData("System.Boolean")]
    [InlineData("System.Byte")]
    [InlineData("System.DateTime")]
    [InlineData("System.Decimal")]
    [InlineData("System.Double")]
    [InlineData("System.Int16")]
    [InlineData("System.Int32")]
    [InlineData("System.Int64")]
    [InlineData("System.SByte")]
    [InlineData("System.Single")]
    [InlineData("System.UInt16")]
    [InlineData("System.UInt32")]
    [InlineData("System.UInt64")]
    public void GetParameterParsingStrategy_ReturnsParse_ForSpecialType(string type)
    {
        var symbol = CompileParametersAndGetSymbol("Value", type);
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

    [Fact]
    public void WithSyntax_CallsCopyConstructor()
    {
    }
}

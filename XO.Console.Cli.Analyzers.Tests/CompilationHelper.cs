using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace XO.Console.Cli;

internal static class CompilationHelper
{
    public static CSharpCompilation CompileParameters(string propertyName, string propertyType)
    {
        return CreateCompilation(
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

    public static IPropertySymbol? CompileParametersAndGetSymbol(string propertyName, string propertyType)
    {
        var compilation = CompileParameters(propertyName, propertyType);
        var parameters = compilation.GetTypeByMetadataName("Test.Parameters");
        var property = parameters?.GetMembers(propertyName).OfType<IPropertySymbol>().SingleOrDefault();

        return property;
    }

    public static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        return CreateCompilation(syntaxTree);
    }

    public static CSharpCompilation CreateCompilation(params SyntaxTree[] syntaxTrees)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
            .Select(_ => MetadataReference.CreateFromFile(_.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(CommandParameters).Assembly.Location),
            })
            .ToArray();
        var compilation = CSharpCompilation.Create(
            assemblyName: "Test",
            syntaxTrees: syntaxTrees,
            references: references);

        return compilation;
    }

    public static GeneratorDriver RunGenerator<TGenerator>(
        string source,
        out Compilation outputCompilation,
        out ImmutableArray<Diagnostic> diagnostics,
        CancellationToken cancellationToken = default)
        where TGenerator : IIncrementalGenerator, new()
    {
        const string main =
            """

            internal static class Program
            {
                public static int Main(string[] args) => 0;
            }
            """;

        var generator = new TGenerator();
        var compilation = CreateCompilation(source + main);

        // make sure the input source compiles without warnings before involving the generator
        Assert.Empty(compilation.GetDiagnostics(cancellationToken));

        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out diagnostics, cancellationToken);

        return driver;
    }

    public static GeneratorDriver RunGeneratorAndAssertEmptyDiagnostics<TGenerator>(string source)
        where TGenerator : IIncrementalGenerator, new()
    {
        var driver = RunGenerator<TGenerator>(
            source,
            out var outputCompilation,
            out var diagnostics);

        // make sure the generator didn't output any diagnostics
        Assert.Empty(diagnostics);

        // make sure the generator added a syntax tree to the compilation
        Assert.Equal(2, outputCompilation.SyntaxTrees.Count());

        // make sure the generator's output compiles
        Assert.Empty(outputCompilation.GetDiagnostics());

        // don't call Verify directly; it will mess up its detection of the source test
        return driver;
    }
}

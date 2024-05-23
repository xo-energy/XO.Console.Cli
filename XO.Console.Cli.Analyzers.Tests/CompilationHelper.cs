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

    public static GeneratorDriver RunGenerators(CSharpCompilation compilation, params IIncrementalGenerator[] generators)
    {
        return CSharpGeneratorDriver.Create(generators)
            .RunGenerators(compilation);
    }
}

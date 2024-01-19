using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace XO.Console.Cli.Generators;

[UsesVerify]
public sealed class CommandParametersFactoryGeneratorTest
{
    [Fact]
    public Task DetectsParameters()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                bool Enable { get; set; }
            }
            """);
    }

    [Fact]
    public Task DetectsArgument()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandArgument(1, "arg")]
                public bool Enable { get; set; }
            }
            """);
    }

    [Fact]
    public Task DetectsOption()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandOption("--option")]
                public bool Enable { get; set; }
            }
            """);
    }

    [Fact]
    public Task DetectsOptionWithAliases()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandOption("--option", "-o")]
                public bool Enable { get; set; }
            }
            """);
    }

    [Fact]
    public Task DetectsInherited()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public abstract class BaseParameters : CommandParameters
            {
                [CommandOption("--option", "-o")]
                public bool Enable { get; set; }
            }

            public sealed class Parameters : BaseParameters
            {
                [CommandArgument(1, "name")]
                public string Name { get; set; }
            }
            """);
    }

    [Fact]
    public Task DetectsNested()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public static class Wrapper
            {
                public sealed class Parameters : CommandParameters
                {
                    [CommandArgument(1, "name")]
                    public string Name { get; set; }
                }
            }
            """);
    }

    [Fact]
    public Task AssignsGreedyArgument_ToStringArray()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandArgument(1, "names", IsGreedy = true)]
                public string[] Names { get; set; }
            }
            """);
    }

    private static Task VerifySource(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
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
            syntaxTrees: new[] { syntaxTree },
            references: references);

        var generator = new CommandParametersFactoryGenerator();
        var generatorDriver = CSharpGeneratorDriver.Create(generator);

        var result = generatorDriver.RunGenerators(compilation);

        return Verify(result);
    }
}

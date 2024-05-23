using Microsoft.CodeAnalysis;

namespace XO.Console.Cli.Generators;

public sealed class CommandParametersFactoryGeneratorTest
{
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

    [Fact]
    public Task AssignsValuesForEachParsingStrategy()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class MyType(string value)
            {
                public string Value { get; } = value;
            }

            public sealed class Parameters : CommandParameters
            {
                [CommandOption("--value")]
                public MyType Constructor { get; set; }

                [CommandOption("--count")]
                public int Parse { get; set; }

                [CommandOption("--style")]
                public CommandOptionStyle Enum { get; set; }

                [CommandOption("--command")]
                public Command None { get; set; }

                [CommandOption("--name")]
                public string String { get; set; }

                [CommandOption("--code")]
                public char Char { get; set; }
            }
            """);
    }

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
    public Task DetectsArgumentWithNamedArguments()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandArgument(1, "arg", Description = "Enables things", IsGreedy = false, IsOptional = true)]
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
    public Task DetectsOptionWithNamedArguments()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandOption("--option", "-o", Description = "Enables things", IsHidden = false)]
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
    public void DoesNotGenerateEmptyFactory()
    {
        var driver = RunCommandParametersFactoryGenerator("");
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public Task IgnoresArgument_WhenNameIsNull()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandArgument(1, null)]
                public string Name { get; set; }
            }
            """);
    }

    [Fact]
    public Task IgnoresOption_WhenNameIsNull()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandOption(null)]
                public string Name { get; set; }
            }
            """);
    }

    [Fact]
    public void IgnoresTypeNotDerivedFromCommandParameters()
    {
        var driver = RunCommandParametersFactoryGenerator(
            """
            using System;

            namespace Test;

            public sealed class Thing : ICloneable
            {
                public object Clone() => new Thing();
            }
            """);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
    }

    private static GeneratorDriver RunCommandParametersFactoryGenerator(string source)
    {
        const string main =
            """

            internal static class Program
            {
                public static int Main(string[] args) => 0;
            }
            """;

        var generator = new CommandParametersFactoryGenerator();
        var compilation = CompilationHelper.CreateCompilation(source + main);
        var driver = CompilationHelper.RunGenerators(compilation, generator);

        // make sure there were no compilation errors (are we testing what we think we are?)
        Assert.Empty(compilation.GetDiagnostics());

        return driver;
    }

    private static Task VerifySource(string source)
    {
        var driver = RunCommandParametersFactoryGenerator(source);

        return Verify(driver);
    }
}

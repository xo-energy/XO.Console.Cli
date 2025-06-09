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
    public Task AssignsValuesForNullableValueTypes()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandOption("--count")]
                public int? Parse { get; set; }

                [CommandOption("--style")]
                public CommandOptionStyle? Enum { get; set; }

                [CommandOption("--code")]
                public char? Char { get; set; }
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
    public Task DetectsOptionWithIsRequired()
    {
        return VerifySource(
            """
            using XO.Console.Cli;

            namespace Test;

            public sealed class Parameters : CommandParameters
            {
                [CommandOption("--option", IsRequired = true)]
                public string Value { get; set; }
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
        _ = CompilationHelper.RunGenerator<CommandParametersFactoryGenerator>(
            "",
            out var outputCompilation,
            out _);

        Assert.Single(outputCompilation.SyntaxTrees);
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
        _ = CompilationHelper.RunGenerator<CommandParametersFactoryGenerator>(
            """
            using System;

            namespace Test;

            public sealed class Thing : ICloneable
            {
                public object Clone() => new Thing();
            }
            """,
            out var outputCompilation,
            out _);

        Assert.Single(outputCompilation.SyntaxTrees);
    }

    private static async Task VerifySource(string source)
    {
        var driver = CompilationHelper.RunGeneratorAndAssertEmptyDiagnostics<CommandParametersFactoryGenerator>(source);

        await Verify(driver)
            .ScrubGeneratedCodeAttribute()
            ;
    }
}

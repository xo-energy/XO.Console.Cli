using XO.Console.Cli.Commands;
using XO.Console.Cli.Fixtures;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;
using Xunit;

namespace XO.Console.Cli;

public class CommandAppTest : CommandAppTestBase
{
    public static IEnumerable<object[]> GetGreedyArgs()
    {
        yield return new object[] { new string[] { "verb", "arg1" } };
        yield return new object[] { new string[] { "verb", "arg1", "arg2" } };
        yield return new object[] { new string[] { "verb", "arg1", "arg2", "arg3" } };
    }

    [Theory]
    [InlineData(CommandOptionStyle.Dos, "/arg")]
    [InlineData(CommandOptionStyle.Posix, "-a")]
    [InlineData(CommandOptionStyle.Posix, "--arg")]
    public void BindAssignsArgument_WhenOptionLeaderMustStartOptionIsFalse(
        CommandOptionStyle optionStyle, string arg)
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Argument>(Builtins.Delegates.NoOp)
            .SetOptionStyle(optionStyle, optionLeaderMustStartOption: false)
            .Build();

        var context = app.Bind(new[] { arg });
        var parameters = Assert.IsType<TestParameters.Argument>(context.Parameters);

        Assert.Equal(arg, parameters.Arg);
        Assert.Empty(context.RemainingArguments);
    }

    [Theory]
    [InlineData(CommandOptionStyle.Dos, "/arg")]
    [InlineData(CommandOptionStyle.Posix, "-a")]
    [InlineData(CommandOptionStyle.Posix, "--arg")]
    public void BindAssignsExplicitArgument(CommandOptionStyle optionStyle, string arg)
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Argument>(Builtins.Delegates.NoOp)
            .SetOptionStyle(optionStyle, optionLeaderMustStartOption: true)
            .Build();

        var context = app.Bind(new[] { "--", arg });
        var parameters = Assert.IsType<TestParameters.Argument>(context.Parameters);

        Assert.Equal(arg, parameters.Arg);
        Assert.Empty(context.RemainingArguments);
    }

    [Fact]
    public void BindAssignsHyphenToArgument()
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Argument>(Builtins.Delegates.NoOp)
            .Build();

        var context = app.Bind(new[] { "-" });
        var parameters = Assert.IsType<TestParameters.Argument>(context.Parameters);

        Assert.Equal("-", parameters.Arg);
    }

    [Fact]
    public void BindAssignsHyphenToOption()
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Option>(Builtins.Delegates.NoOp)
            .Build();

        var context = app.Bind(new[] { "--option", "-" });
        var parameters = Assert.IsType<TestParameters.Option>(context.Parameters);

        Assert.Equal("-", parameters.Value);
    }

    [Fact]
    public void BindAssignsOptionGroup()
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.OptionGroup>(Builtins.Delegates.NoOp)
            .Build();

        var context = app.Bind(new[] { "-abc" });
        var parameters = Assert.IsType<TestParameters.OptionGroup>(context.Parameters);

        Assert.True(parameters.ValueA);
        Assert.True(parameters.ValueB);
        Assert.True(parameters.ValueC);
        Assert.False(parameters.ValueD);
    }

    [Fact]
    public void BindReturnsCommand()
    {
        var app = new CommandAppBuilder()
            .AddCommand<TestCommands.NoOp>("noop")
            .Build();

        var context = app.Bind(new[] { "noop" });

        Assert.IsType<TestCommands.NoOp>(context.Command);
    }

    [Fact]
    public void BindReturnsCommand_WithAlias()
    {
        var alias = "noooop";

        var app = new CommandAppBuilder()
            .AddCommand<TestCommands.NoOp>("noop", builder => builder.AddAlias(alias))
            .Build();

        var context = app.Bind(new[] { alias });

        Assert.IsType<TestCommands.NoOp>(context.Command);
    }

    [Fact]
    public void BindReturnsCommandBranch()
    {
        var app = new CommandAppBuilder()
            .AddBranch("branch",
                builder =>
                {
                    builder.AddCommand<TestCommands.NoOp>("noop");
                })
            .Build();

        var context = app.Bind(new[] { "branch", "noop" });

        Assert.IsType<TestCommands.NoOp>(context.Command);
    }

    [Fact]
    public void BindReturnsCommandBranch_WhenParentHasArgument()
    {
        var app = new CommandAppBuilder()
            .AddCommand<TestCommandWithParameters>("branch",
                builder =>
                {
                    builder.AddCommand<TestCommandWithParameters>("noop");
                })
            .Build();

        var context = app.Bind(new[] { "branch", "foo", "noop" });

        Assert.Equal(["branch", "noop"], context.ParseResult.GetVerbs().Select(x => x.Value));
    }

    [Fact]
    public void BindReturnsCommandParseResult()
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Argument>(Builtins.Delegates.NoOp)
            .DisableStrictParsing()
            .Build();

        using var context = app.Bind(new[] { "--arg?" });

        Assert.Collection(
            context.ParseResult.Tokens,
            token => Assert.Equal(CommandTokenType.Unknown, token.TokenType))
            ;
        Assert.Collection(
            context.ParseResult.Errors,
            error => Assert.Equal($"Missing required argument 'arg'", error))
            ;
    }

    [Theory]
    [InlineData(CommandOptionStyle.Dos, "/arg")]
    [InlineData(CommandOptionStyle.Posix, "-a")]
    [InlineData(CommandOptionStyle.Posix, "--arg")]
    public void BindThrowsCommandParsingException_WhenArgumentValueMustBeParsedAsOption(
        CommandOptionStyle optionStyle, string arg)
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Argument>(Builtins.Delegates.NoOp)
            .SetOptionStyle(optionStyle, optionLeaderMustStartOption: true)
            .Build();

        var ex = Assert.Throws<CommandParsingException>(() => app.Bind(new[] { arg }));

        Assert.Collection(
            ex.ParseResult.Tokens,
            token => Assert.Equal(CommandTokenType.Unknown, token.TokenType))
            ;
        Assert.Collection(
            ex.ParseResult.Errors,
            error => Assert.Equal($"Missing required argument 'arg'", error))
            ;
    }

    [Fact]
    public void BindThrowsCommandParsingException_WhenRequiredOptionIsMissing()
    {
        // Arrange
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.RequiredOption>(Builtins.Delegates.NoOp)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<CommandParsingException>(() => app.Bind([]));

        // Verify the exception message contains information about the missing required option
        Assert.Contains("--required", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BindThrowsCommandTypeException_WhenTypeResolverReturnsNullCommand()
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestCommands.NoOp>()
            .UseTypeResolver(new ExplicitTypeResolver())
            .Build();

        Assert.Throws<CommandTypeException>(
            () => app.Bind(Array.Empty<string>()));
    }

    [Fact]
    public void BindThrowsCommandTypeException_WhenTypeResolverReturnsNullParameters()
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestCommands.NoOp>()
            .UseTypeResolver(new ExplicitTypeResolver(typeof(TestCommands.NoOp)))
            .Build();

        Assert.Throws<CommandTypeException>(
            () => app.Bind(Array.Empty<string>()));
    }

    [Fact]
    public void BindThrowsCommandParameterValidationException_WhenValidationFails()
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.ValidationFailure>(Builtins.Delegates.NoOp)
            .Build();

        Assert.Throws<CommandParameterValidationException>(
            () => app.Bind(Array.Empty<string>()));
    }

    [Fact]
    public async Task ExecuteAsyncReturns0_FromAlias()
    {
        var alias = "noooop";

        var app = CreateBuilder()
            .AddCommand<TestCommands.NoOp>("noop", builder => builder.AddAlias(alias))
            .Build();

        var result = await app.ExecuteAsync(new[] { alias });

        Assert.Equal(0, result);
    }

    [Fact]
    public void ParseReturnsArgument()
    {
        var expected = "word";

        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Argument>(Builtins.Delegates.NoOp)
            .Build();

        var parse = app.Parse(new[] { expected });

        Assert.Collection(
            parse.Tokens,
            token =>
            {
                Assert.Equal(CommandTokenType.Argument, token.TokenType);
                Assert.Equal(expected, token.Value);
            });

        Assert.Empty(parse.Errors);
    }

    [Fact]
    public void ParseReturnsError_WithGreedyArgumentMissing()
    {
        var app = new CommandAppBuilder()
            .AddDelegate<TestParameters.Greedy>("verb", Builtins.Delegates.NoOp)
            .Build();
        var parse = app.Parse(new[] { "verb" });

        Assert.Collection(parse.Errors, error => error.StartsWith("Missing required argument"));
    }

    [Theory]
    [MemberData(nameof(GetGreedyArgs))]
    public void ParseReturnsSuccess_WithGreedyArgument(string[] args)
    {
        var app = new CommandAppBuilder()
            .AddDelegate<TestParameters.Greedy>("verb", Builtins.Delegates.NoOp)
            .Build();
        var parse = app.Parse(args);

        Assert.Empty(parse.Errors);
    }

    [Fact]
    public void ParseReturnsSuccess_WithGreedyOptionalArgument()
    {
        var app = new CommandAppBuilder()
            .AddDelegate<TestParameters.GreedyOptional>("verb", Builtins.Delegates.NoOp)
            .Build();
        var parse = app.Parse(new[] { "verb" });

        Assert.Empty(parse.Errors);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ParseReturnsOptionFlag_WithEquals(bool expected)
    {
        var args = new[] { $"--option={expected}" };

        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Option>(Builtins.Delegates.NoOp)
            .Build();

        var parse = app.Parse(args);

        Assert.Collection(
            parse.Tokens,
            token =>
            {
                Assert.Equal(CommandTokenType.OptionValue, token.TokenType);
                Assert.Equal(expected.ToString(), token.Value);
            });

        Assert.Empty(parse.Errors);
    }

    [Fact]
    public void ParseReturnsOptionValue()
    {
        var args = new[] { "--option", "value" };

        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Option>(Builtins.Delegates.NoOp)
            .Build();

        var parse = app.Parse(args);

        Assert.Collection(
            parse.Tokens,
            token =>
            {
                Assert.Equal(CommandTokenType.Option, token.TokenType);
                Assert.Equal(args[0], token.Value);
            },
            token =>
            {
                Assert.Equal(CommandTokenType.OptionValue, token.TokenType);
                Assert.Equal(args[1], token.Value);
            });
    }

    [Fact]
    public void ParseReturnsOptionValue_WithEquals()
    {
        var expected = "value";
        var args = new[] { $"--option={expected}" };

        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Option>(Builtins.Delegates.NoOp)
            .Build();

        var parse = app.Parse(args);

        Assert.Collection(
            parse.Tokens,
            token =>
            {
                Assert.Equal(CommandTokenType.OptionValue, token.TokenType);
                Assert.Equal(expected, token.Value);
            });

        Assert.Empty(parse.Errors);
    }

    [Fact]
    public void ParseReturnsOptionValueMissing()
    {
        var args = new[] { "--option" };

        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Option>(Builtins.Delegates.NoOp)
            .Build();

        var parse = app.Parse(args);

        Assert.Collection(
            parse.Tokens,
            token =>
            {
                Assert.Equal(CommandTokenType.Option, token.TokenType);
                Assert.Equal(args[0], token.Value);
            });

        Assert.Collection(
            parse.Errors,
            error => Assert.Equal($"Expected a value for {args[0]}", error));
    }

    [Fact]
    public void ParseReturnsOptionValueMissing_BeforeExplicitSeparator()
    {
        var args = new[] { "--option", "--" };

        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.Option>(Builtins.Delegates.NoOp)
            .Build();

        var parse = app.Parse(args);

        Assert.Collection(
            parse.Tokens,
            token =>
            {
                Assert.Equal(CommandTokenType.Option, token.TokenType);
                Assert.Equal(args[0], token.Value);
            },
            token =>
            {
                Assert.Equal(CommandTokenType.System, token.TokenType);
                Assert.Equal(args[1], token.Value);
                Assert.Equal("Explicit arguments enabled", token.Context);
            });

        Assert.Collection(
            parse.Errors,
            error => Assert.Equal($"Expected a value for {args[0]}", error));
    }

    [Fact]
    public void ParseReturnsUnexpectedOption()
    {
        var arg = "--option";
        var app = new CommandAppBuilder()
            .Build();

        var parse = app.Parse(new[] { arg });

        Assert.Collection(
            parse.Tokens,
            token =>
            {
                Assert.Equal(CommandTokenType.Unknown, token.TokenType);
                Assert.Equal(arg, token.Value);
            });
    }

    [Theory]
    [InlineData("-abee")]
    [InlineData("--a")]
    [InlineData("--abc")]
    public void ParseReturnsUnexpectedOption_WithInvalidOptionGroup(string arg)
    {
        var app = CommandAppBuilder
            .WithDefaultCommand<TestParameters.OptionGroup>(Builtins.Delegates.NoOp)
            .Build();

        var parse = app.Parse(new[] { arg });

        Assert.Collection(
            parse.Tokens,
            token =>
            {
                Assert.Equal(CommandTokenType.Unknown, token.TokenType);
                Assert.Equal(arg, token.Value);
                Assert.Equal($"Unexpected option", token.Context);
            })
            ;
    }

    [Fact]
    public void WithEmptyAppDisableStrictParsing_ArgumentsBindToRemaining()
    {
        var expected = new[] { "extra1", "extra2" };

        var app = new CommandAppBuilder()
            .DisableStrictParsing()
            .Build();

        var context = app.Bind(expected);

        Assert.Collection(
            context.RemainingArguments,
            arg => Assert.Equal(expected[0], arg),
            arg => Assert.Equal(expected[1], arg));
    }

    [Fact]
    public void WithEmptyApp_ExplicitArgumentsEnabled()
    {
        var args = new[] { "--" };

        var app = new CommandAppBuilder()
            .Build();

        var parse = app.Parse(args);

        Assert.Collection(
            parse.Tokens,
            token => Assert.Equal(CommandTokenType.System, token.TokenType));
    }

    [Fact]
    public void WithEmptyApp_ArgumentsThrowCommandParsingException()
    {
        var expected = new[] { "extra1", "extra2" };

        var app = new CommandAppBuilder()
            .Build();

        Assert.Throws<CommandParsingException>(() => app.Bind(expected));
    }

    [Fact]
    public async Task WithEmptyApp_ExecuteAsyncReturns1()
    {
        var app = CreateBuilder()
            .Build();

        var result = await app.ExecuteAsync(Array.Empty<string>());

        Assert.Equal(1, result);
    }

    [Fact]
    public void WithEmptyApp_EmptyArgsBindsToMissingCommand()
    {
        var app = new CommandAppBuilder()
            .Build();

        var parse = app.Parse(Array.Empty<string>());
        var context = app.Bind(parse);

        Assert.IsType<MissingCommand>(context.Command);
    }

    [Fact]
    public void WithEmptyApp_CliExplainOptionBindsToCliExplainCommand()
    {
        var app = new CommandAppBuilder()
            .Build();

        var context = app.Bind(new[] { "--cli-explain" });

        Assert.IsType<CliExplainCommand>(context.Command);
    }

    [Fact]
    public void WithEmptyApp_HelpOptionBindsToHelpCommand()
    {
        var app = new CommandAppBuilder()
            .Build();

        var parse = app.Parse(new[] { "--help" });
        var context = app.Bind(parse);

        Assert.IsType<HelpCommand>(context.Command);
    }

    [Fact]
    public void WithEmptyApp_VersionOptionBindsToVersionCommand()
    {
        var app = new CommandAppBuilder()
            .Build();

        var parse = app.Parse(new[] { "--version" });
        var context = app.Bind(parse);

        Assert.IsType<VersionCommand>(context.Command);
    }
}

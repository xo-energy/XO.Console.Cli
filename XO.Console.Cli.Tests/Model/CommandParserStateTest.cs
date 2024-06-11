using System.Collections.Immutable;
using XO.Console.Cli.Infrastructure;
using Xunit;

namespace XO.Console.Cli.Model;

public class CommandParserStateTest
{
    private static CommandOption CreateOption(string name, ImmutableArray<string> aliases)
    {
        return new CommandOption(
            $"{nameof(CommandParserStateTest)}.Option",
            name,
            Builtins.Options.DiscardValue,
            typeof(string),
            description: null)
        {
            Aliases = aliases,
        };
    }

    private static CommandParserState CreateState(CommandOptionStyle optionStyle, StringComparer? optionNameComparer = null)
    {
        var settings = new CommandAppSettings(
            "",
            "",
            CommandAppDefaults.Converters,
            ImmutableList<CommandOption>.Empty)
        {
            OptionNameComparer = optionNameComparer ?? optionStyle.GetDefaultNameComparer(),
            OptionStyle = optionStyle,
        };

        return new CommandParserState(0, ImmutableList<ConfiguredCommand>.Empty, settings);
    }

    [Fact]
    public void AddParameters_SkipsParametersSeenInBaseType()
    {
        var state = CreateState(CommandAppDefaults.OptionStyle);
        var command = new ConfiguredCommand("go", static (_) => new MissingCommand(), typeof(Parameters.Valid))
        {
            Commands = ImmutableList.Create(
                new ConfiguredCommand("child", static (_) => new MissingCommand(), typeof(Parameters.ValidDerived))
            ),
        };

        state.AddParameters(command);
        _ = state.Arguments.Dequeue();
        state.AddParameters(command.Commands[0]);

        Assert.Single(state.Arguments);
    }

    [Fact]
    public void ValidateArgumentThrowsCommandTypeException_WithGreedyBeforeChild()
    {
        var state = CreateState(CommandAppDefaults.OptionStyle);
        var command = new ConfiguredCommand("go", static (_) => new MissingCommand(), typeof(Parameters.ValidDerived))
        {
            Commands = ImmutableList.Create(
                new ConfiguredCommand("child", static (_) => new MissingCommand(), typeof(Parameters.Valid))
            ),
        };

        var ex = Assert.Throws<CommandTypeException>(() => state.AddParameters(command));
        Assert.Matches(@"has subcommands, but its argument '\w+' is greedy", ex.Message);
    }

    [Fact]
    public void ValidateArgumentThrowsCommandTypeException_WithGreedyBeforeEnd()
    {
        var state = CreateState(CommandAppDefaults.OptionStyle);
        var command = new ConfiguredCommand("go", static (_) => new MissingCommand(), typeof(Parameters.WithGreedyBeforeEnd));

        var ex = Assert.Throws<CommandTypeException>(() => state.AddParameters(command));
        Assert.Contains("greedy, but there are other arguments after it", ex.Message);
    }

    [Fact]
    public void ValidateArgumentThrowsCommandTypeException_WithOptionalBeforeChild()
    {
        var state = CreateState(CommandAppDefaults.OptionStyle);
        var command = new ConfiguredCommand("go", static (_) => new MissingCommand(), typeof(Parameters.ValidDerivedOptional))
        {
            Commands = ImmutableList.Create(
                new ConfiguredCommand("child", static (_) => new MissingCommand(), typeof(Parameters.Valid))
            ),
        };

        var ex = Assert.Throws<CommandTypeException>(() => state.AddParameters(command));
        Assert.Matches(@"has subcommands, but its argument '\w+' is optional", ex.Message);
    }

    [Fact]
    public void ValidateArgumentThrowsCommandTypeException_WithOptionalBeforeRequired()
    {
        var state = CreateState(CommandAppDefaults.OptionStyle);
        var command = new ConfiguredCommand("go", static (_) => new MissingCommand(), typeof(Parameters.WithOptionalBeforeRequired));

        var ex = Assert.Throws<CommandTypeException>(() => state.AddParameters(command));
        Assert.Contains("required, but the previous argument was optional", ex.Message);
    }

    [Theory]
    [MemberData(nameof(GetOptionValidNames))]
    public void ValidateOptionNameDoesNotThrow_WithValidOptionAlias(CommandOptionStyle optionStyle, string alias)
    {
        var state = CreateState(optionStyle);
        var option = CreateOption(optionStyle.GetNameWithLeader("apple"), ImmutableArray.Create(alias));

        state.AddOption(typeof(CommandParameters), option);
    }

    [Theory]
    [MemberData(nameof(GetOptionValidNames))]
    public void ValidateOptionNameDoesNotThrow_WithValidOptionName(CommandOptionStyle optionStyle, string name)
    {
        var state = CreateState(optionStyle);
        var option = CreateOption(name, ImmutableArray<string>.Empty);

        state.AddOption(typeof(CommandParameters), option);
    }

    [Theory]
    [MemberData(nameof(GetParametersWithDuplicateOptionNames))]
    public void ValidateOptionNameThrowsCommandTypeException_WithDuplicateOptionName(
        CommandOptionStyle optionStyle,
        StringComparer? optionNameComparer,
        Type parametersType)
    {
        var state = CreateState(optionStyle, optionNameComparer);
        var command = new ConfiguredCommand("go", static (_) => new MissingCommand(), parametersType);

        Assert.Throws<CommandTypeException>(
            () => state.AddParameters(command));
    }

    [Theory]
    [MemberData(nameof(GetOptionInvalidNames))]
    public void ValidateOptionNameThrowsCommandTypeException_WithInvalidOptionAlias(CommandOptionStyle optionStyle, string alias)
    {
        var state = CreateState(optionStyle);
        var option = CreateOption(optionStyle.GetNameWithLeader("apple"), ImmutableArray.Create(alias));

        Assert.Throws<CommandTypeException>(() => state.AddOption(typeof(CommandParameters), option));
    }

    [Theory]
    [MemberData(nameof(GetOptionInvalidNames))]
    public void ValidateOptionNameThrowsCommandTypeException_WithInvalidOptionName(CommandOptionStyle optionStyle, string name)
    {
        var state = CreateState(optionStyle);
        var option = CreateOption(name, ImmutableArray<string>.Empty);

        Assert.Throws<CommandTypeException>(() => state.AddOption(typeof(CommandParameters), option));
    }

    public static TheoryData<CommandOptionStyle, string> GetOptionInvalidNames()
    {
        return new() {
            { CommandOptionStyle.Posix, "" },
            { CommandOptionStyle.Posix, "- " },
            { CommandOptionStyle.Posix, " -a" },
            { CommandOptionStyle.Posix, "-!" },
            { CommandOptionStyle.Posix, "arg" },
            { CommandOptionStyle.Posix, " arg" },
            { CommandOptionStyle.Posix, "-arg" },
            { CommandOptionStyle.Posix, "--" },
            { CommandOptionStyle.Posix, "---" },
            { CommandOptionStyle.Posix, "--arg " },
            { CommandOptionStyle.Posix, "--arg!" },
            { CommandOptionStyle.Posix, " --arg" },
            { CommandOptionStyle.Dos, "" },
            { CommandOptionStyle.Dos, "/ " },
            { CommandOptionStyle.Dos, " /a" },
            { CommandOptionStyle.Dos, "/!" },
            { CommandOptionStyle.Dos, "arg" },
            { CommandOptionStyle.Dos, " arg" },
            { CommandOptionStyle.Dos, "//" },
            { CommandOptionStyle.Dos, "//arg" },
            { CommandOptionStyle.Dos, "/arg " },
        };
    }

    public static TheoryData<CommandOptionStyle, string> GetOptionValidNames()
    {
        return new() {
            { CommandOptionStyle.Posix, "-a" },
            { CommandOptionStyle.Posix, "-B" },
            { CommandOptionStyle.Posix, "-9" },
            { CommandOptionStyle.Posix, "--a" },
            { CommandOptionStyle.Posix, "--option" },
            { CommandOptionStyle.Posix, "--option-name" },
            { CommandOptionStyle.Posix, "--option1" },
            { CommandOptionStyle.Posix, "--optionName" },
            { CommandOptionStyle.Dos, "/a" },
            { CommandOptionStyle.Dos, "/B" },
            { CommandOptionStyle.Dos, "/9" },
            { CommandOptionStyle.Dos, "/?" },
            { CommandOptionStyle.Dos, "/option" },
            { CommandOptionStyle.Dos, "/Option" },
            { CommandOptionStyle.Dos, "/option-name" },
            { CommandOptionStyle.Dos, "/option1" },
            { CommandOptionStyle.Dos, "/optionName" },
        };
    }

    public static TheoryData<CommandOptionStyle, StringComparer?, Type> GetParametersWithDuplicateOptionNames()
    {
        return new() {
            { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateOptionAlias) },
            { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateOptionName) },
            { CommandAppDefaults.OptionStyle, StringComparer.OrdinalIgnoreCase, typeof(Parameters.WithDuplicateOptionNameCaseInsensitive) },
            { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateDerivedOptionAlias) },
            { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateDerivedOptionName) },
        };
    }

    public static class Parameters
    {
        public class Valid : CommandParameters
        {
            [CommandArgument(1, "arg1")]
            public string? Arg1 { get; set; }

            [CommandOption("--option1", "-1")]
            public string? Option1 { get; set; }
        }

        public class ValidDerived : Valid
        {
            [CommandArgument(2, "arg2", IsGreedy = true, IsOptional = true)]
            public string[]? Arg2 { get; set; }

            [CommandOption("--option2", "-2")]
            public bool Option2 { get; set; }
        }

        public class ValidDerivedOptional : Valid
        {
            [CommandArgument(2, "arg2", IsOptional = true)]
            public string? Arg2 { get; set; }

            [CommandOption("--option2", "-2")]
            public bool Option2 { get; set; }
        }

        public class WithDuplicateDerivedOptionAlias : Valid
        {
            [CommandOption("--option2", "-1")]
            public string? Option2 { get; set; }
        }

        public class WithDuplicateDerivedOptionName : Valid
        {
            [CommandOption("--option1")]
            public string? Option2 { get; set; }
        }

        public class WithDuplicateOptionAlias : CommandParameters
        {
            [CommandOption("--option1", "-o")]
            public string? Option1 { get; set; }

            [CommandOption("--option2", "-o")]
            public string? Option2 { get; set; }
        }

        public class WithDuplicateOptionName : CommandParameters
        {
            [CommandOption("--option")]
            public string? Option1 { get; set; }

            [CommandOption("--option")]
            public string? Option2 { get; set; }
        }

        public class WithDuplicateOptionNameCaseInsensitive : CommandParameters
        {
            [CommandOption("--option")]
            public string? Option1 { get; set; }

            [CommandOption("--OPTION")]
            public string? Option2 { get; set; }
        }

        public class WithGreedyBeforeEnd : ValidDerived
        {
            [CommandArgument(3, "arg3")]
            public string? Arg3 { get; set; }
        }

        public class WithOptionalBeforeRequired : Valid
        {
            [CommandArgument(3, "arg3")]
            public string? Arg4 { get; set; }

            [CommandArgument(2, "arg2", IsOptional = true)]
            public string? Arg3 { get; set; }
        }
    }
}

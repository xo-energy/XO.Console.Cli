using System;
using System.Collections.Generic;
using XO.Console.Cli.Implementation;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;
using Xunit;

namespace XO.Console.Cli.Tests;

public class CommandParametersInspectorTest
{
    private static void TestArgument(
        CommandArgument argument,
        CommandArgumentAttribute attribute,
        Type declaringType,
        Type valueType,
        string? description)
    {
        Assert.NotNull(argument);
        Assert.Equal(attribute, argument.Attribute);
        Assert.Equal(declaringType, argument.DeclaringType);
        Assert.Equal(valueType, argument.ValueType);
        Assert.Equal(description, argument.Description);
    }

    private static void TestOption(
        CommandOption option,
        CommandOptionAttribute attribute,
        Type declaringType,
        Type valueType,
        string? description,
        bool isFlag)
    {
        Assert.NotNull(option);
        Assert.Equal(attribute, option.Attribute);
        Assert.Equal(declaringType, option.DeclaringType);
        Assert.Equal(valueType, option.ValueType);
        Assert.Equal(description, option.Description);
        Assert.Equal(isFlag, option.IsFlag);
    }

    [Fact]
    public void ReturnsArgument()
    {
        var inspector = new CommandParametersInspector();
        var command = new CommandBuilder("verb", typeof(Parameters.Valid))
            .Build();
        var info = inspector.InspectParameters(command);

        Assert.Collection(
            info.Arguments,
            argument => TestArgument(
                argument,
                new CommandArgumentAttribute(1, "arg1"),
                typeof(Parameters.Valid),
                typeof(string),
                null)
        );
    }

    [Fact]
    public void ReturnsArgument_WithDerivedParameters()
    {
        var inspector = new CommandParametersInspector();
        var command = new CommandBuilder("verb", typeof(Parameters.ValidDerived))
            .Build();
        var info = inspector.InspectParameters(command);

        // note arguments get sorted so they will be in order
        Assert.Collection(
            info.Arguments,
            argument => TestArgument(
                argument,
                new CommandArgumentAttribute(1, "arg1"),
                typeof(Parameters.Valid),
                typeof(string),
                null),
            argument => TestArgument(
                argument,
                new CommandArgumentAttribute(2, "arg2") { IsGreedy = true, IsOptional = true },
                typeof(Parameters.ValidDerived),
                typeof(string),
                null)
        );
    }

    [Fact]
    public void ReturnsOption()
    {
        var inspector = new CommandParametersInspector();
        var command = new CommandBuilder("verb", typeof(Parameters.Valid))
            .Build();
        var info = inspector.InspectParameters(command);

        Assert.Collection(
            info.Options,
            option => TestOption(
                option,
                new CommandOptionAttribute("--option1", "-1"),
                typeof(Parameters.Valid),
                typeof(string),
                null,
                false)
        );
    }

    [Fact]
    public void ReturnsOption_WithDerivedParameters()
    {
        var inspector = new CommandParametersInspector();
        var command = new CommandBuilder("verb", typeof(Parameters.ValidDerived))
            .Build();
        var info = inspector.InspectParameters(command);

        // note declared properties are returned before inherited properties
        Assert.Collection(
            info.Options,
            option => TestOption(
                option,
                new CommandOptionAttribute("--option2", "-2"),
                typeof(Parameters.ValidDerived),
                typeof(bool),
                null,
                true),
            option => TestOption(
                option,
                new CommandOptionAttribute("--option1", "-1"),
                typeof(Parameters.Valid),
                typeof(string),
                null,
                false)
        );
    }

    [Theory]
    [MemberData(nameof(GetParametersWithDuplicateNames))]
    public void ThrowsCommandTypeException_WithDuplicateName(
        CommandOptionStyle optionStyle,
        StringComparer optionNameComparer,
        Type parametersType)
    {
        var inspector = new CommandParametersInspector(optionStyle, optionNameComparer);
        var command = new CommandBuilder("verb", parametersType)
            .Build();

        Assert.Throws<CommandTypeException>(
            () => inspector.InspectParameters(command));
    }

    [Theory]
    [MemberData(nameof(GetOptionValidNames))]
    public void ValidateOptionNameDoesNotThrow_WithValidOptionAlias(CommandOptionStyle optionStyle, string alias)
    {
        var option = new CommandOption(
            new CommandOptionAttribute(optionStyle.GetNameWithLeader("name"), alias),
            typeof(CommandOptionTest),
            typeof(string),
            Builtins.Options.DiscardValue);

        var inspector = new CommandParametersInspector(optionStyle: optionStyle);

        inspector.ValidateNames(
            typeof(CommandParameters),
            Array.Empty<CommandArgument>(),
            new[] { option });
    }

    [Theory]
    [MemberData(nameof(GetOptionValidNames))]
    public void ValidateOptionNameDoesNotThrow_WithValidOptionName(CommandOptionStyle optionStyle, string name)
    {
        var option = new CommandOption(
            new CommandOptionAttribute(name),
            typeof(CommandOptionTest),
            typeof(string),
            Builtins.Options.DiscardValue);

        var inspector = new CommandParametersInspector(optionStyle: optionStyle);

        inspector.ValidateNames(
            typeof(CommandParameters),
            Array.Empty<CommandArgument>(),
            new[] { option });
    }

    [Theory]
    [MemberData(nameof(GetOptionInvalidNames))]
    public void ValidateOptionNameThrowsArgumentException_WithInvalidOptionAlias(CommandOptionStyle optionStyle, string alias)
    {
        var option = new CommandOption(
            new CommandOptionAttribute($"{optionStyle.GetLeader()}name", alias),
            typeof(CommandOptionTest),
            typeof(string),
            Builtins.Options.DiscardValue);

        var inspector = new CommandParametersInspector(optionStyle: optionStyle);

        Assert.Throws<ArgumentException>(
            () => inspector.ValidateNames(
                typeof(CommandParameters),
                Array.Empty<CommandArgument>(),
                new[] { option }));
    }

    [Theory]
    [MemberData(nameof(GetOptionInvalidNames))]
    public void ValidateOptionNameThrowsArgumentException_WithInvalidOptionName(CommandOptionStyle optionStyle, string name)
    {
        var option = new CommandOption(
            new CommandOptionAttribute(name),
            typeof(CommandOptionTest),
            typeof(string),
            Builtins.Options.DiscardValue);

        var inspector = new CommandParametersInspector(optionStyle: optionStyle);

        Assert.Throws<ArgumentException>(
            () => inspector.ValidateNames(
                typeof(CommandParameters),
                Array.Empty<CommandArgument>(),
                new[] { option }));
    }

    public static IEnumerable<object[]> GetOptionInvalidNames()
    {
        yield return new object[] { CommandOptionStyle.Posix, "" };
        yield return new object[] { CommandOptionStyle.Posix, "- " };
        yield return new object[] { CommandOptionStyle.Posix, " -a" };
        yield return new object[] { CommandOptionStyle.Posix, "-!" };
        yield return new object[] { CommandOptionStyle.Posix, "arg" };
        yield return new object[] { CommandOptionStyle.Posix, " arg" };
        yield return new object[] { CommandOptionStyle.Posix, "-arg" };
        yield return new object[] { CommandOptionStyle.Posix, "--" };
        yield return new object[] { CommandOptionStyle.Posix, "---" };
        yield return new object[] { CommandOptionStyle.Posix, "--arg " };
        yield return new object[] { CommandOptionStyle.Posix, "--arg!" };
        yield return new object[] { CommandOptionStyle.Posix, " --arg" };
        yield return new object[] { CommandOptionStyle.Dos, "" };
        yield return new object[] { CommandOptionStyle.Dos, "/ " };
        yield return new object[] { CommandOptionStyle.Dos, " /a" };
        yield return new object[] { CommandOptionStyle.Dos, "/!" };
        yield return new object[] { CommandOptionStyle.Dos, "arg" };
        yield return new object[] { CommandOptionStyle.Dos, " arg" };
        yield return new object[] { CommandOptionStyle.Dos, "//" };
        yield return new object[] { CommandOptionStyle.Dos, "//arg" };
        yield return new object[] { CommandOptionStyle.Dos, "/arg " };
    }

    public static IEnumerable<object[]> GetOptionValidNames()
    {
        yield return new object[] { CommandOptionStyle.Posix, "-a" };
        yield return new object[] { CommandOptionStyle.Posix, "-B" };
        yield return new object[] { CommandOptionStyle.Posix, "-9" };
        yield return new object[] { CommandOptionStyle.Posix, "--a" };
        yield return new object[] { CommandOptionStyle.Posix, "--option" };
        yield return new object[] { CommandOptionStyle.Posix, "--option-name" };
        yield return new object[] { CommandOptionStyle.Posix, "--option1" };
        yield return new object[] { CommandOptionStyle.Posix, "--optionName" };
        yield return new object[] { CommandOptionStyle.Dos, "/a" };
        yield return new object[] { CommandOptionStyle.Dos, "/B" };
        yield return new object[] { CommandOptionStyle.Dos, "/9" };
        yield return new object[] { CommandOptionStyle.Dos, "/?" };
        yield return new object[] { CommandOptionStyle.Dos, "/option" };
        yield return new object[] { CommandOptionStyle.Dos, "/Option" };
        yield return new object[] { CommandOptionStyle.Dos, "/option-name" };
        yield return new object[] { CommandOptionStyle.Dos, "/option1" };
        yield return new object[] { CommandOptionStyle.Dos, "/optionName" };
    }

    public static IEnumerable<object?[]> GetParametersWithDuplicateNames()
    {
        yield return new object?[] { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateArgumentName) };
        yield return new object?[] { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateOptionAlias) };
        yield return new object?[] { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateOptionName) };
        yield return new object?[] { CommandAppDefaults.OptionStyle, StringComparer.OrdinalIgnoreCase, typeof(Parameters.WithDuplicateOptionNameCaseInsensitive) };
        yield return new object?[] { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateDerivedArgumentName) };
        yield return new object?[] { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateDerivedOptionAlias) };
        yield return new object?[] { CommandAppDefaults.OptionStyle, null, typeof(Parameters.WithDuplicateDerivedOptionName) };
    }

    private static class Parameters
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
            public string? Arg2 { get; set; }

            [CommandOption("--option2", "-2")]
            public bool Option2 { get; set; }
        }

        public class WithDuplicateDerivedArgumentName : Valid
        {
            [CommandArgument(2, "arg1")]
            public string? Arg2 { get; set; }
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

        public class WithDuplicateArgumentName : CommandParameters
        {
            [CommandArgument(1, "arg")]
            public string? Arg1 { get; set; }

            [CommandArgument(2, "arg")]
            public string? Arg2 { get; set; }
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
    }
}

using System;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;
using Xunit;

namespace XO.Console.Cli.Tests;

public class CommandOptionTest
{
    [Theory]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(bool?), true)]
    [InlineData(typeof(bool[]), true)]
    [InlineData(typeof(bool?[]), true)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(int?), false)]
    [InlineData(typeof(int[]), false)]
    [InlineData(typeof(int?[]), false)]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(string[]), false)]
    public void ConstructorSetsIsFlag(Type type, bool isFlag)
    {
        var attribute = new CommandOptionAttribute("--option");

        var option = new CommandOption(
            attribute,
            typeof(CommandOptionTest),
            type,
            Builtins.Options.DiscardValue);

        Assert.Equal(isFlag, option.IsFlag);
    }

    [Fact]
    public void ConstructorSetsIsFlagExplicitFalse()
    {
        var attribute = new CommandOptionAttribute("--option");

        var option = new CommandOption(
            attribute,
            typeof(CommandOptionTest),
            typeof(bool),
            Builtins.Options.DiscardValue,
            isFlag: false);

        Assert.False(option.IsFlag);
    }

    [Fact]
    public void ConstructorSetsIsFlagExplicitTrue()
    {
        var attribute = new CommandOptionAttribute("--option");

        var option = new CommandOption(
            attribute,
            typeof(CommandOptionTest),
            typeof(string),
            Builtins.Options.DiscardValue,
            isFlag: true);

        Assert.True(option.IsFlag);
    }
}

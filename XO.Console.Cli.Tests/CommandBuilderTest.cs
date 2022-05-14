using Xunit;

namespace XO.Console.Cli.Tests;

public class CommandBuilderTest
{
    [Fact]
    public void AddAliasAddsAlias()
    {
        var builder = new CommandBuilder("verb", typeof(CommandParameters));

        builder.AddAlias("alias");

        var command = builder.Build();

        Assert.Collection(command.Aliases, alias => Assert.Equal("alias", alias));
    }

    [Fact]
    public void AddAliasIgnoresDuplicate()
    {
        var builder = new CommandBuilder("verb", typeof(CommandParameters));

        builder.AddAlias("alias");
        builder.AddAlias("alias");

        var command = builder.Build();

        Assert.Collection(command.Aliases, alias => Assert.Equal("alias", alias));
    }

    [Fact]
    public void AddAliasIgnoresVerb()
    {
        var builder = new CommandBuilder("verb", typeof(CommandParameters));

        builder.AddAlias("verb");

        var command = builder.Build();

        Assert.Empty(command.Aliases);
    }
}

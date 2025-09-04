using XO.Console.Cli.Fixtures;
using XO.Console.Cli.Infrastructure;
using Xunit;

namespace XO.Console.Cli.Commands;

public class HelpCommandTest : CommandAppTestBase
{
    [Fact]
    public async Task HelpCommandOutputs()
    {
        var app = CreateBuilder()
            .Build();

        await app.ExecuteAsync(new[] { "--help" }, TestContext.Current.CancellationToken);

        Assert.Contains("USAGE", this.Console.ReadOutputAsString());
    }

    [Fact]
    public async Task HelpCommandReturns0()
    {
        var app = CreateBuilder()
            .Build();

        var result = await app.ExecuteAsync(new[] { "--help" }, TestContext.Current.CancellationToken);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task HelpCommandReturns0_WithAlias()
    {
        var alias = "nooooop";

        var app = CreateBuilder()
            .AddCommand<TestCommands.NoOp>("noop", builder => builder.AddAlias(alias))
            .Build();

        var result = await app.ExecuteAsync(new[] { alias, "--help" }, TestContext.Current.CancellationToken);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task HelpCommandReturns0_WithParameters()
    {
        var app = CreateBuilder()
            .AddCommand<TestCommandWithParameters>("test")
            .Build();

        var result = await app.ExecuteAsync(new[] { "test", "foo", "--help" }, TestContext.Current.CancellationToken);

        Assert.Equal(0, result);
    }
}

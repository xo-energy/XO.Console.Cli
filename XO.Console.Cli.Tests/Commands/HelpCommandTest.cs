using XO.Console.Cli.Tests.Fixtures;
using Xunit;

namespace XO.Console.Cli.Commands.Tests;

public class HelpCommandTest : CommandAppTestBase
{
    [Fact]
    public async Task HelpCommandOutputs()
    {
        var app = CreateBuilder()
            .Build();

        await app.ExecuteAsync(new[] { "--help" });

        Assert.Contains("USAGE", this.Console.OutputBuffer.ToString());
    }

    [Fact]
    public async Task HelpCommandReturns0()
    {
        var app = CreateBuilder()
            .Build();

        var result = await app.ExecuteAsync(new[] { "--help" });

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task HelpCommandReturns0_WithAlias()
    {
        var alias = "nooooop";

        var app = CreateBuilder()
            .AddCommand<TestCommands.NoOp>("noop", builder => builder.AddAlias(alias))
            .Build();

        var result = await app.ExecuteAsync(new[] { alias, "--help" });

        Assert.Equal(0, result);
    }
}

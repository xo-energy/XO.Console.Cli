using XO.Console.Cli.Commands;
using XO.Console.Cli.Features;
using Xunit;

namespace XO.Console.Cli.Tests;

public class CommandContextTest
{
    private CommandContext EmptyContext { get; }
        = new CommandContext(new MissingCommand(), new CommandParameters());

    [Fact]
    public void ConsoleReturnsSystemConsole()
    {
        Assert.IsType<SystemConsole>(EmptyContext.Console);
    }

    [Fact]
    public void SystemConsoleInputIsSystemConsoleIn()
    {
        Assert.Same(System.Console.In, EmptyContext.Console.Input);
    }

    [Fact]
    public void SystemConsoleOutputIsSystemConsoleOut()
    {
        Assert.Same(System.Console.Out, EmptyContext.Console.Output);
    }

    [Fact]
    public void SystemConsoleErrorIsSystemConsoleError()
    {
        Assert.Same(System.Console.Error, EmptyContext.Console.Error);
    }
}

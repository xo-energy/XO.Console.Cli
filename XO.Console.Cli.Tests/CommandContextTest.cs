using XO.Console.Cli.Features;
using XO.Console.Cli.Implementation;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;
using Xunit;

namespace XO.Console.Cli;

public class CommandContextTest
{
    private readonly TestTypeResolverScope _scope;

    public CommandContextTest()
    {
        _scope = new TestTypeResolverScope();
        EmptyContext = new CommandContext(_scope, new MissingCommand(), new CommandParameters(), CommandParseResult.Empty);
    }

    private CommandContext EmptyContext { get; }

    [Fact]
    public void CommandServicesReturnsScopeTypeResolver()
    {
        Assert.Same(_scope.TypeResolver, EmptyContext.CommandServices);
    }

    [Fact]
    public void ConsoleReturnsSystemConsole()
    {
        Assert.IsType<SystemConsole>(EmptyContext.Console);
    }

    [Fact]
    public void DisposeDisposesScopeSynchronously()
    {
        EmptyContext.Dispose();
        Assert.True(_scope.IsDisposed);
    }

    [Fact]
    public async Task DisposeAsyncDisposesScopeAsynchronously()
    {
        await EmptyContext.DisposeAsync();
        Assert.True(_scope.IsDisposedAsync);
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

    public sealed class TestTypeResolverScope : ITypeResolverScope
    {
        public bool IsDisposed { get; private set; }
        public bool IsDisposedAsync { get; private set; }

        public ITypeResolver TypeResolver
            => DefaultTypeResolver.Instance;

        public void Dispose()
        {
            IsDisposed = true;
        }

        public ValueTask DisposeAsync()
        {
            IsDisposedAsync = true;
            return default;
        }
    }
}

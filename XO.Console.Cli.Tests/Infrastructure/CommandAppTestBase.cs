using System;
using System.Threading;
using System.Threading.Tasks;

namespace XO.Console.Cli;

public abstract class CommandAppTestBase : IDisposable
{
    private TestConsole? _console;

    protected TestConsole Console
        => LazyInitializer.EnsureInitialized(ref _console);

    protected ICommandAppBuilder CreateBuilder()
        => CommandAppBuilder.Create()
            .UseConsole(this.Console);

    protected ICommandAppBuilder CreateBuilder<TParameters>(
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
        => CommandAppBuilder.WithDefaultCommand<TParameters>(executeAsync)
            .UseConsole(this.Console);

    protected ICommandAppBuilder CreateBuilder<TDefaultCommand>()
        where TDefaultCommand : class, ICommand
        => CommandAppBuilder.WithDefaultCommand<TDefaultCommand>()
            .UseConsole(this.Console);

    public void Dispose()
    {
        _console?.Dispose();
    }
}

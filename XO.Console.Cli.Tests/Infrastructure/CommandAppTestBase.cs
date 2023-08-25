namespace XO.Console.Cli.Infrastructure;

public abstract class CommandAppTestBase : IDisposable
{
    private TestConsole? _console;

    protected TestConsole Console
        => LazyInitializer.EnsureInitialized(ref _console);

    protected ICommandAppBuilder CreateBuilder()
        => new CommandAppBuilder()
            .UseConsole(this.Console);

    protected ICommandAppBuilder CreateBuilder<TParameters>(
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
        => CommandAppBuilder
            .WithDefaultCommand(executeAsync)
            .UseConsole(this.Console);

    protected ICommandAppBuilder CreateBuilder<TDefaultCommand>()
        where TDefaultCommand : class, ICommand
        => CommandAppBuilder
            .WithDefaultCommand<TDefaultCommand>()
            .UseConsole(this.Console);

    public void Dispose()
    {
        _console?.Dispose();
    }
}

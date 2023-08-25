using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli.Commands;

internal sealed class DelegateCommand : AsyncCommand
{
    private readonly Func<ICommandContext, CancellationToken, Task<int>> _executeAsync;

    public DelegateCommand(Func<ICommandContext, CancellationToken, Task<int>> executeAsync)
    {
        _executeAsync = executeAsync;
    }

    public override Task<int> ExecuteAsync(
        ICommandContext context,
        CancellationToken cancellationToken)
    {
        return _executeAsync(context, cancellationToken);
    }
}

internal sealed class DelegateCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TParameters> : AsyncCommand<TParameters>
    where TParameters : CommandParameters
{
    private readonly Func<ICommandContext, TParameters, CancellationToken, Task<int>> _executeAsync;

    public DelegateCommand(Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
    {
        _executeAsync = executeAsync;
    }

    public override Task<int> ExecuteAsync(
        ICommandContext context,
        TParameters parameters,
        CancellationToken cancellationToken)
    {
        return _executeAsync(context, parameters, cancellationToken);
    }
}

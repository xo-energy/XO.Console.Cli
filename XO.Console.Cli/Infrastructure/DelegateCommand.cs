namespace XO.Console.Cli.Infrastructure;

/// <summary>
/// Wraps a delegate command implementation.
/// </summary>
/// <typeparam name="TParameters">A class whose properties describe the command parameters.</typeparam>
public sealed class DelegateCommand<TParameters> : AsyncCommand<TParameters>
    where TParameters : CommandParameters
{
    private readonly Func<ICommandContext, TParameters, CancellationToken, Task<int>> _executeAsync;

    /// <summary>
    /// Initializes a new instance of <see cref="DelegateCommand{TParameters}"/>.
    /// </summary>
    /// <param name="executeAsync">The implementation delegate.</param>
    public DelegateCommand(Func<ICommandContext, CancellationToken, Task<int>> executeAsync)
    {
        _executeAsync = (context, _, cancellationToken) => executeAsync(context, cancellationToken);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DelegateCommand{TParameters}"/>.
    /// </summary>
    /// <param name="executeAsync">The implementation delegate.</param>
    public DelegateCommand(Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
    {
        _executeAsync = executeAsync;
    }

    /// <inheritdoc/>
    public override Task<int> ExecuteAsync(
        ICommandContext context,
        TParameters parameters,
        CancellationToken cancellationToken)
    {
        return _executeAsync(context, parameters, cancellationToken);
    }
}

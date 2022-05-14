namespace XO.Console.Cli;

/// <summary>
/// Base class for synchronous commands without parameters.
/// </summary>
public abstract class Command : ICommand<CommandParameters>
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may request that command execution be canceled.</param>
    /// <returns>The exit code of the command.</returns>
    public abstract int Execute(ICommandContext context, CancellationToken cancellationToken);

    /// <inheritdoc/>
    Task<int> ICommand.ExecuteAsync(
        ICommandContext context,
        CommandParameters _,
        CancellationToken cancellationToken)
    {
        var result = Execute(context, cancellationToken);
        return Task.FromResult(result);
    }
}

/// <summary>
/// Base class for synchronous commands.
/// </summary>
public abstract class Command<TParameters> : ICommand<TParameters>
    where TParameters : CommandParameters
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="parameters">The parameters bound from the command-line arguments.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may request that command execution be canceled.</param>
    /// <returns>The exit code of the command.</returns>
    public abstract int Execute(ICommandContext context, TParameters parameters, CancellationToken cancellationToken);

    /// <inheritdoc/>
    Task<int> ICommand.ExecuteAsync(
        ICommandContext context,
        CommandParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = Execute(context, (TParameters)parameters, cancellationToken);
        return Task.FromResult(result);
    }
}

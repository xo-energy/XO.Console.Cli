using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli;

/// <summary>
/// Base class for commands without parameters.
/// </summary>
public abstract class AsyncCommand : ICommand<CommandParameters>
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may request that command execution be canceled.</param>
    /// <returns>A <see cref="Task{T}"/> whose result is the exit code of the command.</returns>
    public abstract Task<int> ExecuteAsync(ICommandContext context, CancellationToken cancellationToken);

    /// <inheritdoc/>
    Task<int> ICommand.ExecuteAsync(
        ICommandContext context,
        CommandParameters _,
        CancellationToken cancellationToken)
        => ExecuteAsync(context, cancellationToken);
}

/// <summary>
/// Base class for commands with parameters.
/// </summary>
public abstract class AsyncCommand<TParameters> : ICommand<TParameters>
    where TParameters : CommandParameters
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="parameters">The parameters bound from the command-line arguments.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may request that command execution be canceled.</param>
    /// <returns>A <see cref="Task{T}"/> whose result is the exit code of the command.</returns>
    public abstract Task<int> ExecuteAsync(ICommandContext context, TParameters parameters, CancellationToken cancellationToken);

    /// <inheritdoc/>
    Task<int> ICommand.ExecuteAsync(
        ICommandContext context,
        CommandParameters parameters,
        CancellationToken cancellationToken)
        => ExecuteAsync(context, (TParameters)parameters, cancellationToken);
}

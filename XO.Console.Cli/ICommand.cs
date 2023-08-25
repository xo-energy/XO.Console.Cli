using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli;

/// <summary>
/// Represents a command.
/// </summary>
/// <remarks>
/// This interface cannot be implemented directly. These abstract classes are provided for implementing commands:
/// <list type="bullet">
///     <item><see cref="AsyncCommand"/>: asynchronous command, no parameters</item>
///     <item><see cref="AsyncCommand{TParameters}"/>: asynchronous command with parameters</item>
///     <item><see cref="Command"/>: synchronous command, no parameters</item>
///     <item><see cref="Command{TParameters}"/>: synchronous command with parameters</item>
/// </list>
/// </remarks>
public interface ICommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <remarks>
    /// This method is <see langword="internal"/>. To implement this interface, consumers must extend one of the
    /// abstract command classes. Therefore, we can guarantee such implementations also implement
    /// <see cref="ICommand{TParameters}"/>.
    /// </remarks>
    /// <param name="context">The command execution context.</param>
    /// <param name="parameters">The parameters bound from the command-line arguments.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may request that command execution be canceled.</param>
    /// <returns>A <see cref="Task{T}"/> whose result is the exit code of the command.</returns>
    internal Task<int> ExecuteAsync(
        ICommandContext context,
        CommandParameters parameters,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a command with parameters.
/// </summary>
/// <remarks>
/// This interface cannot be implemented directly. These abstract classes are provided for implementing commands:
/// <list type="bullet">
///     <item><see cref="AsyncCommand{TParameters}"/>: asynchronous command with parameters</item>
///     <item><see cref="Command{TParameters}"/>: synchronous command with parameters</item>
/// </list>
/// </remarks>
/// <typeparam name="TParameters">A class whose properties describe the command parameters.</typeparam>
public interface ICommand<TParameters> : ICommand
    where TParameters : CommandParameters
{ }

namespace XO.Console.Cli;

/// <summary>
/// Represents a middleware service for <see cref="ICommandApp"/>.
/// </summary>
public interface ICommandAppMiddleware
{
    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="next">The next delegate in the execution pipeline. Implementors must call this method to continue executing the command.</param>
    /// <param name="context">The command execution context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may request that execution be canceled.</param>
    /// <returns>
    /// An exit code that describes the result of executing the command. Usually, this will be the return value of
    /// <paramref name="next"/>, but implementors may choose to return other values as necessary.
    /// </returns>
    Task<int> ExecuteAsync(ExecutorDelegate next, CommandContext context, CancellationToken cancellationToken);
}

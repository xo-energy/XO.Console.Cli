using System.Diagnostics;

namespace XO.Console.Cli.Middleware;

/// <summary>
/// A middleware that handles exceptions and writes the exception message to <c>stderr</c>.
/// </summary>
public class ExceptionHandlerMiddleware
{
    private readonly ExecutorDelegate _next;

    /// <summary>
    /// Initializes a new instance of <see cref="ExceptionHandlerMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public ExceptionHandlerMiddleware(ExecutorDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Executes the middleware pipeline.
    /// </summary>
    /// <param name="context">The <see cref="CommandContext"/> for the current execution.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that, when canceled, indicates the process is stopping.</param>
    /// <returns>A <see cref="Task"/> whose result is the exit code of the command.</returns>
    [DebuggerNonUserCode]
    public async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        try
        {
            return await _next(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            context.Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}

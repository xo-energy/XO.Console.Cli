using XO.Console.Cli.Model;

namespace XO.Console.Cli;

/// <summary>
/// Represents a delegate that executes a command.
/// </summary>
/// <param name="context">The <see cref="CommandContext"/> for the current execution.</param>
/// <param name="cancellationToken">A <see cref="CancellationToken"/> that, when canceled, indicates the process is stopping.</param>
/// <returns>A <see cref="Task"/> whose result is the exit code of the command.</returns>
public delegate Task<int> ExecutorDelegate(
    CommandContext context,
    CancellationToken cancellationToken);

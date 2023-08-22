namespace XO.Console.Cli;

/// <summary>
/// Extension methods for <see cref="ICommandAppBuilder"/>.
/// </summary>
public static class CommandAppBuilderExtensions
{
    /// <summary>
    /// Builds the <see cref="ICommandApp"/> and executes a command.
    /// </summary>
    /// <param name="builder">The <see cref="ICommandAppBuilder"/> to build.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may request that command execution be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the exit code of the command.</returns>
    public static async Task<int> ExecuteAsync(
        this ICommandAppBuilder builder,
        IReadOnlyList<string> args,
        CancellationToken cancellationToken = default)
    {
        var app = builder.Build();

        return await app.ExecuteAsync(
            args,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}

namespace XO.Console.Cli;

/// <summary>
/// Represents a configured command-line application.
/// </summary>
public interface ICommandApp
{
    /// <summary>
    /// Parses a sequence of command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The list of tokens recognized from the arguments and any errors encountered while parsing.</returns>
    CommandParseResult Parse(IReadOnlyList<string> args);

    /// <summary>
    /// Binds parsed command-line arguments to the appropriate command and parameters types.
    /// </summary>
    /// <param name="parse">The result of parsing the command-line arguments.</param>
    /// <returns>
    /// A new instance of <see cref="CommandContext"/> containing the bound command implementation and parameters.
    /// </returns>
    /// <exception cref="CommandParameterBindingException">An error occurred while binding parameter values.</exception>
    /// <exception cref="CommandParameterValidationException"><see cref="CommandParameters.Validate"/> did not succeed.</exception>
    /// <exception cref="CommandParsingException">The provided <see cref="CommandParseResult"/> contained errors, and strict parsing is enabled.</exception>
    /// <exception cref="CommandTypeException">An error occurred while instantiating the command implementation or its parameters.</exception>
    CommandContext Bind(CommandParseResult parse);

    /// <summary>
    /// Executes a command.
    /// </summary>
    /// <param name="context">The bound <see cref="CommandContext"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that, when canceled, indicates the process is stopping.</param>
    /// <returns>A <see cref="Task"/> whose result is the exit code of the command.</returns>
    Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default);
}

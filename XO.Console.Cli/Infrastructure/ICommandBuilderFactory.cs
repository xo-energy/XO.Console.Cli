namespace XO.Console.Cli.Infrastructure;

/// <summary>
/// Represents a factory that creates <see cref="CommandBuilder"/> instances.
/// </summary>
/// <remarks>
/// This type supports the <c>XO.Console.Cli</c> infrastructure. It is not intended to be used directly from your code.
/// </remarks>
public interface ICommandBuilderFactory
{
    /// <summary>
    /// Adds commands configured using <see cref="CommandAttribute"/> to the <see cref="ICommandAppBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ICommandAppBuilder"/> to configure.</param>
    void ConfigureCommandApp(ICommandAppBuilder builder);

    /// <summary>
    /// Creates an instance of <see cref="CommandBuilder"/> for the specified command type.
    /// </summary>
    /// <typeparam name="TCommand">The command implementation type.</typeparam>
    /// <param name="verb">The verb that invokes the command.</param>
    /// <returns>If this factory supports commands of type <typeparamref name="TCommand"/>, a new instance of <see cref="CommandBuilder"/>; otherwise, <see langword="null"/>.</returns>
    CommandBuilder? CreateCommandBuilder<TCommand>(string verb)
        where TCommand : class, ICommand;
}

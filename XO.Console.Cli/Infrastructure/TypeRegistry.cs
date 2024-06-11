using XO.Console.Cli.Implementation;
using XO.Console.Cli.Model;

namespace XO.Console.Cli.Infrastructure;

/// <summary>
/// Handles registration and invocation of source-generated command and parameters factories.
/// </summary>
/// <remarks>
/// This type supports the <c>XO.Console.Cli</c> infrastructure. It is not intended to be used directly from your code.
/// </remarks>
public static class TypeRegistry
{
    private static readonly List<ICommandBuilderFactory> _commandBuilderFactories = new();
    private static readonly List<ICommandParametersFactory> _commandParametersFactories = new() { BuiltinCommandParametersFactory.Instance };

    /// <summary>
    /// Gets the name of the entry assembly (detected via source generation).
    /// </summary>
    public static string? EntryAssemblyName { get; private set; }

    /// <summary>
    /// Gets the description of the entry assembly (detected via source generation).
    /// </summary>
    public static string? EntryAssemblyDescription { get; private set; }

    /// <summary>
    /// Gets the version of the entry assembly (detected via source generation).
    /// </summary>
    public static string? EntryAssemblyVersion { get; private set; }

    /// <summary>
    /// Adds commands configured using <see cref="CommandAttribute"/> to the application.
    /// </summary>
    /// <param name="builder">The application's root<see cref="CommandBuilder"/>.</param>
    public static void AddCommandAttributeCommands(CommandBuilder builder)
    {
        foreach (var factory in _commandBuilderFactories)
            factory.AddCommandAttributeCommands(builder);
    }

    /// <summary>
    /// Creates an instance of <see cref="CommandBuilder"/> for the specified command type.
    /// </summary>
    /// <typeparam name="TCommand">The command implementation type.</typeparam>
    /// <param name="verb">The verb that invokes the command.</param>
    /// <returns>A new instance of <see cref="CommandBuilder"/>.</returns>
    /// <exception cref="CommandTypeException">No registered <see cref="ICommandBuilderFactory"/> supports the specified type.</exception>
    public static CommandBuilder CreateCommandBuilder<TCommand>(
        string verb)
        where TCommand : class, ICommand
    {
        foreach (var factory in _commandBuilderFactories)
        {
            if (factory.CreateCommandBuilder<TCommand>(verb) is { } builder)
                return builder;
        }

        throw new CommandTypeException(
            typeof(TCommand),
            $"No {nameof(ICommandBuilderFactory)} registered for {typeof(TCommand)}");
    }

    /// <summary>
    /// Describes the parameters declared by the specified type.
    /// </summary>
    /// <param name="parametersType">The command parameters type.</param>
    /// <returns>An instance of <see cref="CommandParametersInfo"/> that describes the specified type.</returns>
    /// <exception cref="CommandTypeException">No registered <see cref="ICommandParametersFactory"/> supports the specified type.</exception>
    public static CommandParametersInfo DescribeParameters(Type parametersType)
    {
        foreach (var factory in _commandParametersFactories)
        {
            if (factory.DescribeParameters(parametersType) is { } parameters)
                return parameters;
        }

        throw new CommandTypeException(
            parametersType,
            $"No {nameof(ICommandParametersFactory)} registered for {parametersType}");
    }

    /// <summary>
    /// Registers a new <see cref="ICommandBuilderFactory"/>.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    public static void RegisterCommandBuilderFactory(ICommandBuilderFactory factory)
    {
        _commandBuilderFactories.Add(factory);
    }

    /// <summary>
    /// Registers a new <see cref="ICommandParametersFactory"/>.
    /// </summary>
    /// <param name="factory">The factory instance.</param>
    public static void RegisterCommandParametersFactory(ICommandParametersFactory factory)
    {
        _commandParametersFactories.Add(factory);
    }

    /// <summary>
    /// Sets the entry assembly properties.
    /// </summary>
    public static void SetEntryAssemblyProperties(string? assemblyName, string? assemblyDescription, string? assemblyVersion)
    {
        EntryAssemblyName = assemblyName;
        EntryAssemblyDescription = assemblyDescription;
        EntryAssemblyVersion = assemblyVersion;
    }
}

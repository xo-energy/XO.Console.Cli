using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli.Infrastructure;

/// <summary>
/// Stores the configuration for a command and its parameters.
/// </summary>
public sealed class ConfiguredCommand
{
    private readonly string _verb;
    private readonly Func<ITypeResolver, ICommand?> _factory;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    private readonly Type _parametersType;

    /// <summary>
    /// Initializes a new instance of <see cref="ConfiguredCommand"/>.
    /// </summary>
    /// <param name="verb">The verb that invokes this command.</param>
    /// <param name="factory">A factory delegate that creates instance(s) of the command implementation type.</param>
    /// <param name="parametersType">The type of the command's parameters.</param>
    /// <remarks>
    /// <see cref="ConfiguredCommand"/> supports the <c>XO.Console.Cli</c> infrastructure and is not intended to be used
    /// directly by consumers. <paramref name="parametersType"/> is checked at application startup for
    /// assignment-compatibility with the parent command's parameters type, and executing the command returned by the
    /// <paramref name="factory"/> delegate will throw an exception if <paramref name="parametersType"/> does not match
    /// its <see cref="ICommand{TParameters}"/> implementation. Use the <see cref="ICommandBuilder"/> interface for
    /// type-checked command configuration.
    /// </remarks>
    public ConfiguredCommand(
        string verb,
        Func<ITypeResolver, ICommand?> factory,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type parametersType)
    {
        _verb = verb;
        _factory = factory;
        _parametersType = parametersType;
    }

    /// <summary>
    /// The collection of aliases that can be used as alternatives to <see cref="Verb"/>.
    /// </summary>
    public ImmutableHashSet<string> Aliases { get; init; }
        = ImmutableHashSet<string>.Empty;

    /// <summary>
    /// The collection of children of this command.
    /// </summary>
    public ImmutableList<ConfiguredCommand> Commands { get; init; }
        = ImmutableList<ConfiguredCommand>.Empty;

    /// <summary>
    /// A description of the command, which is displayed in generated command help.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Indicates whether the command is hidden from generated command help.
    /// </summary>
    public bool IsHidden { get; init; }

    /// <summary>
    /// The type of the command's parameters.
    /// </summary>
    public Type ParametersType => _parametersType;

    /// <summary>
    /// The verb that invokes this command.
    /// </summary>
    public string Verb => _verb;

    /// <summary>
    /// Creates an instance of the command implementation type.
    /// </summary>
    /// <param name="resolver">The <see cref="ITypeResolver"/> that will instantiate the command.</param>
    /// <returns>A new instance of <see cref="ICommand"/>.</returns>
    public ICommand CreateCommand(ITypeResolver resolver)
        => _factory(resolver)
        ?? throw new CommandTypeException(_factory.Method.ReturnType, "Failed to instantiate command!");

    /// <summary>
    /// Creates an instance of the command's parameters type.
    /// </summary>
    /// <param name="resolver">The <see cref="ITypeResolver"/> that will instantiate the command.</param>
    /// <returns>A new instance of <see cref="CommandParameters"/>.</returns>
    public CommandParameters CreateParameters(ITypeResolver resolver)
        => resolver.Get(_parametersType) as CommandParameters
        ?? throw new CommandTypeException(_parametersType, "Failed to instantiate parameters!");

    /// <summary>
    /// Tests whether the given verb matches this command.
    /// </summary>
    /// <param name="verb">The verb to test.</param>
    /// <returns>If <paramref name="verb"/> is equal to <see cref="Verb"/> or contained in <see cref="Aliases"/>, <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
    public bool IsMatch(string verb)
    {
        return this.Verb == verb
            || this.Aliases.Contains(verb);
    }
}

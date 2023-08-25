using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli.Model;

/// <summary>
/// Stores the configuration for a command and its parameters.
/// </summary>
public abstract class ConfiguredCommand
{
    /// <summary>
    /// Initializes a new instance of <see cref="ConfiguredCommand"/>.
    /// </summary>
    /// <param name="verb">The verb that invokes this command.</param>
    public ConfiguredCommand(string verb)
    {
        this.Verb = verb;
    }

    /// <summary>
    /// The collection of aliases that can be used as alternatives to <see cref="Verb"/>.
    /// </summary>
    public ImmutableHashSet<string> Aliases { get; init; }
        = ImmutableHashSet<string>.Empty;

    /// <summary>
    /// The collection of children of this command.
    /// </summary>
    public IImmutableList<ConfiguredCommand> Commands { get; init; }
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
    public abstract Type ParametersType { get; }

    /// <summary>
    /// The verb that invokes this command.
    /// </summary>
    public string Verb { get; }

    /// <summary>
    /// Creates an instance of the command implementation type.
    /// </summary>
    /// <param name="resolver">The <see cref="ITypeResolver"/> that will instantiate the command.</param>
    /// <returns>A new instance of <see cref="ICommand"/>.</returns>
    public abstract ICommand CreateCommand(ITypeResolver resolver);

    /// <summary>
    /// Creates an instance of the command's parameters type.
    /// </summary>
    /// <param name="resolver">The <see cref="ITypeResolver"/> that will instantiate the command.</param>
    /// <returns>A new instance of <see cref="CommandParameters"/>.</returns>
    public abstract CommandParameters CreateParameters(ITypeResolver resolver);

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

/// <summary>
/// Stores the configuration for a command and its parameters.
/// </summary>
/// <typeparam name="TCommand">The command implementation type.</typeparam>
/// <typeparam name="TParameters">The command parameters type.</typeparam>
public sealed class ConfiguredCommand<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>
    : ConfiguredCommand
    where TCommand : ICommand<TParameters>
    where TParameters : CommandParameters
{
    private readonly Func<ITypeResolver, TCommand?> _commandFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="ConfiguredCommand{TCommand, TParameters}"/>.
    /// </summary>
    /// <param name="verb">The verb that invokes this command.</param>
    /// <param name="factory">A factory delegate that creates instance(s) of the command implementation type.</param>
    public ConfiguredCommand(string verb, Func<ITypeResolver, TCommand?> factory)
        : base(verb)
    {
        _commandFactory = factory;
    }

    /// <inheritdoc/>
    public override Type ParametersType
        => typeof(TParameters);

    /// <returns>A new instance of <typeparamref name="TCommand"/>.</returns>
    /// <inheritdoc/>
    public override ICommand CreateCommand(ITypeResolver resolver)
        => _commandFactory(resolver)
        ?? throw new CommandTypeException(typeof(TCommand), "Failed to instantiate command!");

    /// <returns>A new instance of <typeparamref name="TParameters"/>.</returns>
    /// <inheritdoc/>
    public override CommandParameters CreateParameters(ITypeResolver resolver)
        => resolver.Get<TParameters>()
        ?? throw new CommandTypeException(typeof(TParameters), "Failed to instantiate parameters!");
}

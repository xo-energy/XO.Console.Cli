using System.Diagnostics.CodeAnalysis;

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
    /// Creates an instance of <see cref="CommandBuilder"/> for the specified command type.
    /// </summary>
    /// <typeparam name="TCommand">The command implementation type.</typeparam>
    /// <param name="verb">The verb that invokes the command.</param>
    /// <returns>If this factory supports commands of type <typeparamref name="TCommand"/>, a new instance of <see cref="CommandBuilder"/>; otherwise, <see langword="null"/>.</returns>
    CommandBuilder? CreateCommandBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>(string verb)
        where TCommand : class, ICommand;
}

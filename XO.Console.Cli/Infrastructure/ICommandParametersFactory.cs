using XO.Console.Cli.Model;

namespace XO.Console.Cli.Infrastructure;

/// <summary>
/// Represents a factory that describes command parameters types.
/// </summary>
/// <remarks>
/// This type supports the <c>XO.Console.Cli</c> infrastructure. It is not intended to be used directly from your code.
/// </remarks>
public interface ICommandParametersFactory
{
    /// <summary>
    /// Describes the parameters declared by the specified type.
    /// </summary>
    /// <param name="parametersType">The command parameters type.</param>
    /// <returns>If this factory supports commands of type <paramref name="parametersType"/>, a new instance of <see cref="CommandParametersInfo"/>; otherwise, <see langword="null"/>.</returns>
    CommandParametersInfo? DescribeParameters(Type parametersType);
}

using Microsoft.Extensions.Hosting;

namespace XO.Console.Cli;

/// <summary>
/// Configures the <see cref="ICommandAppBuilder"/> via <see cref="CommandAppHostBuilderExtensions.ConfigureCommandApp"/>.
/// </summary>
public sealed class CommandAppBuilderOptions
{
    /// <summary>
    /// A list of delegates that configure <see cref="ICommandAppBuilder"/>.
    /// </summary>
    public List<Action<HostBuilderContext, ICommandAppBuilder>> ConfigureActions { get; }
        = new();
}

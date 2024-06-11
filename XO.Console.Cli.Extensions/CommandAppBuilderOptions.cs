using Microsoft.Extensions.Hosting;

namespace XO.Console.Cli;

/// <summary>
/// Configures the <see cref="ICommandAppBuilder"/>.
/// </summary>
public sealed class CommandAppBuilderOptions
{
    /// <summary>
    /// A delegate that will be called to create the <see cref="ICommandAppBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Used to configure the factory method that will be called to initialize the <see cref="ICommandAppBuilder"/>. The
    /// default factory method is the default constructor. To configure a default command, set an appropriate factory
    /// method; for example, <see cref="CommandAppBuilder.WithDefaultCommand{TCommand}()"/>.
    /// </remarks>
    public Func<ICommandAppBuilder> CommandAppBuilderFactory { get; set; }
        = static () => new CommandAppBuilder();

    /// <summary>
    /// A list of delegates that configure <see cref="ICommandAppBuilder"/>.
    /// </summary>
    public List<Action<HostBuilderContext, ICommandAppBuilder>> ConfigureActions { get; }
        = new();
}

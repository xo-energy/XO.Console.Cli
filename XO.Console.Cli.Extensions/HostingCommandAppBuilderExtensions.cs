using Microsoft.Extensions.Hosting;

namespace XO.Console.Cli;

/// <summary>
/// Extension methods for adding <c>Microsoft.Extensions.Hosting</c>-related features to
/// <see cref="ICommandAppBuilder"/>.
/// </summary>
public static class HostingCommandAppBuilderExtensions
{
    /// <summary>
    /// Adds global options to the <see cref="ICommandAppBuilder"/> for configuring the generic host.
    /// </summary>
    /// <remarks>
    /// The options added by this method do not anything by themselves — they serve only as a source of documentation
    /// and &quot;allowlist&quot; entries for command parsing. You must separately configure the
    /// <see cref="IHostBuilder"/> to parse and implement their values.
    /// </remarks>
    /// <param name="builder">The <see cref="ICommandAppBuilder"/> to configure.</param>
    /// <returns>The <see cref="ICommandAppBuilder"/>.</returns>
    public static ICommandAppBuilder AddHostingGlobalOptions(
        this ICommandAppBuilder builder)
    {
        return builder
            .AddGlobalOption<string[]>("--configuration", "Adds an additional configuration file", "-c")
            .AddGlobalOption<string>("--environment", "Sets the hosting environment")
            ;
    }
}

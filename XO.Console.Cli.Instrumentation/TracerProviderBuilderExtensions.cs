using Microsoft.Extensions.DependencyInjection;
using XO.Console.Cli;
using XO.Console.Cli.Instrumentation;

namespace OpenTelemetry.Trace;

/// <summary>
/// <c>XO.Console.Cli</c> extension methods for <see cref="TracerProviderBuilder"/>.
/// </summary>
public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Enables <see cref="ICommandApp"/> instrumentation.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/> to configure.</param>
    /// <param name="configure">A delegate that configures the <see cref="CommandAppInstrumentationOptions"/>.</param>
    /// <returns>The same <see cref="TracerProviderBuilder"/> instance.</returns>
    public static TracerProviderBuilder AddCommandAppInstrumentation(
        this TracerProviderBuilder builder,
        Action<CommandAppInstrumentationOptions>? configure = default)
    {
        return builder
            .AddSource(CommandAppInstrumentationMiddleware.ActivitySourceName)
            .ConfigureServices(services =>
            {
                var optionsBuilder = services.AddOptions<CommandAppInstrumentationOptions>();

                if (configure != null)
                    optionsBuilder.Configure(configure);

                services.AddCommandAppMiddleware<CommandAppInstrumentationMiddleware>();
            })
            ;
    }
}

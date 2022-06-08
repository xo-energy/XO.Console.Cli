using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace XO.Console.Cli;

/// <summary>
/// Extension methods for running command-line applications using <c>Microsoft.Extensions.Hosting</c>.
/// </summary>
public static class CommandAppHostExtensions
{
    /// <summary>
    /// Builds and runs a hosted command-line application with a default command.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/>.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="configure">A delegate that configures the <see cref="ICommandAppBuilder"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    public static Task<int> RunCommandAppAsync<TDefaultCommand>(
        this IHost host,
        IReadOnlyList<string> args,
        Action<ICommandAppBuilder>? configure = null)
        where TDefaultCommand : class, ICommand
        => RunCommandAppAsync(host, args, CommandAppBuilder.WithDefaultCommand<TDefaultCommand>, configure);

    /// <summary>
    /// Builds and runs a hosted command-line application.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/>.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="configure">A delegate that configures the <see cref="ICommandAppBuilder"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    public static Task<int> RunCommandAppAsync(
        this IHost host,
        IReadOnlyList<string> args,
        Action<ICommandAppBuilder>? configure = null)
        => RunCommandAppAsync(host, args, CommandAppBuilder.Create, configure);

    /// <summary>
    /// Builds and runs a hosted command-line application.
    /// </summary>
    /// <param name="host">The <see cref="IHost"/>.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="builderFactory">A delegate that constructs the <see cref="ICommandAppBuilder"/>.</param>
    /// <param name="configure">A delegate that configures the <see cref="ICommandAppBuilder"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    [DebuggerNonUserCode]
    internal static async Task<int> RunCommandAppAsync(
        this IHost host,
        IReadOnlyList<string> args,
        Func<ICommandAppBuilder> builderFactory,
        Action<ICommandAppBuilder>? configure = null)
    {
        using var cancellationSource = new CancellationTokenSource();
        var logger = host.Services.GetService<ILogger<ICommandApp>>();
        var token = cancellationSource.Token;
        int result;
        try
        {
            var context = host.Services.GetRequiredService<HostBuilderContext>();
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            var resolver = new ServiceProviderTypeResolver(host.Services);

            var builder = builderFactory()
                .AddHostingGlobalOptions()
                .SetApplicationName(context.HostingEnvironment.ApplicationName)
                .UseTypeResolver(resolver);

            var optionsAccessor = host.Services.GetService<IOptions<CommandAppBuilderOptions>>();
            if (optionsAccessor?.Value is CommandAppBuilderOptions options)
            {
                foreach (var action in options.ConfigureActions)
                    action(context, builder);
            }

            configure?.Invoke(builder);

            var app = builder.Build();
            var parse = app.Parse(args);

            await host.StartAsync(token).ConfigureAwait(false);

            try
            {
                logger?.LogDebug("Executing command {Command} ... ", from x in parse.GetVerbs() select x.Value);
                logger?.LogTrace("Command args: {Args}", args);

                result = await app.ExecuteAsync(parse, lifetime.ApplicationStopping)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, "Command crashed! {Message}", ex.Message);

                System.Console.Error.WriteLine(ex.Message);
                result = 1;
            }
            finally
            {
                lifetime.StopApplication();
            }

            await host.WaitForShutdownAsync(token)
                .ConfigureAwait(false);
        }
        finally
        {
            cancellationSource.Cancel();
        }

        return result;
    }
}

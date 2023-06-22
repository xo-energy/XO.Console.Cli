using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace XO.Console.Cli;

/// <summary>
/// Extension methods for running command-line applications using <c>Microsoft.Extensions.Hosting</c>.
/// </summary>
public static class CommandAppHostExtensions
{
    /// <summary>
    /// Runs the configured command-line application.
    /// </summary>
    /// <remarks>
    /// To configure the command-line application, call <see
    /// cref="o:CommandAppServiceCollectionExtensions.AddCommandApp()"/> before building the host.
    /// </remarks>
    /// <param name="host">The <see cref="IHost"/>.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    [DebuggerNonUserCode]
    public static async Task<int> RunCommandAppAsync(this IHost host, IReadOnlyList<string> args)
    {
        using var cancellationSource = new CancellationTokenSource();
        var logger = host.Services.GetService<ILogger<ICommandApp>>();
        var token = cancellationSource.Token;
        int result;
        try
        {
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            var app = host.Services.GetService<ICommandApp>();

            // if the caller did not configure an application, do nothing as a default
            app ??= CommandAppFactory.BuildCommandApp(host.Services);

            var parse = app.Parse(args);

            await host.StartAsync(token).ConfigureAwait(false);

            try
            {
                logger?.LogDebug("Executing command {Command} ... ", from x in parse.GetVerbs() select x.Value);

                result = await app.ExecuteAsync(parse, lifetime.ApplicationStopping)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, "An unhandled exception occurred while executing the command.");
                throw;
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

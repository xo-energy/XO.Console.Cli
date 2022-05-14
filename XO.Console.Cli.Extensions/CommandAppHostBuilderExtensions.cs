using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace XO.Console.Cli;

/// <summary>
/// Extension methods for building command-line applications using <c>Microsoft.Extensions.Hosting</c>.
/// </summary>
public static class CommandAppHostBuilderExtensions
{
    /// <summary>
    /// Configures <see cref="CommandAppBuilderOptions"/> and adds a delegate to its list of configuration actions.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="configure">A delegate that configures <see cref="ICommandAppBuilder"/>.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    public static IHostBuilder ConfigureCommandApp(
        this IHostBuilder builder,
        Action<HostBuilderContext, ICommandAppBuilder> configure)
    {
        return builder
            .ConfigureServices((_, services) =>
            {
                services.AddOptions<CommandAppBuilderOptions>()
                    .Configure(options => options.ConfigureActions.Add(configure))
                    ;
            })
            ;
    }

    /// <summary>
    /// Builds the host, then builds and runs a hosted command-line application with a default command.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="configure">A delegate that configures the <see cref="ICommandAppBuilder"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    public static Task<int> RunCommandAppAsync<TDefaultCommand>(
        this IHostBuilder hostBuilder,
        IReadOnlyList<string> args,
        Action<ICommandAppBuilder>? configure = null)
        where TDefaultCommand : class, ICommand
        => RunCommandAppAsync(hostBuilder, args, CommandAppBuilder.WithDefaultCommand<TDefaultCommand>, configure);

    /// <summary>
    /// Builds the host, then builds and runs a hosted command-line application.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="configure">A delegate that configures the <see cref="ICommandAppBuilder"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    public static Task<int> RunCommandAppAsync(
        this IHostBuilder hostBuilder,
        IReadOnlyList<string> args,
        Action<ICommandAppBuilder>? configure = null)
        => RunCommandAppAsync(hostBuilder, args, CommandAppBuilder.Create, configure);

    /// <summary>
    /// Builds the host, then builds and runs a hosted command-line application.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="builderFactory">A delegate that constructs the <see cref="ICommandAppBuilder"/>.</param>
    /// <param name="configure">A delegate that configures the <see cref="ICommandAppBuilder"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    [DebuggerNonUserCode]
    internal static async Task<int> RunCommandAppAsync(
        this IHostBuilder hostBuilder,
        IReadOnlyList<string> args,
        Func<ICommandAppBuilder> builderFactory,
        Action<ICommandAppBuilder>? configure)
    {
        IHost host;
        try
        {
            host = hostBuilder.Build();
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(ex.Message);
            return -1;
        }

        // get a reference to the logger factory
        var loggerFactory = host.Services.GetService<ILoggerFactory>();

        // run the host
        int result;
        try
        {
            result = await host.RunCommandAppAsync(args, builderFactory, configure)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(ex.Message);
            result = -1;
        }
        finally
        {
            await DisposeAndFlush(host, loggerFactory)
                .ConfigureAwait(false);
        }

        return result;
    }

    private static async Task DisposeAndFlush(IHost host, ILoggerFactory? loggerFactory)
    {
        try
        {
            if (host is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync()
                    .ConfigureAwait(false);
            else
                host.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        finally
        {
            loggerFactory?.Dispose();
        }
    }
}

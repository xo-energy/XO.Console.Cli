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
    /// Builds the host, then builds and runs a hosted command-line application.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="configure">A delegate that configures the <see cref="ICommandAppBuilder"/>.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    public static Task<int> RunCommandAppAsync(
        this IHostBuilder hostBuilder,
        IReadOnlyList<string> args,
        Action<HostBuilderContext, ICommandAppBuilder>? configure = null)
        => RunCommandAppAsync(hostBuilder, args, default, configure);

    /// <summary>
    /// Builds the host, then builds and runs a hosted command-line application with a default command.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="configure">A delegate that configures the <see cref="ICommandAppBuilder"/>.</param>
    /// <typeparam name="TDefaultCommand">The command implementation type.</typeparam>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    public static Task<int> RunCommandAppAsync<TDefaultCommand>(
        this IHostBuilder hostBuilder,
        IReadOnlyList<string> args,
        Action<HostBuilderContext, ICommandAppBuilder>? configure = null)
        where TDefaultCommand : class, ICommand
        => RunCommandAppAsync(hostBuilder, args, CommandAppBuilder.WithDefaultCommand<TDefaultCommand>, configure);

    /// <summary>
    /// Builds the host, then builds and runs a hosted command-line application with a default command.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="executeAsync">The command implementation delegate.</param>
    /// <param name="configure">A delegate that configures the <see cref="ICommandAppBuilder"/>.</param>
    /// <typeparam name="TParameters">A class whose properties describe the command parameters.</typeparam>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the command exit code.</returns>
    public static Task<int> RunCommandAppAsync<TParameters>(
        this IHostBuilder hostBuilder,
        IReadOnlyList<string> args,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync,
        Action<HostBuilderContext, ICommandAppBuilder>? configure = null)
        where TParameters : CommandParameters
        => RunCommandAppAsync(hostBuilder, args, () => CommandAppBuilder.WithDefaultCommand(executeAsync), configure);

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
        Func<ICommandAppBuilder>? builderFactory,
        Action<HostBuilderContext, ICommandAppBuilder>? configure)
    {
        hostBuilder.ConfigureServices(
            (_, services) => services.AddCommandApp(builderFactory, configure));

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
            result = await host.RunCommandAppAsync(args)
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

    private static ValueTask DisposeAndFlush(IHost host, ILoggerFactory? loggerFactory)
    {
        try
        {
            if (host is IAsyncDisposable asyncDisposable)
                return asyncDisposable.DisposeAsync();

            host.Dispose();
            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return ValueTask.CompletedTask;
        }
        finally
        {
            loggerFactory?.Dispose();
        }
    }
}

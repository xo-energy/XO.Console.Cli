using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace XO.Console.Cli;

/// <summary>
/// Extension methods for adding <see cref="ICommandApp"/>-related services to a service collection.
/// </summary>
public static class CommandAppServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ICommandApp"/> to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configure">A delegate that configures <see cref="ICommandAppBuilder"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCommandApp(
        this IServiceCollection services,
        Action<HostBuilderContext, ICommandAppBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        return services.AddCommandApp(default, configure);
    }

    /// <summary>
    /// Adds <see cref="ICommandApp"/> to the service collection with a default command.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configure">A delegate that configures <see cref="ICommandAppBuilder"/>.</param>
    /// <typeparam name="TDefaultCommand">The command implementation type.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCommandApp<TDefaultCommand>(
        this IServiceCollection services,
        Action<HostBuilderContext, ICommandAppBuilder>? configure = null)
        where TDefaultCommand : class, ICommand
    {
        return services.AddCommandApp(
            CommandAppBuilder.WithDefaultCommand<TDefaultCommand>,
            configure);
    }

    /// <summary>
    /// Adds <see cref="ICommandApp"/> to the service collection with a default command.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="executeAsync">The command implementation delegate.</param>
    /// <param name="configure">A delegate that configures <see cref="ICommandAppBuilder"/>.</param>
    /// <typeparam name="TParameters">A class whose properties describe the command parameters.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddCommandApp<TParameters>(
        this IServiceCollection services,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync,
        Action<HostBuilderContext, ICommandAppBuilder>? configure = null)
        where TParameters : CommandParameters
    {
        ArgumentNullException.ThrowIfNull(executeAsync);

        return services.AddCommandApp(
            () => CommandAppBuilder.WithDefaultCommand(executeAsync),
            configure);
    }

    internal static IServiceCollection AddCommandApp(
        this IServiceCollection services,
        Func<ICommandAppBuilder>? builderFactory,
        Action<HostBuilderContext, ICommandAppBuilder>? configure)
    {
        var optionsBuilder = services.AddOptions<CommandAppBuilderOptions>();

        if (builderFactory != null)
            optionsBuilder.Configure(options => options.CommandAppBuilderFactory = builderFactory);

        if (configure != null)
            optionsBuilder.Configure(options => options.ConfigureActions.Add(configure));

        services.TryAddSingleton(
            static services => CommandAppFactory.BuildCommandApp(services));

        return services;
    }
}

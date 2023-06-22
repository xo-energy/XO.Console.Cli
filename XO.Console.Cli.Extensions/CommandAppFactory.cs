using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace XO.Console.Cli;

internal static class CommandAppFactory
{
    public static ICommandApp BuildCommandApp(IServiceProvider services)
    {
        var context = services.GetRequiredService<HostBuilderContext>();
        var lifetime = services.GetRequiredService<IHostApplicationLifetime>();
        var resolver = new ServiceProviderTypeResolver(services);

        var optionsAccessor = services.GetService<IOptions<CommandAppBuilderOptions>>();
        var options = optionsAccessor?.Value ?? new();

        var builder = options.CommandAppBuilderFactory()
            .SetApplicationName(context.HostingEnvironment.ApplicationName)
            .UseTypeResolver(resolver);

        foreach (var middleware in services.GetServices<ICommandAppMiddleware>())
            builder.UseMiddleware(middleware);

        foreach (var action in options.ConfigureActions)
            action(context, builder);

        return builder.Build();
    }
}

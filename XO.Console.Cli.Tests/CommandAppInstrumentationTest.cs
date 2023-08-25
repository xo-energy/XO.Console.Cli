using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using Xunit;

namespace XO.Console.Cli;

public class CommandAppInstrumentationTest
{
    private static IHostBuilder CreateHostBuilder()
        => new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddOpenTelemetry()
                    .WithTracing(builder =>
                    {
                        builder.AddCommandAppInstrumentation(options =>
                        {
                            options.EnrichWithCommandContext = static (activity, context) =>
                            {
                                activity.SetTag("foo", "bar");
                            };
                        });
                    });
            })
            .UseDefaultServiceProvider(configure =>
            {
                configure.ValidateOnBuild = true;
                configure.ValidateScopes = true;
            });

    [Fact]
    public async Task CanExecuteWithInstrumentation()
    {
        Activity? activity = null;

        var result = await CreateHostBuilder()
            .RunCommandAppAsync<CommandParameters>(
                Array.Empty<string>(),
                (context, _, _) =>
                {
                    activity = Activity.Current;
                    return Task.FromResult(1);
                },
                (context, builder) =>
                {
                });

        Assert.Multiple(
            () => Assert.Equal(1, result),
            () => Assert.Equal(1, activity?.GetTagItem("command.exit_code")),
            () => Assert.Equal("bar", activity?.GetTagItem("foo")));
    }

    [Fact]
    public async Task RecordsException()
    {
        Activity? activity = null;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateHostBuilder()
                .RunCommandAppAsync<CommandParameters>(
                    Array.Empty<string>(),
                    (context, _, _) =>
                    {
                        activity = Activity.Current;
                        throw new InvalidOperationException("boo");
                    },
                    (context, builder) =>
                    {
                    }));

        Assert.Equal(Status.Error.WithDescription(ex.Message), activity.GetStatus());
    }
}

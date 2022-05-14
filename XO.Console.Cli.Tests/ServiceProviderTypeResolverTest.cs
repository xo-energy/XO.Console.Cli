using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace XO.Console.Cli.Tests;

public class ServiceProviderTypeResolverTest
{
    [Fact]
    public void ResolvesCommand()
    {
        using var host = new HostBuilder()
            .Build();

        var resolver = new ServiceProviderTypeResolver(host.Services);

        var command = Assert.IsType<TestCommand>(resolver.Get<TestCommand>());

        Assert.NotNull(command.ServiceProvider);
    }

    private sealed class TestCommand : AsyncCommand
    {
        public TestCommand(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public override Task<int> ExecuteAsync(ICommandContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
